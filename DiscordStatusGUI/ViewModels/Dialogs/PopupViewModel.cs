using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.ViewModels.Dialogs
{
    class PopupViewModel : TemplateViewModel
    {
        public object __Content;
        public object Content {
            get => __Content;
            set
            {
                __Content = value;
                OnPropertyChanged("Content");
            }
        }
        public Command BackCommand { get; set; }

        public PopupViewModel()
        {
        }
    }
}
