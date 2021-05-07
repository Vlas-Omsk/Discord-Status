using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DiscordStatusGUI.Extensions;

namespace DiscordStatusGUI.Views.Tabs
{
    /// <summary>
    /// Логика взаимодействия для Embeds.xaml
    /// </summary>
    public partial class Embeds : UserControl
    {
        public Embeds()
        {
            InitializeComponent();
        }

        ListViewItem ListViewItem;

        private void item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem = (ListViewItem)((Grid)sender).TemplatedParent;
            ListViewItem.Tag = "True";
        }

        private void item_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ListViewItem != null)
            {
                ListViewItem.Tag = "False";
            }
        }

        Color FromHex(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.PreviewMouseLeftButtonUp += item_PreviewMouseLeftButtonUp;
        }
    }
}
