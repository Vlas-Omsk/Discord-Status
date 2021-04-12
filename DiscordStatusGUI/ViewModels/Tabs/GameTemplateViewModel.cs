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
using System.Collections.ObjectModel;
using System.Reflection;
using DiscordStatusGUI.Libs;
using DiscordStatusGUI.Models;
using System.Threading;

namespace DiscordStatusGUI.ViewModels.Tabs
{
    abstract class GameTemplateViewModel : TemplateViewModel
    {
        protected PropertiesModel _Properties = new PropertiesModel();
        public ObservableCollection<PropertyModel> Properties
        {
            get => new ObservableCollection<PropertyModel>(_Properties);
            set
            {
                _Properties = new PropertiesModel(value);
                OnPropertyChanged("Properties");
            }
        }

        public Activity[] Profiles
        {
            get
            {
                var t = new List<Activity>(Static.Activities);
                t.Add(new Activity() { ProfileName = Locales.Lang.GetResource("NotAssigned") });
                return t.ToArray();
            }
        }

        protected int DefaultProfileIndex = -1;
        public int SelectedProfileIndex
        {
            get => DefaultProfileIndex;
            set
            {
                DefaultProfileIndex = value;
                OnPropertyChanged("SelectedProfileIndex");
                OnGameProcessStateChanged(_SavedState);
                Preferences.Save();
            }
        }

        private int _SavedLastProfileIndex = 0;
        private bool _SavedState = false;

        protected void OnGameProcessStateChanged(bool opened)
        {
            Static.OnGameStatusChanged(opened, SelectedProfileIndex, ref _SavedLastProfileIndex, ref _SavedState);
        }

        protected abstract void UpdateDiscordActivityIf();
    }
}
