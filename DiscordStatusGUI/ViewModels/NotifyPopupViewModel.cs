using DiscordStatusGUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.ViewModels
{
    class NotifyPopupViewModel : TemplateViewModel
    {
        public ObservableCollection<NotifyItem> Items
        {
            get
            {
                return new ObservableCollection<NotifyItem>()
                {
                    new NotifyItem(null, "/DiscordStatusGUI;component/Resources/logo/logo_small.png", 0.7, Static.Title) { IsEnabled = false, WithSplitter = true },
#if DEBUG
                    new NotifyItem(new Command(() => Static.MainWindow.NotifyPopup.IsOpen = false), "", 0, "Hide this menu...") { WithSplitter = true },
#endif
                    new NotifyItem(new Command(() => Static.MainWindow.Close()), "", 0, "Quit " + Static.Title)
                };
            }
        }

        public NotifyPopupViewModel()
        {
        }
    }
}
