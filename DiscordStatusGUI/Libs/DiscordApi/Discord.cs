using DiscordStatusGUI.Extensions;
using PinkJson;
using System;
using System.Drawing;
using System.IO;
using System.Security;
using System.Text;
using WEBLib;

namespace DiscordStatusGUI.Libs.DiscordApi
{
    class Discord
    {
        //private static readonly string _TempFolder = ProcessEx.GetOutput("cmd", "/c echo %TEMP%").Trim();
        private const string _DiscordAppUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.306 Chrome/78.0.3904.130 Electron/7.1.11 Safari/537.36";
        public const string DiscordApiVersion = "6";
        public string Language = "en-US, en;q=0.9, *;q=0.8";

        public struct AppImages
        {
            private static JsonArray _LastAppAssets;
            private static string _LastAppAssetsResponse;

            public static Bitmap GetImageById(string id, string appId)
            {
                try
                {
                    System.Net.HttpWebRequest request =
                            (System.Net.HttpWebRequest)System.Net.WebRequest.Create(
                                "https://cdn.discordapp.com/app-assets/" + appId + "/" + id);
                    using (System.Net.WebResponse response = request.GetResponse())
                    using (Stream responseStream = response.GetResponseStream())
                        return new System.Drawing.Bitmap(responseStream);
                }
                catch { return new Bitmap(1, 1); }
            }

            public static string GetImageIdByName(string name, string appId)
            {
                var response = WEB.Get("https://discord.com/api/v6/oauth2/applications/" + appId + "/assets");
                if (_LastAppAssetsResponse != response)
                {
                    try
                    {
                        _LastAppAssets = new JsonArray(response);
                        _LastAppAssetsResponse = response;
                    }
                    catch
                    {
                        return null;
                    }
                }

                foreach (JsonArrayObject item in _LastAppAssets)
                {
                    if (item["name"].Value.ToString() == name)
                        return item["id"].Value.ToString();
                }

                return null;
            }

            public static JsonArray GetAppAssets(string appId)
            {
                var response = WEB.Get("https://discord.com/api/v6/oauth2/applications/" + appId + "/assets");
                if (_LastAppAssetsResponse != response)
                {
                    try
                    {
                        _LastAppAssets = new JsonArray(response);
                        _LastAppAssetsResponse = response;
                    }
                    catch
                    {
                        return null;
                    }
                }
                return _LastAppAssets;
            }
        }

        public DiscordSocket Socket;
        public string Email = "";
        public string Password = "";
        public string Phone { get; private set; }
        public string LastError { get; private set; }

        private SecureString _Token = new SecureString();
        public string Token
        {
            get => _Token.Get();
            set => _Token.Set(value);
        }

        private string _Ticket = "";

        public Discord()
        {
            Socket = new DiscordSocket(this);
        }

        public static bool IsTokenValid(string token)
        {
            try
            {
                var result = WEB.Post("https://discord.com/api/v" + DiscordApiVersion + "/users/@me", new string[] { "authorization: " + token }, null, "GET");
                return !result.Contains("message");
            }
            catch
            {
                return false;
            }
        }
        public static Bitmap GetUserAvatar(string userid, string avatarid, int size)
        {
            System.Net.HttpWebRequest request =
                    (System.Net.HttpWebRequest)System.Net.WebRequest.Create(
                        $"https://cdn.discordapp.com/avatars/{userid}/{avatarid}.png?size={size}");
            request.UserAgent = _DiscordAppUserAgent;
            try
            {
                using (System.Net.WebResponse response = request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                {
                    return new Bitmap(responseStream);
                }
            }
            catch
            {
                return new Bitmap(Properties.Resources.DefaultAvatar);
            }
        }

        public AuthErrors Auth()
        {
            var authJson = new Json()
            {
                new JsonObject("email", Email),
                new JsonObject("password", Password),
                new JsonObject("undelete", false),
                new JsonObject("captcha_key", null),
                new JsonObject("login_source", null),
                new JsonObject("gift_code_sku_id", null)
            };

            try
            {
                var resp = WEB.Post("https://discord.com/api/v" + DiscordApiVersion + "/auth/login", new string[] { "Content-Type: application/json", "accept-language: " + Language }, Encoding.UTF8.GetBytes(authJson.ToString()));
                var respJson = new Json(resp);
                if (respJson.IndexByKey("email") != -1 || respJson.IndexByKey("password") != -1)
                {
                    LastError = resp;
                    return AuthErrors.LoginError;
                }
                if (respJson.IndexByKey("captcha_key") != -1)
                {
                    LastError = "Необходим captcha_key, сначала попробуйте войти через браузер";
                    return AuthErrors.Error;
                }
                if (respJson.IndexByKey("message") != -1)
                {
                    LastError = respJson["message"].Value.ToString();
                    return AuthErrors.Error;
                }
                if (respJson["token"].Value is null && (bool)respJson["mfa"].Value == true)
                {
                    _Ticket = respJson["ticket"].Value.ToString();
                    return AuthErrors.MultiFactorAuthentication;
                }
                else
                {
                    Token = respJson["token"].Value.ToString();
                    Email = Password = "";
                    return AuthErrors.Successful;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return AuthErrors.Error;
            }
        }

        public MFAuthErrors MFAuth(int code, MFAuthType type)
        {
            var codeJson = Json.FromAnonymous(new
            {
                code = code,
                ticket = _Ticket,
                login_source = string.Empty,
                gift_code_sku_id = string.Empty
            });

            try
            {
                var resp = "";
                if (type == MFAuthType.Code)
                    resp = WEB.Post("https://discord.com/api/v" + DiscordApiVersion + "/auth/mfa/totp", new string[] { "Content-Type: application/json", "accept-language: " + Language }, Encoding.UTF8.GetBytes(codeJson.ToString()));
                else if (type == MFAuthType.SMS)
                    resp = WEB.Post("https://discord.com/api/v" + DiscordApiVersion + "/auth/mfa/sms", new string[] { "Content-Type: application/json", "accept-language: " + Language }, Encoding.UTF8.GetBytes(codeJson.ToString()));
                var respJson = new Json(resp);
                if (respJson.IndexByKey("token") == -1)
                {
                    LastError = resp;
                    return MFAuthErrors.InvalidData;
                }

                Token = respJson["token"].Value.ToString();
                Email = Password = "";
                return MFAuthErrors.Successful;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return MFAuthErrors.Error;
            }
        }

        public MFAuthErrors MFAuthSendSMS()
        {
            var codeJson = Json.FromAnonymous(new
            {
                ticket = _Ticket,
            });

            try
            {
                var resp = WEB.Post("https://discord.com/api/v" + DiscordApiVersion + "/auth/mfa/sms/send", new string[] { "Content-Type: application/json", "accept-language: " + Language }, Encoding.UTF8.GetBytes(codeJson.ToString()));
                var respJson = new Json(resp);
                if (respJson.IndexByKey("phone") == -1)
                {
                    LastError = resp;
                    return MFAuthErrors.InvalidData;
                }
                Phone = respJson["phone"].Value.ToString();
                return MFAuthErrors.Successful;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return MFAuthErrors.Error;
            }
        }

        public ForgotPasswordErrors ForgotPassword()
        {
            var forgotJson = Json.FromAnonymous(new
            {
                email = Email
            });

            try
            {
                var resp = WEB.Post("https://discord.com/api/v" + DiscordApiVersion + "/auth/forgot", new string[] { "Content-Type: application/json", "accept-language: " + Language }, Encoding.UTF8.GetBytes(forgotJson.ToString()));
                if (!string.IsNullOrEmpty(resp))
                {
                    LastError = resp;
                    try
                    {
                        new Json(resp);
                        return ForgotPasswordErrors.DataError;
                    }
                    catch
                    {
                        return ForgotPasswordErrors.Error;
                    }
                }
                else
                    return ForgotPasswordErrors.Successful;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return ForgotPasswordErrors.Error;
            }
        }

        public bool SetCustomStatus(string text = null, string emoji_name = null, DateTime expires_at = default)
        {
            try
            {
                if (text is null &&
                    emoji_name is null &&
                    expires_at == default)
                    WEB.Post("https://discord.com/api/v" + DiscordApiVersion + "/users/@me/settings", new string[] { "Content-Type: application/json", "Authorization: " + Token, "accept-language: " + Language }, Encoding.UTF8.GetBytes("{\"custom_status\":null}"), "PATCH");
                else
                {
                    var json = Json.FromAnonymous(new
                    {
                        custom_status = new { }
                    });
                    if (!(text is null))
                        json["custom_status"].Get<Json>().Add(new JsonObject("text", text));
                    if (!(emoji_name is null))
                        json["custom_status"].Get<Json>().Add(new JsonObject("emoji_name", emoji_name));
                    if (expires_at != default)
                        json["custom_status"].Get<Json>().Add(new JsonObject("expires_at", expires_at.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
                    //2021-02-08T14:12:22.964+06Z
                    //yyyy-MM-ddTHH:mm:ss.fffzzZ
                    //2021-02-08T18:00:00.000Z
                    //yyyy-MM-ddTHH:mm:ss.fffZ

                    WEB.Post("https://discord.com/api/v" + DiscordApiVersion + "/users/@me/settings", new string[] { "Content-Type: application/json", "Authorization: " + Token, "accept-language: " + Language }, Encoding.UTF8.GetBytes(json.ToString()), "PATCH");
                }

                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }
    }
}
