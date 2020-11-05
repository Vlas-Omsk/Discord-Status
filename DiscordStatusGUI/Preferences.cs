using DiscordStatusGUI.Libs;
using DiscordStatusGUI.Models;
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
        public static int CurrentActivityIndex { get => Static.CurrentActivityIndex; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[0].Page as GameStatus).DataContext as GameStatusViewModel).SelectedProfileIndex = value); }
        public static bool FastGameClientClose { get => WarfaceApi.FastGameClientClose; set => Static.MainWindow.Dispatcher.Invoke(() => ((Static.Tabs[3].Page as Warface).DataContext as WarfaceViewModel).IsFastGameClientClose = value); }

        public static bool Loading { get; private set; } = false;
        public static bool LoadingProfiles { get; private set; } = false;

        public static void LoadProfiles()
        {
            if (File.Exists("profiles.json"))
            {
                LoadingProfiles = true;

                Static.Activities = JsonObjectArray.ToArray<Libs.DiscordApi.Activity>(new JsonObjectArray(File.ReadAllText("profiles.json")));

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
                    FastGameClientClose
                });

                File.WriteAllText("preferences.json", propjson.ToFormatString());
            });
        }

        public static void Load()
        {
            if (File.Exists("preferences.json"))
            {
                Loading = true;

                try
                {
                    var propjson = new Json(File.ReadAllText("preferences.json"));

                    Discord_Token = propjson["Accounts"]["Discord"]["token"].Value.ToString();
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
    }
}
