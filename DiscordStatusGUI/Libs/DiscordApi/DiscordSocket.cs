using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using PinkJson;
using System.Diagnostics;
using System.Windows.Controls;
using DiscordStatusGUI.Extensions;
using System.Threading;

namespace DiscordStatusGUI.Libs.DiscordApi
{
    class DiscordSocket
    {
        public WebSocket WebSocket;
        public bool IsConnected => WebSocket?.IsAlive ?? false;
        public UserStatus CurrentUserStatus;

        private bool _DisconnectedManually = false;
        private System.Timers.Timer _KeepAliveTimer;
        private Discord _Discord;

        public DiscordSocket(Discord discord)
        {
            _Discord = discord;
        }

        public async void Connect()
        {
            await Task.Run(() =>
            {
                OnWorkingStatusChanged?.Invoke("Connecting to Discord");

                WebSocket = new WebSocket("wss://gateway.discord.gg/?encoding=json&v=" + Discord.DiscordApiVersion);
                WebSocket.OnOpen += WSOnOpen;
                WebSocket.OnMessage += WSOnMessage;
                WebSocket.OnClose += WSOnClose;
                WebSocket.OnError += WSOnError;

                WebSocket.Connect();
            });
        }

        public async void Disconnect()
        {
            await Task.Run(() =>
            {
                _DisconnectedManually = true;
                if (WebSocket != null)
                    WebSocket?.Close();
                else
                    _DisconnectedManually = false;
                _KeepAliveTimer?.Stop();
            });
        }

        public async void UpdateActivity(Activity activity)
        {
            await Task.Run(() =>
            {
                var statusJson = Json.FromAnonymous(new
                {
                    op = 3,
                    d = CreateActivityJson(activity)
                });

                if (IsConnected)
                {
                    var str = statusJson.ToString();
                    WebSocket.Send(str);
                }
            });
        }


        private void WSOnError(object sender, ErrorEventArgs e)
        {
            //c.u("WebSocket", "Error: " + e.Exception.ToString());
            ConsoleEx.WriteLine(ConsoleEx.WebSocket, e.Message);
        }

        private void WSOnClose(object sender, CloseEventArgs e)
        {
            //c.u("WebSocket", "Closed " + (e.WasClean ? "clean" : "") + ": " + e.Reason + "(" + e.Code + ")");
            ConsoleEx.WriteLine(ConsoleEx.WebSocket, "Closed" + (e.WasClean ? " clean" : "") + ": " + e.Reason + "(" + e.Code + ")");

            _KeepAliveTimer?.Stop();
            if (!_DisconnectedManually)
            {
                OnAutoReconnect();
                //Parent.setStatus();
            }
            else
                _DisconnectedManually = false;
        }

        private void WSOnMessage(object sender, MessageEventArgs e)
        {
            ConsoleEx.WriteLine(ConsoleEx.WebSocket, e.Data);
            //if (e.Data.Length > 200)
            //    c.u("WebSocket", "Message [Truncated]: " + e.Data.Substring(0, 200) + "[...]");
            //else
            //    c.u("WebSocket", e.Data);
            OnMessage(e.Data);
        }

        private void WSOnOpen(object sender, EventArgs e)
        {
            ConsoleEx.WriteLine(ConsoleEx.WebSocket, "Opened");
            //c.u("WebSocket", "Opened");

            AuthClear();

            //c.u("WebSocket", "Login");

            InitKeepAliveTimer();
        }

        Thread AutoReconnectThread;
        private void OnAutoReconnect()
        {
            if (AutoReconnectThread?.IsAlive != true)
            {
                AutoReconnectThread = new Thread(OnAutoReconnectThread) { IsBackground = true, ApartmentState = ApartmentState.STA };
                AutoReconnectThread.Start();
            }
        }

        private void OnAutoReconnectThread()
        {
            while (!_DisconnectedManually && !IsConnected)
            {
                AutoReconnect?.Invoke();
                Connect();
                Thread.Sleep(30000);
            }
        }


        private Json CreateActivityJson(Activity Activity)
        {
            ConsoleEx.WriteLine(ConsoleEx.Info, Activity.ActivityType.ToString(Activity));
            var activity = Json.FromAnonymous(new
            {
                type = Activity.ActivityType.ID,
                assets = new
                {
                    //empty = ""
                },
                timestamps = new
                {
                    //empty = ""
                },
                party = new
                {
                    //empty = ""
                }
            });

            if (!string.IsNullOrEmpty(Activity.Name))
                activity.Add(new JsonObject("name", Activity.Name));
            if (!string.IsNullOrEmpty(Activity.ApplicationID))
                activity.Add(new JsonObject("application_id", Activity.ApplicationID));
            if (!string.IsNullOrEmpty(Activity.Details))
                activity.Add(new JsonObject("details", Activity.Details));
            if (!string.IsNullOrEmpty(Activity.State))
                activity.Add(new JsonObject("state", Activity.State));
            if (!string.IsNullOrEmpty(Activity.ImageLargeKey))
            {
                string i;
                if ((i = Discord.AppImages.GetImageIdByName(Activity.ImageLargeKey, Activity.ApplicationID)) != null)
                    (activity["assets"].Value as Json).Add(new JsonObject("large_image", i));
            }
            if (!string.IsNullOrEmpty(Activity.ImageLargeText))
                (activity["assets"].Value as Json).Add(new JsonObject("large_text", Activity.ImageLargeText));
            if (!string.IsNullOrEmpty(Activity.ImageSmallKey))
            {
                string i;
                if ((i = Discord.AppImages.GetImageIdByName(Activity.ImageSmallKey, Activity.ApplicationID)) != null)
                    (activity["assets"].Value as Json).Add(new JsonObject("small_image", i));
            }
            if (!string.IsNullOrEmpty(Activity.ImageSmallText))
                (activity["assets"].Value as Json).Add(new JsonObject("small_text", Activity.ImageSmallText));
            if (!string.IsNullOrEmpty(Activity.StartTime) && Activity.StartTime != "0")
                (activity["timestamps"].Value as Json).Add(new JsonObject("start", Activity.StartTime));
            if (!string.IsNullOrEmpty(Activity.EndTime) && Activity.EndTime != "0")
                (activity["timestamps"].Value as Json).Add(new JsonObject("end", Activity.EndTime));
            if (!string.IsNullOrEmpty(Activity.PartyMax) && !string.IsNullOrEmpty(Activity.PartySize))
            {
                var obj = new JsonArray();
                obj.Add(Activity.PartySize);
                obj.Add(Activity.PartyMax);
                (activity["party"].Value as Json).Add(new JsonObject("size", obj));
            }


            var statusJson = Json.FromAnonymous(new
            {
                status = CurrentUserStatus.ToString(),
                activities = new JsonArray(),
                active = true,
                since = 0,
                afk = false
            });

            if (CurrentUserStatus == UserStatus.online && !string.IsNullOrEmpty(Activity.Name))
            {
                (statusJson["activities"].Value as JsonArray).Add(activity);
            }

            return statusJson;
        }

        private void AuthClear()
        {
            OnWorkingStatusChanged?.Invoke("Authorization in Discord");

            var authJson = Json.FromAnonymous(new
            {
                op = 2,
                d = new
                {
                    token = _Discord.Token,
                    capabilities = 29,
                    properties = new
                    {
                        os = "Windows",
                        browser = "Discord Client",
                        release_channel = "stable",
                        client_build_number = 64473,
                        client_event_source = string.Empty
                    },
                    presence = CreateActivityJson(new Activity()),
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

            var d = authJson.ToString();
            WebSocket?.Send(d);

            OnWorkingStatusChanged?.Invoke("");
        }


        private async void OnMessage(string data)
        {
            await Task.Run(() =>
            {
                if (data.Contains("READY") || data.Contains("USER_UPDATE") || data.Contains("USER_SETTINGS_UPDATE"))
                {
                    //var s = new Stopwatch();
                    //s.Start();

                    var json = new Json(data);
                    if (json["t"].Value.ToString() == "READY")
                    {
                        OnWorkingStatusChanged?.Invoke("Authorization in Discord (READY)");
                        OnUserInfoChanged?.Invoke("READY", json, new UserInfo()
                        {
                            UserName = (json["d"]["user"]["username"].Value ?? "").ToString(),
                            Phone = (json["d"]["user"]["phone"].Value ?? "").ToString(),
                            Id = (json["d"]["user"]["id"].Value ?? "").ToString(),
                            Email = (json["d"]["user"]["email"].Value ?? "").ToString(),
                            Discriminator = (json["d"]["user"]["discriminator"].Value ?? "").ToString(),
                            AvatarId = (json["d"]["user"]["avatar"].Value ?? "").ToString()
                        });
                        OnUserSettingsChanged?.Invoke("READY", json, json["d"]["user_settings"].Get<Json>());
                    }
                    else if (json["t"].Value.ToString() == "USER_UPDATE")
                    {
                        OnWorkingStatusChanged?.Invoke("User info changed (USER_UPDATE)");
                        OnUserInfoChanged?.Invoke("USER_UPDATE", json, new UserInfo()
                        {
                            UserName = (json["d"]["username"].Value ?? "").ToString(),
                            Phone = (json["d"]["phone"].Value ?? "").ToString(),
                            Id = (json["d"]["id"].Value ?? "").ToString(),
                            Email = (json["d"]["email"].Value ?? "").ToString(),
                            Discriminator = (json["d"]["discriminator"].Value ?? "").ToString(),
                            AvatarId = (json["d"]["avatar"].Value ?? "").ToString()
                        });
                    }
                    else if (json["t"].Value.ToString() == "USER_SETTINGS_UPDATE")
                    {
                        OnWorkingStatusChanged?.Invoke("User settings changed (USER_SETTINGS_UPDATE)");
                        OnUserSettingsChanged?.Invoke("USER_SETTINGS_UPDATE", json, json["d"].Get<Json>());
                    }

                    //s.Stop();
                    //OnWorkingStatusChanged?.Invoke(s.ElapsedMilliseconds + " ms");
                }
            });
        }

        public delegate void EventHandler(string eventtype, object rawdata);
        public delegate void EventHandler<T>(string eventtype, object rawdata, T data);
        public event EventHandler<UserInfo> OnUserInfoChanged;
        public event EventHandler<Json> OnUserSettingsChanged;

        public delegate void OnWorkingStatusChangedEventHandler(string msg);
        public event OnWorkingStatusChangedEventHandler OnWorkingStatusChanged;
        public delegate void OnAutoReconnectEventHandler();
        public event OnAutoReconnectEventHandler AutoReconnect;

        private void InitKeepAliveTimer()
        {
            _KeepAliveTimer = new System.Timers.Timer(40000);
            _KeepAliveTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                try
                {
                    WebSocket.Send("{\"op\":1,\"d\":3}");
                }
                catch { }
            };
            _KeepAliveTimer.AutoReset = true;
            _KeepAliveTimer.Enabled = true;
            _KeepAliveTimer.Start();
        }
    }
}