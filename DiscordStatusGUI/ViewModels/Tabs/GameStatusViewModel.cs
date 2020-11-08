﻿using DiscordStatusGUI.Extensions;
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
using System.Collections.ObjectModel;
using System.Reflection;

namespace DiscordStatusGUI.ViewModels.Tabs
{
    class GameStatusViewModel : TemplateViewModel
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
        public Activity[] Profiles
        {
            get => Static.Activities;
            set => Static.Activities = value;
        }
        public int SelectedProfileIndex
        {
            get => Static.CurrentActivityIndex;
            set
            {
                if (value == -1)
                    return;
                Static.CurrentActivityIndex = value;
                Preferences.Save();
            }
        }
        public Activity SelectedProfile
        {
            get => Static.CurrentActivity;
        }
        

        #region Options
        public List<string> Options = new List<string>(new string[] { "Name", "ApplicationID", "State", "Details", 
            "StartTime", "EndTime", "PartySize", "PartyMax", 
            "ImageLargeKey", "ImageLargeText", "ImageSmallKey", "ImageSmallText", "IsAvailableForChange",
            "ImageLarge", "ImageSmall"});
        public string Name
        {
            get => Static.CurrentActivity.Name;
            set
            {
                Static.CurrentActivity.Name = value;
                OnPropertyChanged("Name");
            }
        }
        public string ApplicationID
        {
            get => Static.CurrentActivity.ApplicationID;
            set
            {
                Static.CurrentActivity.ApplicationID = value;
                OnPropertyChanged("ApplicationId");
                OnPropertyChanged("ImageLarge");
                OnPropertyChanged("ImageSmall");
            }
        }
        public string State
        {
            get => Static.CurrentActivity.State;
            set
            {
                Static.CurrentActivity.State = value;
                OnPropertyChanged("State");
            }
        }
        public string Details
        {
            get => Static.CurrentActivity.Details;
            set
            {
                Static.CurrentActivity.Details = value;
                OnPropertyChanged("Details");
            }
        }
        public string StartTime
        {
            get => Static.CurrentActivity.StartTime;
            set
            {
                Static.CurrentActivity.StartTime = value;
                OnPropertyChanged("StartTime");
            }
        }
        public string EndTime
        {
            get => Static.CurrentActivity.EndTime;
            set
            {
                Static.CurrentActivity.EndTime = value;
                OnPropertyChanged("EndTime");
            }
        }
        public string PartySize
        {
            get => Static.CurrentActivity.PartySize;
            set
            {
                Static.CurrentActivity.PartySize = value;
                OnPropertyChanged("PartySize");
            }
        }
        public string PartyMax
        {
            get => Static.CurrentActivity.PartyMax;
            set
            {
                Static.CurrentActivity.PartyMax = value;
                OnPropertyChanged("PartyMax");
            }
        }
        public string ImageLargeKey
        {
            get => Static.CurrentActivity.ImageLargeKey;
            set
            {
                Static.CurrentActivity.ImageLargeKey = value;
                OnPropertyChanged("ImageLargeKey");
                OnPropertyChanged("ImageLarge");
            }
        }
        public string ImageLargeText
        {
            get => Static.CurrentActivity.ImageLargeText;
            set
            {
                Static.CurrentActivity.ImageLargeText = value;
                OnPropertyChanged("ImageLargeText");
            }
        }
        public string ImageSmallKey
        {
            get => Static.CurrentActivity.ImageSmallKey;
            set
            {
                Static.CurrentActivity.ImageSmallKey = value;
                OnPropertyChanged("ImageSmallKey");
                OnPropertyChanged("ImageSmall");
            }
        }
        public string ImageSmallText
        {
            get => Static.CurrentActivity.ImageSmallText;
            set
            {
                Static.CurrentActivity.ImageSmallText = value;
                OnPropertyChanged("ImageSmallText");
            }
        }
        public bool IsAvailableForChange
        {
            get => Static.CurrentActivity.IsAvailableForChange;
            set
            {
                Static.CurrentActivity.IsAvailableForChange = value;
                OnPropertyChanged("IsAvailableForChange");
            }
        }

        private string _SSavedApplicationID, _LSavedApplicationID;
        private System.Drawing.Bitmap _ImageLarge = new System.Drawing.Bitmap(1, 1);
        private string _SavedImageLargeKey;
        private async void ImageLarge_Set()
        {
            await Task.Run(() =>
            {
                _SavedImageLargeKey = Static.ReplaceFilds(ImageLargeKey);
                _LSavedApplicationID = Static.ReplaceFilds(ApplicationID);
                _ImageLarge = Libs.DiscordApi.Discord.AppImages.GetImageById(Libs.DiscordApi.Discord.AppImages.GetImageIdByName(_SavedImageLargeKey, _LSavedApplicationID), _LSavedApplicationID);
                OnPropertyChanged("ImageLarge");
            });
        }
        public ImageSource ImageLarge
        {
            get
            {
                if (_SavedImageLargeKey != Static.ReplaceFilds(ImageLargeKey) || _LSavedApplicationID != Static.ReplaceFilds(ApplicationID))
                    ImageLarge_Set();
                return BitmapEx.ToImageSource(_ImageLarge);
            }
        }
        private System.Drawing.Bitmap _ImageSmall = new System.Drawing.Bitmap(1, 1);
        private string _SavedImageSmallKey;
        private async void ImageSmall_Set()
        {
            await Task.Run(() =>
            {
                _SavedImageSmallKey = Static.ReplaceFilds(ImageSmallKey);
                _SSavedApplicationID = Static.ReplaceFilds(ApplicationID);
                _ImageSmall = Libs.DiscordApi.Discord.AppImages.GetImageById(Libs.DiscordApi.Discord.AppImages.GetImageIdByName(_SavedImageSmallKey, _SSavedApplicationID), _SSavedApplicationID);
                OnPropertyChanged("ImageSmall");
            });
        }
        public ImageSource ImageSmall
        {
            get
            {
                if (_SavedImageSmallKey != Static.ReplaceFilds(ImageSmallKey) || _SSavedApplicationID != Static.ReplaceFilds(ApplicationID))
                    ImageSmall_Set();
                return BitmapEx.ToImageSource(_ImageSmall);
            }
        }
        #endregion

        private GameStatus _GameStatusView;
        public GameStatus GameStatusView
        {
            get => _GameStatusView;
            set
            {
                _GameStatusView = value;

                GameStatusView.SaveChangesBox.ApplyCommand = new Command(() =>
                {
                    Static.UpdateDiscordActivity();
                    Preferences.SaveProfiles();
                    GameStatusView.IsChanged = false;
                });
                GameStatusView.SaveChangesBox.CancelCommand = new Command(() =>
                {
                    var s = Static.CurrentActivity.SavedState;
                    Static.CurrentActivity = (Activity)Static.CurrentActivity.SavedState;
                    Static.CurrentActivity.SavedState = s;
                    OnActivityChanged();
                    GameStatusView.IsChanged = false;
                });
            }
        }

        public GameStatusViewModel()
        {
            Static.Discord.Socket.OnUserInfoChanged += Socket_OnUserInfoChanged;
            Static.OnActivityChanged += OnActivityChanged;
            Static.OnActivitiesChanged += () =>
            {
                OnPropertyChanged("Profiles");
                OnPropertyChanged("SelectedProfileIndex");
            };
        }

        public void OnActivityChanged()
        {
            OnPropertyChanged("SelectedProfileIndex");
            OnPropertyChanged("SelectedProfile");
            Options.ForEach(o => OnPropertyChanged(o));
        }

        private void Socket_OnUserInfoChanged(string eventtype, object data, UserInfo userInfo)
        {
            DiscordUserName = userInfo.UserName;
            DiscordUserTag = userInfo.Discriminator;
            if (userInfo.AvatarId != null)
                GameStatusView.Dispatcher.Invoke(() => 
                    DiscordUserAvatar = BitmapEx.ToImageSource(Libs.DiscordApi.Discord.GetUserAvatar(userInfo.Id, userInfo.AvatarId, 128)));

            if (eventtype == "READY")
                Static.UpdateDiscordActivity();
        }
    }
}