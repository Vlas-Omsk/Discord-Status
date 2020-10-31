using DiscordStatusGUI.ViewModels.Dialogs;
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

namespace DiscordStatusGUI.Views.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для TopPanel.xaml
    /// </summary>
    public partial class SaveChangesBox : UserControl
    {
        public SaveChangesBox()
        {
            InitializeComponent();
        }

        public Command CancelCommand { get => (DataContext as SaveChangesBoxViewModel).CancelCommand; set => (DataContext as SaveChangesBoxViewModel).CancelCommand = value; }
        public Command ApplyCommand { get => (DataContext as SaveChangesBoxViewModel).ApplyCommand; set => (DataContext as SaveChangesBoxViewModel).ApplyCommand = value; }

        public void Hide()
        {
            var margin = new ThicknessAnimation(new Thickness(10, 10, 10, 10), new Thickness(10, 10, 10, -20), TimeSpan.FromMilliseconds(100));
            margin.Completed += (s, e) => Visibility = Visibility.Hidden;
            BeginAnimation(MarginProperty, margin);
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
            var margin = new ThicknessAnimation(new Thickness(10, 10, 10, -20), new Thickness(10, 10, 10, 10), TimeSpan.FromMilliseconds(100));
            BeginAnimation(MarginProperty, margin);
        }
    }
}
