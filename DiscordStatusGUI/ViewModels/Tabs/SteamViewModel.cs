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

namespace DiscordStatusGUI.ViewModels.Tabs
{
    class SteamViewModel : GameTemplateViewModel
    {
        public SteamViewModel()
        {
            DefaultProfileIndex = 7;

            _Properties.Add("{steam:SteamID}",      "SteamID",                                                               Static.GetValueByFieldName("steam:SteamID"));
            _Properties.Add("{steam:Nickname}",     Locales.Lang.GetResource("ViewModels:Tabs:SteamViewModel:Nickname"),     Static.GetValueByFieldName("steam:Nickname"));
            _Properties.Add("{steam:Status}",       Locales.Lang.GetResource("ViewModels:Tabs:SteamViewModel:Status"),       Static.GetValueByFieldName("steam:Status"));
            _Properties.Add("{steam:GameName}",     Locales.Lang.GetResource("ViewModels:Tabs:SteamViewModel:GameName"),     Static.GetValueByFieldName("steam:GameName"));
            _Properties.Add("{steam:GameState}",    Locales.Lang.GetResource("ViewModels:Tabs:SteamViewModel:GameState"),    Static.GetValueByFieldName("steam:GameState"));
            _Properties.Add("{steam:RichPresence}", Locales.Lang.GetResource("ViewModels:Tabs:SteamViewModel:RichPresence"), Static.GetValueByFieldName("steam:RichPresence"));

            OnPropertyChanged("Properties");

            SteamApi.CurrentSteamProfile.OnPropertyChanged += UpdateDiscordActivityIf;

            SteamApi.OnGameProcessStateChanged += OnGameProcessStateChanged;
        }

        protected override void UpdateDiscordActivityIf()
        {
            _Properties[0].Value = Static.GetValueByFieldName("steam:SteamID");
            _Properties[1].Value = Static.GetValueByFieldName("steam:Nickname");
            _Properties[2].Value = Static.GetValueByFieldName("steam:Status");
            _Properties[3].Value = Static.GetValueByFieldName("steam:GameName");
            _Properties[4].Value = Static.GetValueByFieldName("steam:GameState");
            _Properties[5].Value = Static.GetValueByFieldName("steam:RichPresence");

            OnPropertyChanged("Properties");

            if (Static.IsPrefixContainsInFields(Static.CurrentActivity, "steam"))
                Static.UpdateDiscordActivity();
        }
    }
}
