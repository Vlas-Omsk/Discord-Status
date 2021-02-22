using DiscordStatusGUI.Models;
using DiscordStatusGUI.Views.Dialogs;
using DiscordStatusGUI.Views.Discord;
using DiscordStatusGUI.Views.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.ViewModels
{
    class VerticalTabControlViewModel : TemplateViewModel
    {
        public ObservableCollection<VerticalTabItem> Tabs
        {
            get => Static.Tabs;
            set 
            {
                Static.Tabs = value;
                OnPropertyChanged("Tabs");
            }
        }
        private VerticalTabItem _SelectedTab;
        public VerticalTabItem SelectedTab
        {
            get => _SelectedTab;
            set
            {
                if (_SelectedTab?.Page != null)
                {
                    var anim = Animations.VisibleOffSlideDown(_SelectedTab.Page);
                    anim.Completed += (s, e) => On();
                    anim.Begin();
                    //Static.MainWindow.notifications.AddNotification(new Notification("SelectedTab.set", " To=" + value?.Text, true, 5000));
                }
                else
                    On();

                void On()
                {
                    _SelectedTab = value;
                    OnPropertyChanged("SelectedTab");
                    if (_SelectedTab?.Page != null)
                        Animations.VisibleOnSlideDown(_SelectedTab.Page).Begin();
                }
            }
        }

        public VerticalTabControlViewModel()
        {
            
        }
    }
}
