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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DiscordStatusGUI.Models;
using DiscordStatusGUI.ViewModels.Dialogs;
using System.Reflection;

namespace DiscordStatusGUI.Views.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для Notifications.xaml
    /// </summary>
    public partial class Notifications : UserControl
    {
        public Notifications()
        {
            InitializeComponent();
        }

        public void AddNotification(Notification item)
        {
            notifications.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid.SetRow(item, notifications.RowDefinitions.Count - 1);
            notifications.Children.Add(item);
        }
    }
}