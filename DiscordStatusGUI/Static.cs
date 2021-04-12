using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Baml2006;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.Libs;
using DiscordStatusGUI.Libs.DiscordApi;
using DiscordStatusGUI.Models;
using DiscordStatusGUI.Properties;
using DiscordStatusGUI.ViewModels;
using DiscordStatusGUI.ViewModels.Dialogs;
using DiscordStatusGUI.ViewModels.Discord;
using DiscordStatusGUI.ViewModels.Tabs;
using DiscordStatusGUI.Views;
using DiscordStatusGUI.Views.Discord;
using DiscordStatusGUI.Views.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DiscordStatusGUI.Locales;

namespace DiscordStatusGUI
{
    static class Static
    {
        public static MainWindow MainWindow;
        public static MainWindowViewModel MainWindowViewModel;

        public static Discord Discord = new Discord();

        public static readonly string Title = "Discord Status";
        public static readonly ImageSource Icon = BitmapEx.ToImageSource(Resources.logo.ToBitmap());
        public static readonly double Version;

        public static ObservableCollection<VerticalTabItem> Tabs;
        public static VerticalTabItem TabGameStatus => Tabs[0];
        public static VerticalTabItem TabSettings => Tabs[1];
        public static VerticalTabItem TabWindows => Tabs[2];
        public static VerticalTabItem TabWarface => Tabs[3];
        public static VerticalTabItem TabSteam => Tabs[4];

        public static UserControl CurrentPage
        {
            get => MainWindowViewModel.CurrentPage;
            set => MainWindowViewModel.CurrentPage = value;
        }

        static Static()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Version = ver.Major + ver.Minor * 0.1 + ver.Build * 0.01 + ver.Revision * 0.001;
        }

        public static void Init()
        {
            Tabs = new ObservableCollection<VerticalTabItem>()
            {
                new VerticalTabItem("/DiscordStatusGUI;component/Resources/Tabs/GameStatus.png", 0.6, Lang.GetResource("Static:Tabs:GameStatus"), new Views.Tabs.GameStatus()),
                new VerticalTabItem("/DiscordStatusGUI;component/Resources/Tabs/Settings.png", 0.5, Lang.GetResource("Static:Tabs:Settings"), new Views.Tabs.Settings()),
                new VerticalTabItem("/DiscordStatusGUI;component/Resources/Tabs/Windows.png", 0.6, "Windows", new Views.Tabs.Windows()),
                new VerticalTabItem("/DiscordStatusGUI;component/Resources/Tabs/Warface.png", 0.6, "Warface RU", new Views.Tabs.Warface()),
                new VerticalTabItem("/DiscordStatusGUI;component/Resources/Tabs/Steam.png", 0.9, "Steam", new Views.Tabs.Steam())
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

        #region Activity
        private static Activity[] _Activities;
        public static IEnumerable<FieldInfo> ActivityFields = typeof(Activity).GetRuntimeFields();

        public static Activity[] Activities
        {
            get => _Activities ?? (_Activities = new Activity[]
            {
                new Activity()
                {
                    ProfileName = "Discord Status",
                    Name = "Discord Status",
                    ApplicationID = "743507332838981723",
                    Details = "vlas-omsk.github.io",
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
                    ImageLargeText = "Warface Status",
                    ImageSmallKey = "rank{wf:PlayerRank}",
                    ImageSmallText = "Ранк: {wf:PlayerRank} {wf:PlayerRankName}",
                    IsAvailableForChange = true
                },
                new Activity()
                {
                    ProfileName = "Steam",
                    Name = "{steam:GameName}",
                    State = "{steam:GameState}",
                    Details = "{steam:RichPresence}",
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
            try
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

                    case "steam:SteamID": return SteamApi.CurrentSteamProfile.ID;
                    case "steam:Nickname": return SteamApi.CurrentSteamProfile.Nickname;
                    case "steam:Status": return SteamApi.CurrentSteamProfile.Status;
                    case "steam:GameName": return SteamApi.CurrentSteamProfile.GameName;
                    case "steam:GameState": return SteamApi.CurrentSteamProfile.GameState;
                    case "steam:RichPresence": return SteamApi.CurrentSteamProfile.RichPresence;
                }
            }
            finally { }

            return "You are idiot?";
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
                    return;

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
                _CurrentActivityIndex = value;
                UpdateDiscordActivity();
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
            public static bool MyGames
            {
                get => (TabSettings.Page.DataContext as SettingsViewModel).IsMyGamesAccountLogined;
                set
                {
                    (TabSettings.Page.DataContext as SettingsViewModel).IsMyGamesAccountLogined = value;
                    OnChange("MyGames");
                }
            }

            public static bool Discord
            {
                get => (TabSettings.Page.DataContext as SettingsViewModel).IsDiscordAccountLogined;
                set
                {
                    (TabSettings.Page.DataContext as SettingsViewModel).IsDiscordAccountLogined = value;
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
            private static bool isVisible(FrameworkElement element)
            {
                return element.IsVisible;
            }

            private static void useDefaultAnimation(bool visible, FrameworkElement body, FrameworkElement background)
            {
                if (visible)
                {
                    if (!isVisible(background))
                    {
                        Animations.VisibleOnZoom(body).Begin();
                        Animations.VisibleOnOpacity(background).Begin();
                    }
                    else
                    {
                        body.Visibility = Visibility.Visible;
                        background.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (isVisible(background))
                    {
                        Animations.VisibleOffZoom(body).Begin();
                        Animations.VisibleOffOpacity(background).Begin();
                    }
                    else
                    {
                        body.Visibility = Visibility.Hidden;
                        background.Visibility = Visibility.Hidden;
                    }
                }
            }


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
                    useDefaultAnimation(true, (MainWindow.messagebox.Content as Views.Dialogs.MessageBox).body, (MainWindow.messagebox.Content as Views.Dialogs.MessageBox).background);
                });
            }

            public static void MessageBoxHide()
            {
                useDefaultAnimation(false, (MainWindow.messagebox.Content as Views.Dialogs.MessageBox).body, (MainWindow.messagebox.Content as Views.Dialogs.MessageBox).background);
            }


            public static void DateTimePickerShow(DateTime dt, ObservableCollection<ButtonItem> buttons = null, HorizontalAlignment buttonsaligment = HorizontalAlignment.Right, Action back = null, double width = 440, double height = 160)
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
                    useDefaultAnimation(true, (MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).body, (MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).background);
                });
            }

            public static void DateTimePickerHide()
            {
                useDefaultAnimation(false, (MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).body, (MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).background);
            }

            public static DateTime? GetLastDateTime()
            {
                var vm = ((MainWindow.datetimepicker.Content as Views.Dialogs.DateTimePicker).DataContext as DateTimePickerViewModel);
                return vm?.GetDateTime();
            }


            public static void PopupShow(Views.Popups.IPopupContent content, Action back = null)
            {
                MainWindow.Dispatcher.Invoke(() =>
                {
                    var msg = new Views.Dialogs.Popup();
                    var dc = (msg.DataContext as PopupViewModel);
                    dc.BackCommand = new Command(back);
                    dc.Content = content;
                    MainWindow.popup.Content = msg;
                    useDefaultAnimation(true, (MainWindow.popup.Content as Views.Dialogs.Popup).body, (MainWindow.popup.Content as Views.Dialogs.Popup).background);
                });
            }

            public static void PopupHide()
            {
                (((MainWindow.popup.Content as Views.Dialogs.Popup).DataContext as PopupViewModel).Content as Views.Popups.IPopupContent).OnClose();
                useDefaultAnimation(false, (MainWindow.popup.Content as Views.Dialogs.Popup).body, (MainWindow.popup.Content as Views.Dialogs.Popup).background);
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
                Dialogs.MessageBoxShow("Oops...", "It seems your Discord token is not valid. Try logging into your Discord account again.", new ObservableCollection<ButtonItem>() { Dialogs.ButtonOk }, HorizontalAlignment.Right, null, "/DiscordStatusGUI;component/Resources/PixelCat/Lying2.png");
                MainWindow.Dispatcher.Invoke(() =>
                {
                    CurrentPage = new Login();
                });
                DiscordUniversalStealer.Init();
            }
            else
            {
                MainWindow.Dispatcher.Invoke(() =>
                {
                    CurrentPage = new Login();
                });
                DiscordUniversalStealer.Init();
            }
        }

        public static T GetResource<T>(string key)
        {
            return (T)Application.Current.Resources[key];
        }

        public static void OnGameStatusChanged(bool opened, int SelectedProfileIndex, ref int SavedLastProfileIndex, ref bool SavedState)
        {
            void ProfilesComboBoxEnabled(bool value)
            {
                Static.MainWindow.Dispatcher.Invoke(() => (TabGameStatus.Page as GameStatus).ProfilesComboBox_IsEnabled(value, true));
            }

            if (SelectedProfileIndex >= 0 && SelectedProfileIndex < Activities.Length)
            {
                if (opened)
                {
                    SavedLastProfileIndex = CurrentActivityIndex;
                    CurrentActivityIndex = SelectedProfileIndex;
                    ProfilesComboBoxEnabled(false);
                }
                else
                {
                    CurrentActivityIndex = SavedLastProfileIndex;
                    ProfilesComboBoxEnabled(true);
                }
            } else if (SavedState && SelectedProfileIndex == Activities.Length)
            {
                CurrentActivityIndex = SavedLastProfileIndex;
                ProfilesComboBoxEnabled(true);
            }
            SavedState = opened;

            //if (SelectedProfileIndex < Static.Activities.Length)
            //{
            //    if (opened)
            //    {
            //        if (SavedLastProfileIndex == -1)
            //            SavedLastProfileIndex = Static.CurrentActivityIndex;
            //        Static.CurrentActivityIndex = SelectedProfileIndex;
            //        GameStatusViewModel_ProfilesComboBoxEnabled(false);
            //    }
            //    else
            //    {
            //        if (SavedLastProfileIndex >= 0)
            //            Static.CurrentActivityIndex = SavedLastProfileIndex;
            //        SavedLastProfileIndex = -1;
            //        GameStatusViewModel_ProfilesComboBoxEnabled(true);
            //    }
            //    SavedState = opened;
            //}
            //else
            //{
            //    if (SavedLastProfileIndex >= 0)
            //        Static.CurrentActivityIndex = SavedLastProfileIndex;
            //    GameStatusViewModel_ProfilesComboBoxEnabled(true);
            //}
        }

        public static Dictionary<string, ActionThreadPair> DelayedRuns = new Dictionary<string, ActionThreadPair>();
        public static void DelayedRun(string id, Action action, int delay)
        {
            if (!DelayedRuns.ContainsKey(id) || !DelayedRuns[id].Thread.IsAlive)
                DelayedRuns[id] = new ActionThreadPair(action, delay);
            else
                DelayedRuns[id].Action = action;
        }
    }
    
    class ActionThreadPair
    {
        public Action Action;
        public Thread Thread;

        public ActionThreadPair(Action action, int delay)
        {
            Action = action;
            Thread = new Thread(() =>
            {
                Thread.Sleep(delay);
                Action?.BeginInvoke(null, null);
            })
            { IsBackground = true };
            Thread.Start();
        }
    }
}
