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
    class WarfaceViewModel : GameTemplateViewModel
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

        public WarfaceViewModel()
        {
            DefaultProfileIndex = 6;

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

            WarfaceApi.CurrentGameState.PropertyChanged += (s, e) => UpdateDiscordActivityIf();
            WarfaceApi.CurrentPlayer.PlayerInfoChanged += (s, e) => UpdateDiscordActivityIf();
            WarfaceApi.CurrentPlayer.OnUserInfoChanged += (p) => UpdateDiscordActivityIf();

            WarfaceApi.OnGameProcessStateChanged += OnGameProcessStateChanged;
        }

        protected override void UpdateDiscordActivityIf()
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
    }
}
