using DiscordStatusGUI.Libs;
using DiscordStatusGUI.ViewModels.Tabs;
using DiscordStatusGUI.Views.Tabs;
using PinkJson.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DiscordStatusGUI
{
    class Preferences
    {
        public static string Discord_Token { get => Static.Discord.Token; set => Static.Discord.Token = value; }
        public static int CurrentUserStatus { get => (int)Static.Discord.Socket.CurrentUserStatus; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[1].Page as Settings).DataContext as SettingsViewModel).SelectedUserStatusIndex = value); }
        public static bool IsDiscordConnected { get => Static.Discord.Socket.IsConnected; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[1].Page as Settings).DataContext as SettingsViewModel).IsDiscordConnected = value); }
        public static int WarfaceActivityIndex { get => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[3].Page as Warface).DataContext as WarfaceViewModel).SelectedProfileIndex); set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[3].Page as Warface).DataContext as WarfaceViewModel).SelectedProfileIndex = value); }
        public static int CurrentActivityIndex { get => Static.CurrentActivityIndex; set => Static.CurrentActivityIndex = value; }
        public static bool FastGameClientClose { get => WarfaceApi.FastGameClientClose; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[3].Page as Warface).DataContext as WarfaceViewModel).IsFastGameClientClose = value); }

        private static bool _loading = false;

        public static async void LoadProfiles()
        {
            await Task.Run(() =>
            {
                if (File.Exists("profiles.json"))
                    Static.Activities = JsonObjectArray.ToArray<Libs.DiscordApi.Activity>(new JsonObjectArray(File.ReadAllText("profiles.json")));
                Static.InitializationSteps.IsProfilesLoaded = true;
            });
        }

        public static async void SaveProfiles()
        {
            await Task.Run(() =>
            {
                File.WriteAllText("profiles.json", JsonObjectArray.FromArray(Static.Activities, true).ToFormatString());
            });
        }


        public static async void Save()
        {
            if (!_loading)
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
                    FastGameClientClose
                });

                File.WriteAllText("preferences.json", propjson.ToFormatString());
            });
        }

        public static async void Load()
        {
            await Task.Run(() =>
            {
                if (File.Exists("preferences.json"))
                {
                    _loading = true;

                    var propjson = new Json(File.ReadAllText("preferences.json"));

                    Discord_Token = propjson["Accounts"]["Discord"]["token"].Value.ToString();
                    CurrentUserStatus = (int)propjson["CurrentUserStatus"].Value;
                    CurrentActivityIndex = (int)propjson["CurrentActivityIndex"].Value;
                    FastGameClientClose = (bool)propjson["FastGameClientClose"].Value;
                    WarfaceActivityIndex = (int)propjson["Accounts"]["Warface"]["WarfaceActivityIndex"].Value;

                    IsDiscordConnected = (bool)propjson["Accounts"]["Discord"]["IsDiscordConnected"].Value;

                    _loading = false;
                }
                Static.InitializationSteps.IsPreferencesLoaded = true;
            });
        }
    }
}
