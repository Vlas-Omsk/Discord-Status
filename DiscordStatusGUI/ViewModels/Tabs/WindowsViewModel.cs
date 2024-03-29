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
using PinkJson;
using System.Windows;
using DiscordStatusGUI.Views.Discord;
using System.Collections.ObjectModel;
using System.Reflection;
using DiscordStatusGUI.Libs;
using DiscordStatusGUI.Models;

namespace DiscordStatusGUI.ViewModels.Tabs
{
    class WindowsViewModel : GameTemplateViewModel
    {
        public WindowsViewModel()
        {
            _Properties.Add("{win:ForegroundWindowName}",        Locales.Lang.GetResource("ViewModels:Tabs:WindowsViewModel:ForegroundWindowName"), Static.GetValueByFieldName("win:ForegroundWindowName"));
            _Properties.Add("{win:ForegroundWindowProcessName}", Locales.Lang.GetResource("ViewModels:Tabs:WindowsViewModel:ForegroundWindowProcessName"), Static.GetValueByFieldName("win:ForegroundWindowProcessName"));

            OnPropertyChanged("Properties");

            ProcessEx.OnForegroundWindowChanged += (p) => UpdateDiscordActivityIf();
        }

        protected override void UpdateDiscordActivityIf()
        {
            _Properties[0].Value = Static.GetValueByFieldName("win:ForegroundWindowName");
            _Properties[1].Value = Static.GetValueByFieldName("win:ForegroundWindowProcessName");

            OnPropertyChanged("Properties");

            if (Static.IsPrefixContainsInFields(Static.CurrentActivity, "win"))
                Static.UpdateDiscordActivity();
        }
    }
}
