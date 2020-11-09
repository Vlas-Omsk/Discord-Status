using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiscordStatusGUI.Models
{
    class ButtonItem
    {
        public string Text { get; set; }
        public Style Style { get; set; }
        public Command ClickCommand { get; set; }

        public ButtonItem(Action click, string text = "")
        {
            ClickCommand = new Command(click);
            Style = Static.DiscordTheme["BlueButton"] as Style;
            Text = text;
        }
    }
}
