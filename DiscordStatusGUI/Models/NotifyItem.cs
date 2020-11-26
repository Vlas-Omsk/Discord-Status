using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiscordStatusGUI.Models
{
    class NotifyItem
    {
        public string ImagePath { get; set; }
        public string Text { get; set; }
        public Command Command { get; set; }
        public double Zoom { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool WithSplitter { get; set; } = false;

        public NotifyItem(Command command, string imagepath = "", double zoom = 0.7, string text = "")
        {
            ImagePath = imagepath;
            Text = text;
            Zoom = zoom;
            Command = command;
        }
    }
}
