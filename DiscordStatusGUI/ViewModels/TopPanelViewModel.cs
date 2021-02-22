using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.ViewModels
{
    class TopPanelViewModel : TemplateViewModel
    {
        public string Title { get => Static.Title; }

        public Command MinimizeCommand { get; private set; }
        public Command MaximizeCommand { get; private set; }
        public Command CloseCommand { get; private set; }

        public TopPanelViewModel()
        {
            MinimizeCommand = new Command(Static.Window.Minimize);
            MaximizeCommand = new Command(Static.Window.Maximize);
            CloseCommand = new Command(Static.Window.Close);
        }
    }
}
