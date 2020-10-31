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
using PinkJson.Parser;
using System.Windows;
using DiscordStatusGUI.Views.Discord;

namespace DiscordStatusGUI.ViewModels.Tabs
{
    class SettingsViewModel : TemplateViewModel
    {
        private ImageSource _DiscordUserAvatar = BitmapEx.ToImageSource(DiscordStatusGUI.Properties.Resources.DefaultAvatar);
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
        public Style MyGamesAccountButtonStyle
        {
            get => _IsMyGamesAccountLogined ? _GreenButton : _BlueButton;
        }

        public Settings SettingsView;

        public Style _BlueButton { get; private set; }
        public Style _GreenButton { get; private set; }

        public Command DiscordLoginCommand { get; private set; }

        public SettingsViewModel()
        {
            var resd = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Themes/DiscordTheme.xaml") };
            _BlueButton = (Style)resd["BlueButton"];
            _GreenButton = (Style)resd["GreenButton"];

            DiscordLoginCommand = new Command(DiscordLogin);

            Static.Discord.Socket.OnUserInfoChanged += Socket_OnUserInfoChanged;
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
                Static.Dialogs.MessageBoxShow("Discord Account", "You are already signed in to your account, would you like to change it?",
                    new System.Collections.ObjectModel.ObservableCollection<Models.ButtonItem>() { new Models.ButtonItem(Yes, "Yes"), new Models.ButtonItem(No, "No") },
                    HorizontalAlignment.Right, No);
            else
                Static.CurrentPage = new Login();
        }


        private void Socket_OnUserInfoChanged(string eventtype, object data, UserInfo userInfo)
        {
            if (eventtype == "READY")
            {
                DiscordUserName = userInfo.UserName;
                DiscordUserTag = userInfo.Discriminator;
                DiscordUserEmail = userInfo.Email;
                if (userInfo.AvatarId != null)
                    SettingsView.Dispatcher.Invoke(() =>
                        DiscordUserAvatar = BitmapEx.ToImageSource(Libs.DiscordApi.Discord.GetUserAvatar(userInfo.Id, userInfo.AvatarId, 128)));

                DiscordConnectedSwitcherEnable = true;
                SettingsView.HideUserInfoPlug();
            }
        }
    }
}
