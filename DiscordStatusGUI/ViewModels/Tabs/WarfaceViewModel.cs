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
using System.Collections.ObjectModel;
using System.Reflection;
using DiscordStatusGUI.Libs;
using DiscordStatusGUI.Models;
using System.Threading;

namespace DiscordStatusGUI.ViewModels.Tabs
{
    class WarfaceViewModel : TemplateViewModel
    {
        public bool IsFastGameClientClose
        {
            get => WarfaceApi.FastGameClientClose;
            set
            {
                WarfaceApi.FastGameClientClose = value;
                OnPropertyChanged("IsFastGameClientClose");
                Preferences.Save();
            }
        }

        private PropertiesModel _Properties = new PropertiesModel();
        public ObservableCollection<PropertyModel> Properties
        {
            get => new ObservableCollection<PropertyModel>(_Properties);
            set
            {
                _Properties = new PropertiesModel(value);
                OnPropertyChanged("Properties");
            }
        }

        private PropertyModel _SelectedProperty;
        public PropertyModel SelectedProperty
        {
            get => _SelectedProperty;
            set
            {
                _SelectedProperty = value;
                OnPropertyChanged("SelectedProperty");
            }
        }

        public Activity[] Profiles
        {
            get
            {
                var t = new List<Activity>(Static.Activities);
                t.Add(new Activity() { ProfileName = "Not assigned" });
                return t.ToArray();
            }
        }

        private int _SelectedProfileIndex = 6;
        public int SelectedProfileIndex
        {
            get => _SelectedProfileIndex;
            set
            {
                _SelectedProfileIndex = value;
                OnPropertyChanged("SelectedProfileIndex");
                WarfaceApi_OnGameProcessStateChanged(_SavedState);
                Preferences.Save();
            }
        }

        private int _SavedLastProfileIndex = -1;
        private bool _SavedState = false;


        public WarfaceViewModel()
        {

            _Properties.Add("{wf:AppName}", "Название приложения", Static.GetValueByFieldName("wf:AppName"));
            _Properties.Add("{wf:AppID}", "ID приложения", Static.GetValueByFieldName("wf:AppID"));
            _Properties.Add("{wf:Map}", "Текущая карта", Static.GetValueByFieldName("wf:Map"));
            _Properties.Add("{wf:State}", "Текущее состояние игры", Static.GetValueByFieldName("wf:State"));
            _Properties.Add("{wf:StateStartTime}", "Время в мс с которого вы находитесь в этом состоянии", Static.GetValueByFieldName("wf:StateStartTime"));
            _Properties.Add("{wf:InGameServerName}", "Имя внутриигрового сервера, например: Ветераны 15", Static.GetValueByFieldName("wf:InGameServerName"));
            _Properties.Add("{wf:ServerIP}", "Адрес сервера", Static.GetValueByFieldName("wf:ServerIP"));
            _Properties.Add("{wf:ServerName}", "Имя сервера", Static.GetValueByFieldName("wf:ServerName"));
            _Properties.Add("{wf:ServerRegion}", "Ваш регион", Static.GetValueByFieldName("wf:ServerRegion"));
            _Properties.Add("{wf:PlayerNickname}", "Ваше игровое имя на текущем сервере", Static.GetValueByFieldName("wf:PlayerNickname"));
            _Properties.Add("{wf:PlayerRank}", "Ваш ранк на текущем сервере", Static.GetValueByFieldName("wf:PlayerRank"));
            _Properties.Add("{wf:PlayerRankName}", "Ваш ранк на текущем сервере", Static.GetValueByFieldName("wf:PlayerRankName"));
            _Properties.Add("{wf:PlayerUserID}", "Ваш уникальный ID персонажа", Static.GetValueByFieldName("wf:PlayerUserID"));

            OnPropertyChanged("Properties");

            WarfaceApi.CurrentGameState.OnPropertyChanged += (p) => UpdateDiscordActivityIf();
            WarfaceApi.CurrentPlayer.OnPlayerInfoChanged += () => UpdateDiscordActivityIf();
            WarfaceApi.CurrentPlayer.OnUserInfoChanged += (p) => UpdateDiscordActivityIf();


            WarfaceApi.OnGameProcessStateChanged += WarfaceApi_OnGameProcessStateChanged;
        }

        private void UpdateDiscordActivityIf()
        {
            _Properties[2].Value = Static.GetValueByFieldName("wf:Map");
            _Properties[3].Value = Static.GetValueByFieldName("wf:State");
            _Properties[4].Value = Static.GetValueByFieldName("wf:StateStartTime");
            _Properties[5].Value = Static.GetValueByFieldName("wf:InGameServerName");
            _Properties[6].Value = Static.GetValueByFieldName("wf:ServerIP");
            _Properties[7].Value = Static.GetValueByFieldName("wf:ServerName");
            _Properties[8].Value = Static.GetValueByFieldName("wf:ServerRegion");
            _Properties[9].Value = Static.GetValueByFieldName("wf:PlayerNickname");
            _Properties[10].Value = Static.GetValueByFieldName("wf:PlayerRank");
            _Properties[11].Value = Static.GetValueByFieldName("wf:PlayerRankName");
            _Properties[12].Value = Static.GetValueByFieldName("wf:PlayerUserID");

            OnPropertyChanged("Properties");

            if (Static.IsPrefixContainsInFields(Static.CurrentActivity, "wf"))
                Static.UpdateDiscordActivity();
        }

        private void WarfaceApi_OnGameProcessStateChanged(bool opened)
        {
            Static.OnGameStatusChanged(opened, SelectedProfileIndex, ref _SavedLastProfileIndex, ref _SavedState);
        }
    }
}
