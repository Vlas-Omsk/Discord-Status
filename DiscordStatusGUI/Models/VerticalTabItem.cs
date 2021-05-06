using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiscordStatusGUI.Models
{
    class VerticalTabItem
    {
        public string ImagePath { get; set; }
        public string Text { get; set; }
        public UserControl Page { get; set; }
        public double Zoom { get; set; }

        public T GetDataContext<T>()
        {
            var dc = Page.Dispatcher.Invoke(() => Page.DataContext);
            if (dc is null)
                return default;
            else
                return (T)dc;
        }

        public VerticalTabItem(string imagepath = "", double zoom = 0.7, string text = "", UserControl page = null)
        {
            ImagePath = imagepath;
            Text = text;
            Page = page;
            if (Page != null)
                Page.Background = new SolidColorBrush(Colors.Transparent);
            Zoom = zoom;
        }
    }
}
