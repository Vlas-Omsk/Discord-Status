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
            get
            {
                if (string.IsNullOrEmpty(SearchText))
                    return _PrivateChannels;

                IEnumerable<PrivateChannel> result;
                if (SearchText[0] == '#')
                {
                    result = _PrivateChannels.Where(pc =>
                    {
                        if (pc.IsGroup || pc.RecipientIDs == null || pc.UsersCache == null)
                            return false;
                        var us = pc.UsersCache.GetUser(pc.RecipientIDs[0]).UserStatus;
                        var str = SearchText.Substring(1);
                        if (SearchText.Length == 1)
                            return us != UserStatus.offline;
                        else
                            return us.ToString().IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1;
                    });
                } else
                {
                    result = _PrivateChannels.Where(pc => pc.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1);
                }
                return new ObservableCollection<PrivateChannel>(result);
            }
            set
            {
                _PrivateChannels = value;
                OnPropertyChanged("PrivateChannels");
            }
        }

        public string _SearchText = "";
        public string SearchText
        {
            get => _SearchText;
            set
            {
                var val = _SearchText;
                _SearchText = value;
                OnPropertyChanged("SearchText");

                if (value != val)
                {
                    OnPropertyChanged("PrivateChannels");
                }
            }
        }

        public EmbedsViewModel()
        {
            Static.Discord.Socket.PrivateChannelsChanged += Socket_PrivateChannelsChanged;
            Static.Discord.Socket.UsersCacheChanged += Socket_UsersCacheChanged; ;
        }

        private void Socket_UsersCacheChanged(object sender, DiscordEventArgs<UsersCache> e)
        {
            if (e.EventType == "PRESENCE_UPDATE")
                PrivateChannels = new ObservableCollection<PrivateChannel>(Static.Discord.Socket.PrivateChannels.Channels);
        }

        private void Socket_PrivateChannelsChanged(object sender, DiscordEventArgs<PrivateChannels> e)
        {
            PrivateChannels = new ObservableCollection<PrivateChannel>(e.Data.Channels);
        }
    }
}
