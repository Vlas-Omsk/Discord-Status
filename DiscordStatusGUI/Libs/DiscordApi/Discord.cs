using PinkJson;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WEBLib;
using WebSocketSharp;
using DiscordStatusGUI.Extensions;
using PinkJson.Parser;
using System.Security.Cryptography;

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
            private static JsonObjectArray _LastAppAssets;
            private static string _LastAppAssetsResponse;

            public static Bitmap GetImageById(string id, string appId)
            {
                System.Net.HttpWebRequest request =
                        (System.Net.HttpWebRequest)System.Net.WebRequest.Create(
                            "https://cdn.discordapp.com/app-assets/" + appId + "/" + id);
                using (System.Net.WebResponse response = request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                    return new System.Drawing.Bitmap(responseStream);
            }

            public static string GetImageIdByName(string name, string appId)
            {
                var response = WEB.Get("https://discord.com/api/v6/oauth2/applications/" + appId + "/assets");
                if (_LastAppAssetsResponse != response)
                {
                    try
                    {
                        _LastAppAssets = new JsonObjectArray(response);
                        _LastAppAssetsResponse = response;
                    }
                    catch
                    {
                        return null;
                    }
                }

                foreach (Json item in _LastAppAssets)
                {
                    if (item["name"].Value.ToString() == name)
                        return item["id"].Value.ToString();
                }

                return null;
            }
        }

        public DiscordSocket Socket;
        public string Email = "";
        public string Password = "";
        public string Token;
        public string Phone { get; private set; }
        public string LastError { get; private set; }

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

        public void test()
        {
            var avatar = new Bitmap(new Bitmap(@"Z:\Users\Vlas Dergaev\Desktop\Desktop Sorted\images\m1000x1000.png"), 128, 128);
            var tmpimg = new Guid() + ".png";
            avatar.Save(tmpimg);
            var ImageData = "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(tmpimg));

            var req = Json.FromAnonymous(new
            {
                application_name = "Notepa",
                application_hash = "1c2b6420e46051799f9a125a60b94ed5",
                icon = ImageData
            });

            var resp = WEB.Post($"https://discord.com/api/v{DiscordApiVersion}/unverified-applications/icons", new string[] {
                "authorization: " + Token,
                "Content-Type: application/json"
            }, Encoding.UTF8.GetBytes(req.ToString()));
            ConsoleEx.WriteLine("TEST", resp.UnescapeString());

            req = Json.FromAnonymous(new
            {
                name = "Notepa",
                os = "win32",
                icon = "",
                distributor_application = "",
                executable = "notepad++/notepad++.exe",
                publisher = "Notepad++ Team",
                report_version = 3
            });
            resp = WEB.Post($"https://discord.com/api/v{DiscordApiVersion}/unverified-applications", new string[] {
                "authorization: " + Token,
                "Content-Type: application/json"
            }, Encoding.UTF8.GetBytes(req.ToString()));
            ConsoleEx.WriteLine("TEST", resp.UnescapeString());
        }
    }
}
