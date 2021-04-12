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

namespace DiscordStatusGUI.Views.Popups
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class SteamLogin : UserControl, IPopupContent
    {
        public SteamLogin()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Static.DelayedRun("SteamLoginWebBrowserShowing", () => Dispatcher.Invoke(() => webbrowser.Visibility = Visibility.Visible), 200);
        }

        void IPopupContent.OnClose()
        {
            webbrowser.Visibility = Visibility.Hidden;
        }
    }
}
