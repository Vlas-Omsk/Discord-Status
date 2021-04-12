using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.ViewModels.Discord;
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

namespace DiscordStatusGUI.Views.Discord
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private int CatClickCount = 0;
        public static ImageSource[] PixelCat_Walking_frames = new ImageSource[]
            {
                BitmapEx.ToImageSource(Properties.Resources._1),
                BitmapEx.ToImageSource(Properties.Resources._2),
                BitmapEx.ToImageSource(Properties.Resources._3),
                BitmapEx.ToImageSource(Properties.Resources._4)
            };

        public Login()
        {
            InitializeComponent();
            
            (DataContext as LoginViewModel).LoginView = this;
            (DataContext as LoginViewModel).PropertyChanged += Login_PropertyChanged;
        }

        #region Password binding
        private void Login_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Password" && PasswordField.Password != (DataContext as LoginViewModel).Password)
                PasswordField.Password = (DataContext as LoginViewModel).Password;
        }

        private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            (DataContext as LoginViewModel).Password = PasswordField.Password;
            if ((sender as PasswordBox).Password.Length > 4)
            {
                (DataContext as LoginViewModel).PasswordError = "";
                RestorePassword();
            }
        }
        #endregion

        private void CatClick(object sender, MouseButtonEventArgs e)
        {
            if (CatClickCount <= 7)
            {
                Animations.Shake(2, 15, new TimeSpan(00, 00, 00, 00, 500), CatText).Begin();
                if (CatClickCount == 5)
                    CatText.Text = Locales.Lang.GetResource("Views:Discord:Login:CatEmotions:Angry");
            }
            else
            {
                CatImage.MouseUp -= CatClick;

                var opacity = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(100)));
                opacity.Completed += (s, a) => CatText.Visibility = Visibility.Hidden;
                CatText.BeginAnimation(Window.OpacityProperty, opacity);
                Animations.ScaleTransform(1, 0.7, 1, 0.7, CatText).Begin();

                var t = new Thread(() =>
                {
                    var i = 0;
                    while (true)
                    {
                        Thread.Sleep(200);

                        Dispatcher.Invoke(() =>
                        {
                            CatImage.Source = PixelCat_Walking_frames[i];
                        });

                        i++;
                        if (i >= 4)
                            i = 0;
                    }
                })
                {
                    IsBackground = true,
                    ApartmentState = ApartmentState.STA
                };
                t.Start();

                ThicknessAnimation go_out = new ThicknessAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(ActualWidth * 50),
                    To = new Thickness(-(ActualWidth * 2), 0, 0, 0),
                };
                go_out.Completed += (ss, ee) =>
                {
                    t.Abort();
                    CatImage.Visibility = Visibility.Hidden;
                };
                CatImage.BeginAnimation(Image.MarginProperty, go_out);
            }
            CatClickCount++;
        }

        private void EmailField_TextChanged(object sender, TextChangedEventArgs e)
        {
            (DataContext as LoginViewModel)._Email = (sender as TextBox).Text;
            if ((sender as TextBox).Text.Length > 3 && (sender as TextBox).Text.IndexOf('@') != -1)
            {
                (DataContext as LoginViewModel).EmailError = "";
                RestoreEmail();
            }
        }

        private void PasswordField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length > 4)
            {
                (DataContext as LoginViewModel).PasswordError = "";
                RestorePassword();
            }
        }


        public void WrongOrEmptyPassword()
        {
            Dispatcher.Invoke(() =>
            {
                Animations.BrushColorTo(Animations.DiscordLPWrongOrEmpty, Label.ForegroundProperty, PasswordLabel).Begin();
                Animations.BrushColorTo(Animations.DiscordLPWrongOrEmpty, TextBox.BorderBrushProperty, PasswordField).Begin();
            });
        }

        public void WrongOrEmptyEmail()
        {
            Dispatcher.Invoke(() =>
            {
                Animations.BrushColorTo(Animations.DiscordLPWrongOrEmpty, Label.ForegroundProperty, EmailLabel).Begin();
                Animations.BrushColorTo(Animations.DiscordLPWrongOrEmpty, TextBox.BorderBrushProperty, EmailField).Begin();
            });
        }

        public void RestorePassword()
        {
            Dispatcher.Invoke(() =>
            {
                Animations.BrushColorTo(Animations.DiscordLPLabelDefault, Label.ForegroundProperty, PasswordLabel).Begin();
                Animations.BrushColorTo(Animations.DiscordLPTextBoxDefault, TextBox.BorderBrushProperty, PasswordField).Begin();
            });
        }

        public void RestoreEmail()
        {
            Dispatcher.Invoke(() =>
            {
                Animations.BrushColorTo(Animations.DiscordLPLabelDefault, Label.ForegroundProperty, EmailLabel).Begin();
                Animations.BrushColorTo(Animations.DiscordLPTextBoxDefault, TextBox.BorderBrushProperty, EmailField).Begin();
            });
        }        
    }
}
