using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.Libs;
using DiscordStatusGUI.Models;
using DiscordStatusGUI.ViewModels.Tabs;
using DiscordStatusGUI.Views.Tabs;
using PinkJson;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DiscordStatusGUI
{
    class Preferences
    {
        public static string Discord_Token     { get => Static.Discord.Token;                         set => Static.Discord.Token = value; }
        public static int CurrentUserStatus    { get => (int)Static.Discord.Socket.CurrentUserStatus; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.TabSettings.Page as Settings).DataContext as SettingsViewModel).SelectedUserStatusIndex = value); }
        public static bool IsDiscordConnected  { get => Static.Discord.Socket.IsConnected;            set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.TabSettings.Page as Settings).DataContext as SettingsViewModel).IsDiscordConnected = value); }
        public static int WarfaceActivityIndex { 
            get => Static.TabWarface.GetDataContext<WarfaceViewModel>().SelectedProfileIndex; 
            set => Static.TabWarface.GetDataContext<WarfaceViewModel>().SetProfileIndex(value);
        }
        public static int SteamActivityIndex
        {
            get => Static.TabSteam.GetDataContext<SteamViewModel>().SelectedProfileIndex;
            set => Static.TabSteam.GetDataContext<SteamViewModel>().SetProfileIndex(value);
        }
        public static int CurrentActivityIndex { get => Static.CurrentActivityIndex;                  set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.TabGameStatus.Page as GameStatus).DataContext as GameStatusViewModel).SelectedProfileIndex = value); }
        public static bool FastGameClientClose { get => WarfaceApi.FastGameClientClose;               set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.TabWarface.Page as Warface).DataContext as WarfaceViewModel).IsFastGameClientClose = value); }

        public static double X { get => Static.MainWindow.Dispatcher.Invoke(() => Static.MainWindow.Left); set => Static.MainWindow.Left = value; }
        public static double Y { get => Static.MainWindow.Dispatcher.Invoke(() => Static.MainWindow.Top);  set => Static.MainWindow.Top = value; }
        private static double _SavedWidth, _SavedHeight;
        public static double Width  
        {
            get
            {
                if (State != (int)WindowState.Normal)
                    return _SavedWidth;
                return _SavedWidth = Static.MainWindow.Dispatcher.Invoke(() => Static.MainWindow.ActualWidth);
            }
            set => Static.MainWindow.Width = value; 
        }
        public static double Height 
        {
            get
            {
                if (State != (int)WindowState.Normal)
                    return _SavedHeight;
                return _SavedHeight = Static.MainWindow.Dispatcher.Invoke(() => Static.MainWindow.ActualHeight);
            }
            set => Static.MainWindow.Height = value;
        }
        public static int  State  { get => Static.MainWindow.Dispatcher.Invoke(() => (int)Static.MainWindow.WindowState); set => Static.MainWindow.Dispatcher.Invoke(() => Static.MainWindow.WindowState = (WindowState)value); }


        public static bool Loading { get; private set; } = false;
        public static bool LoadingProfiles { get; private set; } = false;

        #region Profiles
        public static void LoadProfiles()
        {
            if (File.Exists("profiles.json"))
            {
                LoadingProfiles = true;

                Static.Activities = JsonArray.ToArray<Libs.DiscordApi.Activity>(new JsonArray(FileInfoEx.SafeReadText("profiles.json")));

                LoadingProfiles = false;
            }
            Static.InitializationSteps.IsProfilesLoaded = true;
        }

        public static void SaveProfiles()
        {
            if (!LoadingProfiles)
                File.WriteAllText("profiles.json", JsonArray.FromArray(Static.Activities, false, new string[] { "SavedState" }).ToFormatString());
        }
        #endregion

        #region Preferences
        public static void Save()
        {
            if (!Loading)
            {
                var propjson = Json.FromAnonymous(new
                {
                    Accounts = new
                    {
                        Discord = new
                        {
                            IsDiscordConnected,
                            token = AES.EncryptString(Discord_Token, "Some kind of password")
                        },
                        Warface = new
                        {
                            WarfaceActivityIndex,
                            FastGameClientClose
                        },
                        Steam = new
                        {
                            SteamApi.CurrentSteamProfile.SteamLoginSecure,
                            SteamApi.CurrentSteamProfile.ID,
                            SteamActivityIndex
                        }
                    },
                    Static.Version,
                    CurrentUserStatus,
                    CurrentActivityIndex,
                    Window = new
                    {
                        X, Y, Width, Height, State
                    }
                });

                try
                {
                    File.WriteAllText("preferences.json", propjson.ToFormatString());
                } catch (Exception ex)
                {
                    ConsoleEx.WriteLine(ConsoleEx.Warning, $"Preferences.Save() -> Error {ex.HResult}");
                }
            }
        }

        public static void Load()
        {
            if (File.Exists("preferences.json"))
            {
                Loading = true;

                try
                {
                    var propjson = new Json(FileInfoEx.SafeReadText("preferences.json"));
                    
                    Static.MainWindow.Dispatcher.Invoke(() =>
                    {
                        X = (int)propjson["Window"]["X"].Value;
                        Y = (int)propjson["Window"]["Y"].Value;
                        Width = (int)propjson["Window"]["Width"].Value;
                        Height = (int)propjson["Window"]["Height"].Value;
                    });
                    State = (int)propjson["Window"]["State"].Value;

                    Discord_Token = AES.DecryptString(propjson["Accounts"]["Discord"]["token"].Value?.ToString(), "Some kind of password");
                    CurrentUserStatus = (int)propjson["CurrentUserStatus"].Value;
                    CurrentActivityIndex = (int)propjson["CurrentActivityIndex"].Value;
                    FastGameClientClose = (bool)propjson["Accounts"]["Warface"]["FastGameClientClose"].Value;
                    WarfaceActivityIndex = (int)propjson["Accounts"]["Warface"]["WarfaceActivityIndex"].Value;
                    SteamActivityIndex = (int)propjson["Accounts"]["Steam"]["SteamActivityIndex"].Value;
                    SteamApi.CurrentSteamProfile.SteamLoginSecure = propjson["Accounts"]["Steam"]["SteamLoginSecure"].Get<string>();
                    SteamApi.CurrentSteamProfile.ID = propjson["Accounts"]["Steam"]["ID"].Get<string>();

                    IsDiscordConnected = (bool)propjson["Accounts"]["Discord"]["IsDiscordConnected"].Value;
                }
                catch (Exception ex)
                {
                    var name = "preferences";
                    int i = 1;
                    for (; File.Exists(name + "_copy" + i + ".json"); i++) ;
                    name += "_copy" + i + ".json";

                    Static.Dialogs.MessageBoxShow("Oops...", "It looks like your settings are not formatted correctly. Your settings will be copied to the file \"" + name + "\", and we will write the new ones.", new System.Collections.ObjectModel.ObservableCollection<ButtonItem>() { Static.Dialogs.ButtonOk }, System.Windows.HorizontalAlignment.Right, null, "/DiscordStatusGUI;component/Resources/PixelCat/Lying2.png");

                    File.Move("preferences.json", name);
                }

                Loading = false;
            }
            Static.InitializationSteps.IsPreferencesLoaded = true;
        }
        #endregion

        public static void SetPropertiesByURL(string url)
        {
            Uri myUri = new Uri(url.Trim(1));
            var gparams = System.Web.HttpUtility.ParseQueryString(myUri.Query);

            if (gparams.Count == 0)
            {
                Static.Window.Normalize();
                return;
            }

            Static.MainWindow.Dispatcher.Invoke(() =>
            {
                foreach (var s in gparams.AllKeys)
                {
                    var value = gparams[s].ToLower();
                    switch (s.ToLower())
                    {
                        case "windowstate":
                            switch (value)
                            {
                                case "opened": Static.Window.Normalize(); break;
                                case "closed": Static.Window.Close(); break;
                            }
                            break;
                        case "currentactivityindex":
                            if (int.TryParse(value, out int result))
                                Preferences.CurrentActivityIndex = result;
                            break;
                    }
                }
            });
        }

        public static void SetPropertiesByCmdLine(string[] cmdline)
        {
            var list = cmdline.ToList();

            Static.MainWindow.Dispatcher.Invoke(() =>
            {
                if (list.Count <= 1)
                {
                    Static.Window.Normalize();
                    return;
                }

                for (var i = 0; i < list.Count; i++)
                {
                    var param = list[i];
                    switch (param)
                    {
                        case "--url":
                            Preferences.SetPropertiesByURL(list[++i]);
                            break;
                        case "--tray":
                            if (Static.InitializationSteps.IsInitialized)
                                Static.Window.Close();
                            else
                                Static.InitializationSteps.FirstInitialization += () => Static.Window.Close();
                            break;
                    }
                }
            });
        }

        public static void OpenLocalServer()
        {
            var d = new WebSocketServer(IPAddress.Parse("127.0.0.1"), 48655);
            d.AddWebSocketService<LocalServer>("/");
            d.Start();
        }
    }

    class LocalServer : WebSocketBehavior
    {
        #region Override
        protected override void OnOpen()
        {
            ConsoleEx.WriteLine(ConsoleEx.WebSocketServer, "Connected");
            base.OnOpen();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            SendResponse(e.Data);
            base.OnMessage(e);
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            ConsoleEx.WriteLine(ConsoleEx.WebSocketServer, "Error: " + e.Message);
            base.OnError(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            ConsoleEx.WriteLine(ConsoleEx.WebSocketServer, "Closed" + (e.WasClean ? " clean" : "") + ": " + e.Reason + "(" + e.Code + ")");
            base.OnClose(e);
        }
        #endregion

        void SendResponse(string data)
        {
            var response = new JsonArray();

            try
            {
                var jsonarr = new JsonArray(data);

                for (var i = 0; i < jsonarr.Count; i++)
                {
                    var inresponse = new Json();
                    try
                    {
                        var json = jsonarr[i].Get<Json>();

                        if (json.IndexByKey("cmd") != -1)
                            switch (json["cmd"].Value.ToString())
                            {
                                case "IsStarted":
                                    inresponse.Add(json["cmd"]);
                                    inresponse.Add(new JsonObject("IsStarted", true));
                                    inresponse.Add(new JsonObject("Protocol", RegistryCommands.Protocol));
                                    break;
                                case "Window":
                                    inresponse.Add(json["cmd"]);
                                    if (json.IndexByKey("Value") != -1)
                                        Static.MainWindow.Dispatcher.Invoke(() =>
                                        {
                                            switch (json["Value"].Value.ToString())
                                            {
                                                case "Minimize": Static.Window.Minimize(); break;
                                                case "Maximize": Static.Window.Maximize(); break;
                                                case "Normalize": Static.Window.Normalize(); break;
                                                case "Close": Static.Window.Normalize(); break;
                                            }
                                        });
                                    inresponse.Add(new JsonObject("WindowState", Preferences.State));
                                    break;
                                case "SetTopStatus":
                                    if (json.IndexByKey("Value") != -1)
                                        Static.Window.SetTopStatus(json["Value"].Value.ToString());
                                    break;
                            }
                    }
                    catch (Exception ex)
                    {
                        inresponse.Add(new JsonObject("Error", Json.FromAnonymous(new
                        {
                            HResult = ex.HResult,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace
                        })));
                    }
                    response.Add(inresponse);
                }
            }
            catch (Exception ex)
            {
                response.Add(Json.FromAnonymous(new
                {
                    HResult = ex.HResult,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                }));
            }

            Send(response.ToString());
        }
    }
}
