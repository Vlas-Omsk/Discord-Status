using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.Libs.DiscordApi;
using DiscordStatusGUI.Views.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Effects;
using PinkJson;
using System.Windows;
using DiscordStatusGUI.Views.Discord;

namespace DiscordStatusGUI.ViewModels.Tabs
{
    class SettingsViewModel : TemplateViewModel
    {
        private ImageSource _DiscordUserAvatar = BitmapEx.ToImageSource(Properties.Resources.DefaultAvatar);
        public ImageSource DiscordUserAvatar
        {
            get => _DiscordUserAvatar;
            set
            {
                _DiscordUserAvatar = value;
                OnPropertyChanged("DiscordUserAvatar");
            }
        }
        private string _DiscordUserName = "UserName";
        public string DiscordUserName
        {
            get => _DiscordUserName;
            set
            {
                _DiscordUserName = value;
                OnPropertyChanged("DiscordUserName");
            }
        }
        private string _DiscordUserTag = "1234";
        public string DiscordUserTag
        {
            get => _DiscordUserTag;
            set
            {
                _DiscordUserTag = value;
                OnPropertyChanged("DiscordUserTag");
            }
        }
        private string _DiscordUserEmail = "UserName@discord.com";
        public string DiscordUserEmail
        {
            get => _DiscordUserEmail;
            set
            {
                _DiscordUserEmail = value;
                OnPropertyChanged("DiscordUserEmail");
            }
        }
        public int _SelectedUserStatusIndex = 0;
        public int SelectedUserStatusIndex
        {
            get => _SelectedUserStatusIndex;
            set
            {
                _SelectedUserStatusIndex = value;
                OnPropertyChanged("SelectedUserStatusIndex");

                Static.Discord.Socket.CurrentUserStatus = (UserStatus)value;
                Static.UpdateDiscordActivity();
                Preferences.Save();
            }
        }


        private bool _IsDiscordConnected;
        public bool IsDiscordConnected
        {
            get => _IsDiscordConnected;
            set
            {
                _IsDiscordConnected = value;
                if (!_IsDiscordConnected)
                {
                    Static.Discord.Socket.Disconnect();
                    Static.Reconnect.IsVisible = false;
                    SettingsView.ShowUserInfoPlug();
                }
                else
                {
                    DiscordConnectedSwitcherEnable = false;
                    Static.Discord.Socket.Connect();
                }
                OnPropertyChanged("IsDiscordConnected");
            }
        }
        private bool _DiscordConnectedSwitcherEnable = false;
        public bool DiscordConnectedSwitcherEnable
        {
            get => _DiscordConnectedSwitcherEnable;
            set
            {
                _DiscordConnectedSwitcherEnable = value;
                OnPropertyChanged("DiscordConnectedSwitcherEnable");
            }
        }

        
        private bool _IsDiscordAccountLogined = false;
        public bool IsDiscordAccountLogined
        {
            get => _IsDiscordAccountLogined;
            set
            {
                _IsDiscordAccountLogined = value;
                OnPropertyChanged("IsDiscordAccountLogined");
                OnPropertyChanged("DiscordAccountButtonStyle");
                DiscordConnectedSwitcherEnable = _IsDiscordAccountLogined;
            }
        }
        public Style DiscordAccountButtonStyle
        {
            get => _IsDiscordAccountLogined ? _GreenButton : _BlueButton;
        }
        private bool _IsMyGamesAccountLogined = false;
        public bool IsMyGamesAccountLogined
        {
            get => _IsMyGamesAccountLogined;
            set
            {
                _IsMyGamesAccountLogined = value;
                OnPropertyChanged("IsMyGamesAccountLogined");
                OnPropertyChanged("MyGamesAccountButtonStyle");
            }
        }
        public Style SteamAccountButtonStyle
        {
            get => !string.IsNullOrEmpty(Libs.SteamApi.CurrentSteamProfile.SteamLoginSecure) ? _GreenButton : _BlueButton;
        }


        public bool IsAutoRunEnabled
        {
            get
            {
                if (RegistryCommands.AutoRunStatus() == RegistryCommands.AutoRun.OtherPath)
                    RegistryCommands.CreateAutoRun();
                if (RegistryCommands.AutoRunStatus() == RegistryCommands.AutoRun.Registered)
                    return true;
                else return false;
            }
            set
            {
                if (value)
                    RegistryCommands.CreateAutoRun();
                else
                    RegistryCommands.RemoveAutoRun();
                OnPropertyChanged("IsAutoRunEnabled");
            }
        }
        public Style MyGamesAccountButtonStyle
        {
            get => _IsMyGamesAccountLogined ? _GreenButton : _BlueButton;
        }

        public string Info
        {
            get => Static.Title + " v" + Static.Version.ToString().Replace(',', '.');
        }

        public Settings SettingsView;

        public static Style _BlueButton = Static.GetResource<Style>("BlueButton");
        public static Style _GreenButton = Static.GetResource<Style>("GreenButton");

        public Command DiscordLoginCommand { get; private set; }
        public Command SteamLoginCommand { get; private set; }

        public SettingsViewModel()
        {
            DiscordLoginCommand = new Command(DiscordLogin);
            SteamLoginCommand = new Command(SteamLogin);

            Static.Discord.Socket.UserInfoChanged += Socket_OnUserInfoChanged;
            Static.Discord.Socket.AutoReconnect += Socket_AutoReconnect;

            Libs.SteamApi.CurrentSteamProfile.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SteamLoginSecure")
                    OnPropertyChanged("SteamAccountButtonStyle");
            };
        }

        private void Socket_AutoReconnect(object sender, EventArgs e)
        {
            DiscordConnectedSwitcherEnable = true;
            SettingsView.HideUserInfoPlug();
        }

        private void DiscordLogin()
        {
            void Yes()
            {
                IsDiscordConnected = false;
                Static.CurrentPage = new Login();
                Static.Dialogs.MessageBoxHide();
            }

            void No()
            {
                Static.Dialogs.MessageBoxHide();
            }

            if (IsDiscordAccountLogined)
                Static.Dialogs.MessageBoxShow(Locales.Lang.GetResource("ViewModels:Tabs:SettingsViewModel:ChangeDiscordAccount:Title"), Locales.Lang.GetResource("ViewModels:Tabs:SettingsViewModel:ChangeDiscordAccount:Text"),
                    new System.Collections.ObjectModel.ObservableCollection<Models.ButtonItem>() { new Models.ButtonItem(Yes, Locales.Lang.GetResource("Yes")), new Models.ButtonItem(No, Locales.Lang.GetResource("No")) },
                    HorizontalAlignment.Right, No);
            else
                Static.CurrentPage = new Login();
        }

        private void SteamLogin()
        {
            Static.Dialogs.PopupShow(new Views.Popups.SteamLogin(), Static.Dialogs.PopupHide);
        }


        private void Socket_OnUserInfoChanged(object sender, DiscordEventArgs<UserInfo> e)
        {
            if (e.EventType == "READY")
            {
                DiscordUserName = e.Data.UserName;
                DiscordUserTag = e.Data.Discriminator;
                DiscordUserEmail = e.Data.Email;
                if (e.Data.AvatarId != null)
                    SettingsView.Dispatcher.Invoke(() =>
                        DiscordUserAvatar = BitmapEx.ToImageSource(Libs.DiscordApi.Discord.GetUserAvatar(e.Data.Id, e.Data.AvatarId, 128)));

                DiscordConnectedSwitcherEnable = true;
                SettingsView.HideUserInfoPlug();
            }
        }
    }
}
