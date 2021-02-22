using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.Libs;
using DiscordStatusGUI.Libs.DiscordApi;
using DiscordStatusGUI.Models;
using DiscordStatusGUI.Properties;
using DiscordStatusGUI.ViewModels;
using DiscordStatusGUI.ViewModels.Dialogs;
using DiscordStatusGUI.ViewModels.Tabs;
using DiscordStatusGUI.Views;
using DiscordStatusGUI.Views.Discord;
using DiscordStatusGUI.Views.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DiscordStatusGUI.Locales;

namespace DiscordStatusGUI
{
    class Static
    {
        public static MainWindow MainWindow;
        public static MainWindowViewModel MainWindowViewModel;

        public static readonly Discord Discord = new Discord();

        public static readonly string Title = "Discord Status";
        public static readonly ImageSource Icon = BitmapEx.ToImageSource(Resources.logo.ToBitmap());
        public static readonly double Version;
        public static ObservableCollection<VerticalTabItem> Tabs;
        public static ResourceDictionary DiscordTheme { get; private set; }

        #region Notifications
        public static Notification CustomStatusOverride { get; private set; }
        public static Notification UpdateAvailable { get; private set; }
        public static Notification Reconnect { get; private set; }

        public static async void InitNotifications()
        {
            CustomStatusOverride = new Notification(Lang.GetResource("Static:CustomStatusOverride:Title"), Lang.GetResource("Static:CustomStatusOverride:Description"), false)
            {
                TitleForeground = new SolidColorBrush(Colors.Red),
                IsClosable = false,
                LinkText = Lang.GetResource("Static:CustomStatusOverride:LinkText"),
                LinkAction = async () =>
                {
                    await Task.Run(() =>
                    {
                        CustomStatusOverride.link.Dispatcher.Invoke(() => CustomStatusOverride.link.IsEnabled = false);
                        Discord.SetCustomStatus();
                        CustomStatusOverride.link.Dispatcher.Invoke(() => CustomStatusOverride.link.IsEnabled = true);
                    });
                }
            };
            UpdateAvailable = new Notification(Lang.GetResource("Static:UpdateAvailable:Title"), Lang.GetResource("Static:UpdateAvailable:Description"), false);
            Reconnect = new Notification(Lang.GetResource("Static:Reconnect:Title"), Lang.GetResource("Static:Reconnect:Description"), false) { TitleForeground = new SolidColorBrush(Colors.Red), IsClosable = false };

            MainWindow.Notifications.AddNotification(CustomStatusOverride);
            MainWindow.Notifications.AddNotification(UpdateAvailable);
            MainWindow.Notifications.AddNotification(Reconnect);

            await Task.Run(() =>
            {
                if (UpdateManager.IsUpdateAvailable(out _, out string tagname))
                {
                    UpdateAvailable.Dispatcher.Invoke(() =>
                    {
                        UpdateAvailable.LinkText = Lang.GetResource("Static:UpdateAvailable:LinkText").Replace("{version}", tagname);
                        UpdateAvailable.LinkAction = new Action(() => TemplateViewModel.OpenLink(UpdateManager.download_latest));
                    });
                    UpdateAvailable.IsVisible = true;
                    MainWindow.NotifyPopup.ShowBalloon(3000, Lang.GetResource("Static:UpdateAvailable:Title"), Lang.GetResource("Static:UpdateAvailable:Description"), System.Windows.Forms.ToolTipIcon.Info, UpdateAvailable.LinkAction);
                }
            });

            Discord.Socket.OnUserSettingsChanged += Socket_OnUserSettingsChanged;
        }

        private static void Socket_OnUserSettingsChanged(string eventtype, object rawdata, PinkJson.Json data)
        {
            if (data.IndexByKey("custom_status") != -1)
            {
                CustomStatusOverride.IsVisible = data["custom_status"].Value != null;
            }
        }
        #endregion

        static Static()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Version = ver.Major + ver.Minor * 0.1 + ver.Build * 0.01 + ver.Revision * 0.001;
        }

        public static void Init()
        {
            DiscordTheme = Application.Current.Resources;

            Tabs = new ObservableCollection<Models.VerticalTabItem>()
            {
                new VerticalTabItem("/DiscordStatusGUI;component/Resources/Tabs/GameStatus.png", 0.6, Lang.GetResource("Static:Tabs:GameStatus"), new Views.Tabs.GameStatus()),
                new VerticalTabItem("/DiscordStatusGUI;component/Resources/Tabs/Settings.png", 0.5, Lang.GetResource("Static:Tabs:Settings"), new Views.Tabs.Settings()),
                new VerticalTabItem("/DiscordStatusGUI;component/Resources/Tabs/Windows.png", 0.6, "Windows", new Views.Tabs.Windows()),
                new VerticalTabItem("/DiscordStatusGUI;component/Resources/Tabs/Warface.png", 0.6, "Warface RU", new Views.Tabs.Warface())
            };

            Discord.Socket.AutoReconnect += Socket_AutoReconnect;
            Static.Discord.Socket.OnUserInfoChanged += Socket_OnUserInfoChanged;
        }

        private static void Socket_OnUserInfoChanged(string eventtype, object rawdata, UserInfo data)
        {
            if (eventtype == "READY")
                Reconnect.IsVisible = false;
        }

        private static void Socket_AutoReconnect()
        {
            Reconnect.IsVisible = true;
        }

        #region Activity
        private static Activity[] _Activities;
        public static IEnumerable<PropertyInfo> ActivityFields = typeof(Activity).GetProperties();

        public static Activity[] Activities
        {
            get => _Activities ?? (_Activities = new Activity[]
            {
                new Activity()
                {
                    ProfileName = "Discord Status",
                    Name = "Discord Status",
                    ApplicationID = "743507332838981723",
                    State = "vlas-omsk.github.io",
                    ImageLargeKey = "logo",
                    IsAvailableForChange = false
                },
                new Activity() { ProfileName = "Profile1", IsAvailableForChange = true },
                new Activity() { ProfileName = "Profile2", IsAvailableForChange = true },
                new Activity() { ProfileName = "Profile3", IsAvailableForChange = true },
                new Activity() { ProfileName = "Profile4", IsAvailableForChange = true },
                new Activity() { ProfileName = "Profile5", IsAvailableForChange = true },
                new Activity()
                {
                    ProfileName = "Warface",
                    Name = "{wf:AppName} ({wf:ServerName}: {wf:PlayerNickname})",
                    ApplicationID = "{wf:AppID}",
                    State = "Карта: {wf:Map}",
                    Details = "{wf:State}",
                    StartTime = "{wf:StateStartTime}",
                    ImageLargeKey = "logo",
                    ImageLargeText = "Warface RU",
                    ImageSmallKey = "rank{wf:PlayerRank}",
                    ImageSmallText = "Ранк: {wf:PlayerRank} {wf:PlayerRankName}",
                    IsAvailableForChange = true
                }
            });
            set
            {
                _Activities = value;
                OnActivitiesChanged?.Invoke();
            }
        }

        public static string GetValueByFieldName(string name)
        {
            switch (name)
            {
                case "wf:AppName": return "Warface";
                case "wf:AppID": return "735872877148110939";
                case "wf:Map": return WarfaceApi.CurrentGameState.Map;
                case "wf:State": return WarfaceApi.CurrentGameState.Screen;
                case "wf:StateStartTime": return WarfaceApi.CurrentGameState.Since.ToString();
                case "wf:InGameServerName": return WarfaceApi.CurrentGameState.Server;
                case "wf:ServerIP": return WarfaceApi.CurrentPlayer.OnlineServer;
                case "wf:ServerName": return WarfaceApi.CurrentPlayer.Server.ToString();
                case "wf:ServerRegion": return WarfaceApi.CurrentPlayer.Region;
                case "wf:PlayerNickname": return WarfaceApi.CurrentPlayer.Nickname;
                case "wf:PlayerRank": return WarfaceApi.CurrentPlayer.Rank.ToString();
                case "wf:PlayerRankName": return WarfaceApi.CurrentPlayer.RankName;
                case "wf:PlayerUserID": return WarfaceApi.CurrentPlayer.Uid.ToString();

                case "win:ForegroundWindowName": return ProcessEx.ForegroundWindowProcess?.MainWindowTitle;
                case "win:ForegroundWindowProcessName": return ProcessEx.ForegroundWindowProcess?.ProcessName;
            }

            return name;//"You are idiot?";
        }

        public static bool IsPrefixContainsInFields(Activity activity, string prefix)
        {
            return ActivityFields.Any((fi) => (fi.GetValue(activity)?.ToString() + "").Contains("{" + prefix));
        }

        public static Activity ReplaceFilds(Activity activity)
        {
            object result = new Activity();
            ActivityFields.ToList().ForEach((fi) =>
            {
                var val = fi.GetValue(activity);
                if (!(val is string))
                {
                    fi.SetValue(result, val);
                    return;
                }

                fi.SetValue(result, ReplaceFilds(val.ToString() + ""));
            });
            return (Activity)result;
        }

        public static string ReplaceFilds(string value)
        {
            value = value ?? "";
            var pattern = @"\{(.*?)\}";
            var matches = Regex.Matches(value, pattern);

            foreach (Match m in matches)
            {
                value = value.Replace($"{{{m.Groups[1].Value}}}", GetValueByFieldName(m.Groups[1].Value));
            }

            return value;
        }

        private static int _CurrentActivityIndex;
        public static int CurrentActivityIndex
        {
            get => _CurrentActivityIndex;
            set
            {
                if (_CurrentActivityIndex != value)
                {
                    _CurrentActivityIndex = value;
                    UpdateDiscordActivity();
                }
            }
        }
        public static void UpdateDiscordActivity()
        {
            Static.Discord.Socket.UpdateActivity(ReplaceFilds(Activities[_CurrentActivityIndex]));
            Activities[_CurrentActivityIndex].SavedState = Activities[_CurrentActivityIndex];
            OnActivityChanged?.Invoke();
        }
        public static ref Activity CurrentActivity { get => ref Activities[CurrentActivityIndex]; }
        public static ref Activity GetActivityByName(string profilename)
        {
            for (var i = 0; i < Activities.Length; i++)
                if (Activities[i].ProfileName == profilename)
                    return ref Activities[i];
            return ref CurrentActivity;
        }

        public delegate void OnActivitiesChangedEventHandler();
        public static event OnActivitiesChangedEventHandler OnActivitiesChanged;

        public delegate void OnActivityChangedChangedEventHandler();
        public static event OnActivityChangedChangedEventHandler OnActivityChanged;
        #endregion

        public struct Accounts
        {
            public const int pageindex = 1;

            public static bool MyGames
            {
                get => (Tabs[pageindex].Page.DataContext as SettingsViewModel).IsMyGamesAccountLogined;
                set
                {
                    (Tabs[pageindex].Page.DataContext as SettingsViewModel).IsMyGamesAccountLogined = value;
                    OnChange("MyGames");
                }
            }

            public static bool Discord
            {
                get => (Tabs[pageindex].Page.DataContext as SettingsViewModel).IsDiscordAccountLogined;
                set
                {
                    (Tabs[pageindex].Page.DataContext as SettingsViewModel).IsDiscordAccountLogined = value;
                    OnChange("Discord");
                }
            }

            public static void OnChange(string name)
            {
                Preferences.Save();
            }
        }

        public struct Window
        {
            private static WindowState laststate;

            public static void Minimize()
            {
                Static.MainWindow.WindowState = WindowState.Minimized;
            }

            public static void Maximize()
            {
                if (Static.MainWindow.WindowState == WindowState.Maximized)
                    Static.MainWindow.WindowState = WindowState.Normal;
                else
                    Static.MainWindow.WindowState = WindowState.Maximized;
            }

            public static void Normalize()
            {
                Static.MainWindow.Show();
                if (laststate != (WindowState)(-1))
                {
                    Static.MainWindow.WindowState = laststate;
                    laststate = (WindowState)(-1);
                }
                Static.MainWindow.Activate();
            }

            public static void Close()
            {
                Static.MainWindow.Hide();
                laststate = Static.MainWindow.WindowState;
                Static.MainWindow.WindowState = WindowState.Minimized;
            }

            public static void SetTopStatus(string msg)
            {
                ConsoleEx.WriteLine(ConsoleEx.Message, msg);

                MainWindow?.Dispatcher.Invoke(() =>
                    MainWindow.toppanel.TopStatus.Content = msg);
            }
        }

        public struct InitializationSteps
        {
            private static List<bool> _InitializationSteps = new List<bool>(new bool[] { false, false, false, false, false });
            private static bool _FirstInitialization = true;

            public static bool IsLanguageInitialized
            {
                get => _InitializationSteps[0];
                set
                {
                    _InitializationSteps[0] = value;
                    OnStepInitialized("Language loaded");
                }
            }
            public static bool IsProcessExInitialized
            {
                get => _InitializationSteps[1];
                set
                {
                    _InitializationSteps[1] = value;
                    OnStepInitialized("ProcessEx loaded");
                }
            }
            public static bool IsWarfaceApiInitialized
            {
                get => _InitializationSteps[2];
                set
                {
                    _InitializationSteps[2] = value;
                    OnStepInitialized("WarfaceApi loaded");
                }
            }
            public static bool IsPreferencesLoaded
            {
                get => _InitializationSteps[3];
                set
                {
                    _InitializationSteps[3] = value;
                    OnStepInitialized("Preferences loaded");
                }
            }
            public static bool IsProfilesLoaded
            {
                get => _InitializationSteps[4];
                set
                {
                    _InitializationSteps[4] = value;
                    OnStepInitialized("Profiles loaded");
                }
            }


            public static void OnStepInitialized(string msg)
            {
                if (_InitializationSteps.TrueForAll(match => { return match; }))
                {
                    Window.SetTopStatus("");
                    IsInitialized = true;
                }
                else
                    Window.SetTopStatus(msg);
            }

            public static bool IsInitialized
            {
                get => MainWindow.initialization != null && MainWindow.initialization.Visibility != Visibility.Visible;
                set
                {
                    MainWindow.Dispatcher.Invoke(() =>
                    {
                        if (value)
                        {
                            Animations.VisibleOff(MainWindow.initialization).Begin();
                            if (_FirstInitialization)
                            {
                                _FirstInitialization = false;
                                OnFirstInitialization();
                            }
                        }
                        else
                            Animations.VisibleOn(MainWindow.initialization).Begin();
                    });
                }
            }

            private static void OnFirstInitialization()
            {
                DiscordLoginSuccessful();

                FirstInitialization?.Invoke();
            }

            public delegate void OnActivitiesChangedEventHandler();
            public static event OnActivitiesChangedEventHandler FirstInitialization;
        }

        public struct Dialogs
        {
            //Dialogs.MessageBoxShow("Hello", "Click Ok", new ObservableCollection<ButtonItem>() { Dialogs.ButtonOk }, HorizontalAlignment.Right, null, "/DiscordStatusGUI;component/Resources/Tabs/Command.png");
            public static bool IsMessageBoxShowed { get; private set; } = false;
            public static bool IsDateTimePickerShowed { get; private set; } = false;

            public static void MessageBoxShow(string title = "", string text = "", ObservableCollection<ButtonItem> buttons = null, HorizontalAlignment buttonsaligment = HorizontalAlignment.Right, Action back = null, string imagepath = "", double imagescale = 0.5, double width = 440, double height = 160)
            {
                MainWindow.Dispatcher.Invoke(() =>
                {
                    var msg = new Views.Dialogs.MessageBox();
                    var dc = (msg.DataContext as MessageBoxViewModel);
                    dc.Title = title;
                    dc.Text = text;
                    dc.BackCommand = new Command(back);
                    dc.Buttons = buttons;
                    dc.ButtonsAligment = buttonsaligment;
                    dc.Width = width;
                    dc.Height = height;
                    dc.ImagePath = imagepath;
                    dc.ImageScale = imagescale;
                    MainWindow.messagebox.Content = msg;
                    if (!IsMessageBoxShowed)
                    {
                        Animations.VisibleOnZoom((MainWindow.messagebox.Content as Views.Dialogs.MessageBox).body).Begin();
                        Animations.VisibleOnOpacity((MainWindow.messagebox.Content as Views.Dialogs.MessageBox).background).Begin();
                    }
                    else
                    {
                        (MainWindow.messagebox.Content as Views.Dialogs.MessageBox).body.Visibility = Visibility.Visible;
                        (MainWindow.messagebox.Content as Views.Dialogs.MessageBox).background.Visibility = Visibility.Visible;
                    }
                    IsMessageBoxShowed = true;
                });
            }

            public static void MessageBoxHide()
            {
                MainWindow.Dispatcher.Invoke(() =>
                {
                    Animations.VisibleOffZoom((MainWindow.messagebox.Content as Views.Dialogs.MessageBox).body).Begin();
                    Animations.VisibleOffOpacity((MainWindow.messagebox.Content as Views.Dialogs.MessageBox).background).Begin();
                    IsMessageBoxShowed = false;
                });
            }


            public static void DateTimePickerShow(DateTime dt, ObservableCollection<ButtonItem> buttons = null, HorizontalAlignment buttonsaligment = HorizontalAlignment.Right, Action back = null, string imagepath = "", double imagescale = 0.5, double width = 440, double height = 160)
            {
                MainWindow.Dispatcher.Invoke(() =>
                {
                    var msg = new Views.Dialogs.DateTimePicker();
                    var dc = (msg.DataContext as DateTimePickerViewModel);
                    dc.BackCommand = new Command(back);
                    dc.Buttons = buttons;
                    dc.ButtonsAligment = buttonsaligment;
                    dc.Width = width;
                    dc.Height = height;
                    dc.SetDateTime(dt);
                    MainWindow.datetimepicker.Content = msg;
                    if (!IsDateTimePickerShowed)
                    {
                        Animations.VisibleOnZoom((MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).body).Begin();
                        Animations.VisibleOnOpacity((MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).background).Begin();
                    }
                    else
                    {
                        (MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).body.Visibility = Visibility.Visible;
                        (MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).background.Visibility = Visibility.Visible;
                    }
                    IsDateTimePickerShowed = true;
                });
            }

            public static void DateTimePickerHide()
            {
                MainWindow.Dispatcher.Invoke(() =>
                {
                    Animations.VisibleOffZoom((MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).body).Begin();
                    Animations.VisibleOffOpacity((MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).background).Begin();
                    IsDateTimePickerShowed = false;
                });
            }

            public static DateTime? GetLastDateTime()
            {
                var vm = ((MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).DataContext as DateTimePickerViewModel);
                return vm?.GetDateTime();
            }


            public static ButtonItem ButtonOk { get => new ButtonItem(MessageBoxHide, Lang.GetResource("Ok")); }
            public static ButtonItem ButtonApply { get => new ButtonItem(DateTimePickerHide, Lang.GetResource("Apply")); }
        }


        public static void DiscordLoginSuccessful()
        {
            if (Discord.IsTokenValid(Discord?.Token))
                MainWindow.Dispatcher.Invoke(() =>
                {
                    MainWindow.ReplaceWithWaves(new VerticalTabControl());
                    Accounts.Discord = true;
                });
            else if (!string.IsNullOrEmpty(Discord?.Token))
            {
                Dialogs.MessageBoxShow(Lang.GetResource("Static:InvalidDiscordToken:Title"), Lang.GetResource("Static:InvalidDiscordToken:Text"), new ObservableCollection<ButtonItem>() { Dialogs.ButtonOk }, HorizontalAlignment.Right, null, "/DiscordStatusGUI;component/Resources/PixelCat/Lying2.png");
                MainWindow.Dispatcher.Invoke(() =>
                {
                    CurrentPage = new Login();
                });
            }
            else
                MainWindow.Dispatcher.Invoke(() =>
                {
                    CurrentPage = new Login();
                });
        }

        public static UserControl CurrentPage
        {
            get => MainWindowViewModel.CurrentPage;
            set => MainWindowViewModel.CurrentPage = value;
        }

        public static void OnGameStatusChanged(bool opened, int SelectedProfileIndex, ref int SavedLastProfileIndex, ref bool SavedState)
        {
            void GameStatusViewModel_ProfilesComboBoxEnabled(bool value)
            {
                Static.MainWindow.Dispatcher.Invoke(() => (Static.Tabs[0].Page as GameStatus).ProfilesComboBox_IsEnabled(value, true));
            }

            if (SelectedProfileIndex < Static.Activities.Length)
            {
                if (opened)
                {
                    if (SavedLastProfileIndex == -1)
                        SavedLastProfileIndex = Static.CurrentActivityIndex;
                    Static.CurrentActivityIndex = SelectedProfileIndex;
                    GameStatusViewModel_ProfilesComboBoxEnabled(false);
                }
                else
                {
                    if (SavedLastProfileIndex >= 0)
                        Static.CurrentActivityIndex = SavedLastProfileIndex;
                    SavedLastProfileIndex = -1;
                    GameStatusViewModel_ProfilesComboBoxEnabled(true);
                }
                SavedState = opened;
            }
            else
            {
                if (SavedLastProfileIndex >= 0)
                    Static.CurrentActivityIndex = SavedLastProfileIndex;
                GameStatusViewModel_ProfilesComboBoxEnabled(true);
            }
        }
    }
}
