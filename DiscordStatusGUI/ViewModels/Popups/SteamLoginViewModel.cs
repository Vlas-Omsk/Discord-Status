using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordStatusGUI.Libs;

namespace DiscordStatusGUI.ViewModels.Popups
{
    class SteamLoginViewModel : TemplateViewModel
    {
        public string SteamLoginSecure
        {
            get => SteamApi.CurrentSteamProfile.SteamLoginSecure;
            set
            {
                SteamApi.CurrentSteamProfile.SteamLoginSecure = value;
                OnPropertyChanged("SteamLoginSecure");
            }
        }

        private bool _ResearchButtonEnabled = true;
        public bool ResearchButtonEnabled
        {
            get => _ResearchButtonEnabled;
            set
            {
                _ResearchButtonEnabled = value;
                OnPropertyChanged("ResearchButtonEnabled");
            }
        }

        public Command ResearchSteamLoginSecureCommand { get; private set; }

        public SteamLoginViewModel()
        {
            ResearchSteamLoginSecureCommand = new Command(ResearchSteamLoginSecure);

            SteamApi.CurrentSteamProfile.OnPropertyChanged += (n) =>
            {
                if (n == "SteamLoginSecure")
                    OnPropertyChanged("SteamLoginSecure");
            };
        }

        private async void ResearchSteamLoginSecure()
        {
            await Task.Run(() =>
            {
                ResearchButtonEnabled = false;
                SteamApi.CurrentSteamProfile.ResearchSteamLoginSecure();
                ResearchButtonEnabled = true;
            });
        }
    }
}
