using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DiscordStatusGUI.Models;
using DiscordStatusGUI.Libs.DiscordApi;

namespace DiscordStatusGUI.ViewModels.Tabs
{
    class EmbedsViewModel : TemplateViewModel
    {
        private ObservableCollection<PrivateChannel> _PrivateChannels = new ObservableCollection<PrivateChannel>()
        {
            new PrivateChannel() { Name = "Username First Username First" },
            new PrivateChannel() { Name = "Username Second Username Second" },
            new PrivateChannel() { Name = "Username Thirth Username Thirth" }
        };
        public ObservableCollection<PrivateChannel> PrivateChannels
        {
            get => _PrivateChannels;
            set
            {
                _PrivateChannels = value;
                OnPropertyChanged("PrivateChannels");
            }
        }

        public EmbedsViewModel()
        {
            Static.Discord.Socket.PrivateChannelsChanged += Socket_PrivateChannelsChanged;
        }

        private void Socket_PrivateChannelsChanged(object sender, DiscordEventArgs<PrivateChannels> e)
        {
            PrivateChannels = new ObservableCollection<PrivateChannel>(e.Data.Channels);
        }
    }
}
