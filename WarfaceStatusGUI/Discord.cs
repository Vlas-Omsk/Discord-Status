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
using WebSocketSharp;
using WarfaceStatusGUI;
using WEBLib;

namespace WarfaceStatus
{
    public class Discord
    {
        private string cookie = @"Z:\Users\Vlas Dergaev\AppData\Roaming\discord\Cookies";
        private static string tempfolder = _GetConsoleCommandOut("cmd", "/c echo %TEMP%").Trim();
        public static string DiscordAppUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.306 Chrome/78.0.3904.130 Electron/7.1.11 Safari/537.36";
        public static string AppIdAssets;
        public static JsonObjectArray AppAssets;
        public static MainWindow Parent;

        public Discord(MainWindow parent)
        {
            Parent = parent;
        }

        public static Bitmap GetImageById(string id, string appId)
        {
            System.Net.HttpWebRequest request =
                    (System.Net.HttpWebRequest)System.Net.WebRequest.Create(
                        "https://cdn.discordapp.com/app-assets/"+ appId + "/" + id);
            System.Net.WebResponse response = request.GetResponse();
            Stream responseStream =
                response.GetResponseStream();
            return new System.Drawing.Bitmap(responseStream);
        }

        public static string GetImageIdByName(string name, string appId)
        {
            if (AppIdAssets != appId)
            {
                var resp = new Json(WEB.Get("https://discord.com/api/v6/oauth2/applications/" + appId + "/assets"));
                if (resp.IndexByKey("code") != -1)
                    return "";
                AppAssets = resp[0].Value;
            }

            AppIdAssets = appId;

            foreach (Json item in AppAssets)
            {
                if (item["name"].Value == name)
                    return item["id"].Value.ToString();
            }

            return "";
        }

        private static string _GetConsoleCommandOut(string path, string args)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = path;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();

            using (StreamReader sr = proc.StandardOutput)
            {
                return sr.ReadToEnd();
            }
        }

        public string Email = "";
        public string Password = "";
        private string _Ticket = "";
        public string Token = "";
        public string Phone;
        public WebSocket WebSocket;
        public Exception LastError;

        public AuthErrors Auth()
        {
            var authJson = new Json(new
            {
                email = Email,
                password = Password,
                undelete = false,
                captcha_key = string.Empty,
                login_source = string.Empty,
                gift_code_sku_id = string.Empty
            });

            try
            {
                var resp = WEB.Post("https://discord.com/api/v6/auth/login", new string[] { "Content-Type: application/json" }, authJson.ToString());
                var respJson = new Json(resp);
                if (respJson.IndexByKey("email") != -1 || respJson.IndexByKey("password") != -1)
                {
                    LastError = new Exception(resp);
                    return AuthErrors.LoginError;
                }
                if (respJson.IndexByKey("captcha_key") != -1)
                {
                    LastError = new Exception("Необходим captcha_key, сначала попробуйте войти через браузер");
                    return AuthErrors.Error;
                }
                if (respJson["token"].Value == "" && respJson["mfa"].Value.Value == true)
                {
                    _Ticket = respJson["ticket"].Value;
                    return AuthErrors.MultiFactorAuthentication;
                }
                else
                {
                    Token = respJson["token"].Value;
                    return AuthErrors.Successful;
                }
            }
            catch (Exception ex)
            {
                LastError = ex;
                return AuthErrors.Error;
            }
        }

        public MFAuthErrors MFAuth(int code, MFAuthType type)
        {
            var codeJson = new Json(new
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
                    resp = WEB.Post("https://discord.com/api/v6/auth/mfa/totp", new string[] { "Content-Type: application/json" }, codeJson.ToString());
                else if (type == MFAuthType.SMS)
                    resp = WEB.Post("https://discord.com/api/v6/auth/mfa/sms", new string[] { "Content-Type: application/json" }, codeJson.ToString());
                var respJson = new Json(resp);
                if (respJson.IndexByKey("token") == -1)
                {
                    LastError = new Exception(resp);
                    return MFAuthErrors.InvalidData;
                }

                Token = respJson["token"].Value;
                return MFAuthErrors.Successful;
            }
            catch (Exception ex)
            {
                LastError = ex;
                return MFAuthErrors.Error;
            }
        }

        public MFAuthErrors MFAuthSendSMS()
        {
            var codeJson = new Json(new
            {
                ticket = _Ticket,
            });

            try
            {
                var resp = WEB.Post("https://discord.com/api/v6/auth/mfa/sms/send", new string[] { "Content-Type: application/json" }, codeJson.ToString());
                var respJson = new Json(resp);
                if (respJson.IndexByKey("phone") == -1)
                {
                    LastError = new Exception(resp);
                    return MFAuthErrors.InvalidData;
                }
                Phone = respJson["phone"].Value;
                return MFAuthErrors.Successful;
            }
            catch (Exception ex)
            {
                LastError = ex;
                return MFAuthErrors.Error;
            }
        }

        public ForgotPasswordErrors ForgotPassword()
        {
            var forgotJson = new Json(new
            {
                email = Email
            });

            try
            {
                var resp = WEB.Post("https://discord.com/api/v6/auth/forgot", new string[] { "Content-Type: application/json" }, forgotJson.ToString());
                if (!string.IsNullOrEmpty(resp))
                {
                    LastError = new Exception(resp);
                    return ForgotPasswordErrors.DataError;
                }
                else
                {
                    return ForgotPasswordErrors.Successful;
                }
            }
            catch (Exception ex)
            {
                LastError = ex;
                return ForgotPasswordErrors.Error;
            }
        }

        public static bool IsTokenValid(string token)
        {
            try
            {
                WEB.Post("https://discord.com/api/v6/users/@me", new string[] { "authorization: " + token }, "", "GET");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void WSConnect()
        {
            WebSocket = new WebSocket("wss://gateway.discord.gg/?encoding=json&v=6");
            WebSocket.OnOpen += WSOnOpen;
            WebSocket.OnMessage += WSOnMessage;
            WebSocket.OnClose += WSOnClose;
            WebSocket.OnError += WSOnError;
            WebSocket.Connect();
        }

        public bool disconnectedManually = false;
        public void WSDisconnect()
        {
            disconnectedManually = true;
            if (WebSocket != null)
                WebSocket.Close();
            if (_EventTimer != null)
                _EventTimer.Stop();
        }

        private System.Timers.Timer _EventTimer;
        public void InitTimer()
        {
            _EventTimer = new System.Timers.Timer(40000);
            _EventTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                try
                {
                    WebSocket.Send("{\"op\":1,\"d\":3}");
                }
                catch { }
            };
            _EventTimer.AutoReset = true;
            _EventTimer.Enabled = true;
            _EventTimer.Start();
        }

        private void WSOnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            c.u("WebSocket", "Error: " + e.Exception.ToString());
        }

        private void WSOnClose(object sender, CloseEventArgs e)
        {
            c.u("WebSocket", "Closed " + (e.WasClean ? "clean" : "") + ": " + e.Reason + "(" + e.Code + ")");
            if (_EventTimer != null)
                _EventTimer.Stop();
            if (!disconnectedManually)
            {
                WSConnect();
                Parent.setStatus();
            }
            else
                disconnectedManually = false;
        }

        private void WSOnMessage(object sender, MessageEventArgs e)
        {
                if (e.Data.Length > 200)
                    c.u("WebSocket", "Message [Truncated]: " + e.Data.Substring(0, 200) + "[...]");
                else
                    c.u("WebSocket", e.Data);
        }

        private void WSOnOpen(object sender, System.EventArgs e)
        {
            c.u("WebSocket", "Opened");

            var authJson = new Json(new
            {
                op = 2,
                d = new
                {
                    token = Token,
                    capabilities = 29,
                    properties = new
                    {
                        os = "Windows",
                        browser = "Discord Client",
                        release_channel = "stable",
                        client_build_number = 64473,
                        client_event_source = string.Empty
                    },
                    presence = new
                    {
                        status = "online",
                        since = 0,
                        activities = new[] { new { } },
                        afk = false
                    },
                    compress = false,
                    client_state = new
                    {
                        guild_hashes = new { },
                        highest_last_message_id = "0",
                        read_state_version = 0,
                        user_guild_settings_version = -1
                    }
                }
            });

            WebSocket.Send(authJson.ToString());

            c.u("WebSocket", "Login");

            InitTimer();
        }
    }

    public enum AuthErrors
    {
        LoginError,
        Error,
        MultiFactorAuthentication,
        EmptyEmailOrPassword,
        Successful
    }

    public enum ForgotPasswordErrors
    {
        DataError,
        Error,
        Successful
    }

    public enum MFAuthType
    {
        Code,
        SMS
    }

    public enum MFAuthErrors
    {
        InvalidData,
        Error,
        Successful
    }
}
