﻿using System;
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
    public class DiscordSocket
    {
        public WebSocket WebSocket;
        public bool IsConnected => WebSocket?.IsAlive ?? false;
        public UserStatus CurrentUserStatus;
        public PrivateChannels PrivateChannels;
        public UsersCache UsersCache;

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
                Static.InvokeAsync(WorkingStatusChanged, new WorkingStatusChangedEventArgs("Connecting to Discord"), this);

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
            ConsoleEx.WriteLine(ConsoleEx.DiscordWebSocket, e.Message);
        }

        private void WSOnClose(object sender, CloseEventArgs e)
        {
            //c.u("WebSocket", "Closed " + (e.WasClean ? "clean" : "") + ": " + e.Reason + "(" + e.Code + ")");
            ConsoleEx.WriteLine(ConsoleEx.DiscordWebSocket, "Closed" + (e.WasClean ? " clean" : "") + ": " + e.Reason + "(" + e.Code + ")");

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
            ConsoleEx.WriteLine(ConsoleEx.DiscordWebSocket, e.Data);
            //if (e.Data.Length > 200)
            //    c.u("WebSocket", "Message [Truncated]: " + e.Data.Substring(0, 200) + "[...]");
            //else
            //    c.u("WebSocket", e.Data);
            OnMessage(e.Data);
        }

        private void WSOnOpen(object sender, EventArgs e)
        {
            ConsoleEx.WriteLine(ConsoleEx.DiscordWebSocket, "Opened");
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
                Static.InvokeAsync(AutoReconnect, EventArgs.Empty, this);
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
            Static.InvokeAsync(WorkingStatusChanged, new WorkingStatusChangedEventArgs("Authorization in Discord"), this);

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

            Static.InvokeAsync(WorkingStatusChanged, new WorkingStatusChangedEventArgs(""), this);
        }


        private async void OnMessage(string data)
        {
            await Task.Run(() =>
            {
                if (data.Contains("READY") || data.Contains("USER_UPDATE") || data.Contains("USER_SETTINGS_UPDATE") || data.Contains("PRESENCE_UPDATE"))
                {
                    //var s = new Stopwatch();
                    //s.Start();

                    var json = new Json(data);
                    var type = json["t"].Value.ToString();
                    if (type == "READY")
                    {
                        Static.InvokeAsync(WorkingStatusChanged, new WorkingStatusChangedEventArgs("Authorization in Discord (READY)"), this);
                        Static.InvokeAsync(UserInfoChanged, new DiscordEventArgs<UserInfo>("READY", json, new UserInfo()
                        {
                            UserName = (json["d"]["user"]["username"].Value ?? "").ToString(),
                            Phone = (json["d"]["user"]["phone"].Value ?? "").ToString(),
                            Id = (json["d"]["user"]["id"].Value ?? "").ToString(),
                            Email = (json["d"]["user"]["email"].Value ?? "").ToString(),
                            Discriminator = (json["d"]["user"]["discriminator"].Value ?? "").ToString(),
                            AvatarId = (json["d"]["user"]["avatar"].Value ?? "").ToString()
                        }));
                        Static.InvokeAsync(UserSettingsChanged, new DiscordEventArgs<Json>("READY", json, json["d"]["user_settings"].Get<Json>()), this);

                        UsersCache = new UsersCache(_Discord);
                        UsersCache.AddRange(json["d"]["users"].Get<JsonArray>());
                        foreach (var user in json["d"]["merged_presences"]["friends"].Get<JsonArray>())
                        {
                            UsersCache.SetUserStatus(user["user_id"].Get<string>(), user["status"].Get<string>(), true);
                        }
                        Static.InvokeAsync(UsersCacheChanged, new DiscordEventArgs<UsersCache>("READY", json, UsersCache), this);

                        PrivateChannels = new PrivateChannels();
                        PrivateChannels.AddRange(json["d"]["private_channels"].Get<JsonArray>(), UsersCache);
                        Static.InvokeAsync(PrivateChannelsChanged, new DiscordEventArgs<PrivateChannels>("READY", json, PrivateChannels), this);
                    }
                    else if (type == "USER_UPDATE")
                    {
                        Static.InvokeAsync(WorkingStatusChanged, new WorkingStatusChangedEventArgs("User info changed (USER_UPDATE)"), this);
                        Static.InvokeAsync(UserInfoChanged, new DiscordEventArgs<UserInfo>("USER_UPDATE", json, new UserInfo()
                        {
                            UserName = (json["d"]["username"].Value ?? "").ToString(),
                            Phone = (json["d"]["phone"].Value ?? "").ToString(),
                            Id = (json["d"]["id"].Value ?? "").ToString(),
                            Email = (json["d"]["email"].Value ?? "").ToString(),
                            Discriminator = (json["d"]["discriminator"].Value ?? "").ToString(),
                            AvatarId = (json["d"]["avatar"].Value ?? "").ToString()
                        }), this);
                    }
                    else if (type == "USER_SETTINGS_UPDATE")
                    {
                        Static.InvokeAsync(WorkingStatusChanged, new WorkingStatusChangedEventArgs("User settings changed (USER_SETTINGS_UPDATE)"), this);
                        Static.InvokeAsync(UserSettingsChanged, new DiscordEventArgs<Json>("USER_SETTINGS_UPDATE", json, json["d"].Get<Json>()), this);
                    }
                    else if (type == "PRESENCE_UPDATE" && json["d"].IndexByKey("guild_id") == -1)
                    {
                        UsersCache.SetUserStatus(json["d"]["user"]["id"].Get<string>(), json["d"]["status"].Get<string>(), false);
                        Static.InvokeAsync(UsersCacheChanged, new DiscordEventArgs<UsersCache>("PRESENCE_UPDATE", json, UsersCache), this);
                    }

                    //s.Stop();
                    //OnWorkingStatusChanged?.Invoke(s.ElapsedMilliseconds + " ms");
                }
            });
        }

        public event EventHandler<DiscordEventArgs<UsersCache>> UsersCacheChanged;
        public event EventHandler<DiscordEventArgs<PrivateChannels>> PrivateChannelsChanged;
        public event EventHandler<DiscordEventArgs<UserInfo>> UserInfoChanged;
        public event EventHandler<DiscordEventArgs<Json>> UserSettingsChanged;

        public event EventHandler<WorkingStatusChangedEventArgs> WorkingStatusChanged;
        public event EventHandler<EventArgs> AutoReconnect;

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

    public class DiscordEventArgs<T>
    {
        public string EventType;
        public object RawData;
        public T Data;

        public DiscordEventArgs(string eventtype, object rawdata, T data)
        {
            EventType = eventtype;
            RawData = rawdata;
            Data = data;
        }
    }

    public class WorkingStatusChangedEventArgs
    {
        public string Message;

        public WorkingStatusChangedEventArgs(string message)
        {
            Message = message;
        }
    }
}