using DiscordStatusGUI.ViewModels;
using DiscordStatusGUI.ViewModels.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace DiscordStatusGUI.Views.Tabs
{
    /// <summary>
    /// Логика взаимодействия для TopPanel.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();

            (DataContext as SettingsViewModel).SettingsView = this;
        }

        public void HideUserInfoPlug()
        {
            Dispatcher.Invoke(() =>
            {
                Storyboard storyboard = new Storyboard();

                #region bluranimation
                var elem = new DiscordStatusGUI.Libs.PathAnimate.Element("0");

                DoubleAnimation blur = new DoubleAnimation(UserInfoPlugBlur.Radius, 0, TimeSpan.FromMilliseconds(100));
                storyboard.Children.Add(blur);

                Storyboard.SetTargetProperty(blur, new PropertyPath(DiscordStatusGUI.Libs.PathAnimate.Element.ValueProperty));
                Storyboard.SetTarget(blur, elem);
                elem.i = 1;
                elem.ValueChanged += (i, d, n) =>
                    UserInfoPlugBlur.Radius = d;
                #endregion

                DoubleAnimation opacity = new DoubleAnimation(0, TimeSpan.FromMilliseconds(100));
                storyboard.Children.Add(opacity);

                Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
                Storyboard.SetTarget(opacity, UserInfoPlugMsg);

                storyboard.Begin();
            });
        }

        public void ShowUserInfoPlug()
        {
            Dispatcher.Invoke(() =>
            {
                Storyboard storyboard = new Storyboard();

                #region bluranimation
                var elem = new DiscordStatusGUI.Libs.PathAnimate.Element("0");

                DoubleAnimation blur = new DoubleAnimation(UserInfoPlugBlur.Radius, 10, TimeSpan.FromMilliseconds(100));
                storyboard.Children.Add(blur);

                Storyboard.SetTargetProperty(blur, new PropertyPath(DiscordStatusGUI.Libs.PathAnimate.Element.ValueProperty));
                Storyboard.SetTarget(blur, elem);
                elem.i = 1;
                elem.ValueChanged += (i, d, n) =>
                    UserInfoPlugBlur.Radius = d;
                #endregion

                DoubleAnimation opacity = new DoubleAnimation(1, TimeSpan.FromMilliseconds(100));
                storyboard.Children.Add(opacity);

                Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
                Storyboard.SetTarget(opacity, UserInfoPlugMsg);

                storyboard.Begin();
            });
        }
    }
}
