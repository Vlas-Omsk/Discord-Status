using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DiscordStatusGUI.Extensions;

namespace DiscordStatusGUI.ViewModels
{
    class TemplateViewModel : INotifyPropertyChanged
    {
        public Command OpenLinkCommand { get; private set; }

        public TemplateViewModel()
        {
            OpenLinkCommand = new Command(e => OpenLink(e+""));
        }

        public static void OpenLink(string link)
        {
            ProcessEx.GetOutput("cmd", "/c start " + link);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
