using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.Libs;
using DiscordStatusGUI.Models;
using DiscordStatusGUI.ViewModels.Tabs;
using DiscordStatusGUI.Views.Tabs;
using PinkJson.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DiscordStatusGUI
{
    class Preferences
    {
        public static string Discord_Token { get => Static.Discord.Token; set => Static.Discord.Token = value; }
        public static int CurrentUserStatus { get => (int)Static.Discord.Socket.CurrentUserStatus; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[1].Page as Settings).DataContext as SettingsViewModel).SelectedUserStatusIndex = value); }
        public static bool IsDiscordConnected { get => Static.Discord.Socket.IsConnected; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[1].Page as Settings).DataContext as SettingsViewModel).IsDiscordConnected = value); }
        public static int WarfaceActivityIndex { get => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[3].Page as Warface).DataContext as WarfaceViewModel).SelectedProfileIndex); set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[3].Page as Warface).DataContext as WarfaceViewModel).SelectedProfileIndex = value); }
        public static int CurrentActivityIndex { get => Static.CurrentActivityIndex; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[0].Page as GameStatus).DataContext as GameStatusViewModel).SelectedProfileIndex = value); }
        public static bool FastGameClientClose { get => WarfaceApi.FastGameClientClose; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[3].Page as Warface).DataContext as WarfaceViewModel).IsFastGameClientClose = value); }

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
        public static int  State  { get => Static.MainWindow.Dispatcher.Invoke(() => (int)Static.MainWindow.WindowState); set => Static.MainWindow.WindowState = (WindowState)value; }

        public static bool Loading { get; private set; } = false;
        public static bool LoadingProfiles { get; private set; } = false;

        #region Profiles
        public static void LoadProfiles()
        {
            if (File.Exists("profiles.json"))
            {
                LoadingProfiles = true;

                Static.Activities = JsonObjectArray.ToArray<Libs.DiscordApi.Activity>(new JsonObjectArray(FileInfoEx.SafeReadText("profiles.json")));

                LoadingProfiles = false;
            }
            Static.InitializationSteps.IsProfilesLoaded = true;
        }

        public static async void SaveProfiles()
        {
            if (!LoadingProfiles)
            await Task.Run(() =>
            {
                File.WriteAllText("profiles.json", JsonObjectArray.FromArray(Static.Activities, true, new string[] { "_SavedState" }).ToFormatString());
            });
        }
        #endregion

        #region Preferences
        public static async void Save()
        {
            if (!Loading)
            await Task.Run(() =>
            {
                var propjson = Json.FromAnonymous(new
                {
                    Accounts = new
                    {
                        Discord = new
                        {
                            IsDiscordConnected,
                            token = Discord_Token
                        },
                        Warface = new
                        {
                            WarfaceActivityIndex
                        }
                    },
                    CurrentUserStatus,
                    CurrentActivityIndex,
                    FastGameClientClose,
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
                    ConsoleEx.WriteLine(ConsoleEx.Warning, $"Preferences.Save() Error {ex.HResult}");
                }
            });
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
                        State = (int)propjson["Window"]["State"].Value;
                        Width = (int)propjson["Window"]["Width"].Value;
                        Height = (int)propjson["Window"]["Height"].Value;
                    });

                    Discord_Token = propjson["Accounts"]["Discord"]["token"].Value?.ToString();
                    CurrentUserStatus = (int)propjson["CurrentUserStatus"].Value;
                    CurrentActivityIndex = (int)propjson["CurrentActivityIndex"].Value;
                    FastGameClientClose = (bool)propjson["FastGameClientClose"].Value;
                    WarfaceActivityIndex = (int)propjson["Accounts"]["Warface"]["WarfaceActivityIndex"].Value;

                    IsDiscordConnected = (bool)propjson["Accounts"]["Discord"]["IsDiscordConnected"].Value;
                }
                catch
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
                if (list.Count <= 1 && Static.InitializationSteps.IsInitialized)
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
    }
}
