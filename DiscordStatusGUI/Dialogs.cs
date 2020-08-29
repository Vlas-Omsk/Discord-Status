using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace DiscordStatusGUI
{
    public class Dialogs
    {
        public static void Init(MainWindow Window)
        {
            window = Window;
            MessageBoxButtonOkClickAction = () => HideMessageBoxOk();
            SaveChangesBoxButtonSaveClickAction = () =>
            {
                window.SaveChangesBoxButtonSave.IsEnabled = false;
                new System.Threading.Thread(() =>
                {
                    window.AutoUpdateDiscord2(null, null);
                    window.Dispatcher.Invoke(() =>
                    {
                        window.SaveVisualizerPreferences();
                        window.PreferencesSave();
                        HideSaveChangesBox();
                        window.SaveChangesBoxButtonSave.IsEnabled = true;
                    });
                }).Start();
            };
            SaveChangesBoxButtonResetClickAction = () => 
            {
                window.LoadVisualizerPreferences();
                HideSaveChangesBox();
            };
        }

        public static MainWindow window;

        #region MessageBox
        public static bool IsMessageBoxOkOpened = false;
        public static Action MessageBoxButtonOkClickAction;
        public static void ShowMessageBoxOk(string Caption, string Content)
        {
            window.MessageBoxCaption.Content = Caption;
            window.MessageBoxContent.Text = Content;

            window.MessageBox.Visibility = Visibility.Visible;
            var opacity = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));
            window.MessageBox.BeginAnimation(Window.OpacityProperty, opacity);
            Animations.CreateScaleTransform(0.7, 1, 0.7, 1, window.MessageBoxBody).Begin();

            IsMessageBoxOkOpened = true;

            window.MessageBoxButtonOk.Click += MessageBoxButtonOk_Click;
        }

        private static void MessageBoxButtonOk_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxButtonOkClickAction();
        }

        public static void HideMessageBoxOk()
        {
            var opacity = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(100)));
            opacity.Completed += (s, a) => window.MessageBox.Visibility = Visibility.Hidden;
            window.MessageBox.BeginAnimation(Window.OpacityProperty, opacity);
            Animations.CreateScaleTransform(1, 0.7, 1, 0.7, window.MessageBoxBody).Begin();

            IsMessageBoxOkOpened = false;

            window.MessageBoxButtonOk.Click -= MessageBoxButtonOk_Click;
        }
        #endregion MessageBox

        #region SaveChangesBox
        public static bool IsSaveChangesBoxOpened = false;
        public static bool DontShowSaveChangesBox = false;
        public static Action SaveChangesBoxButtonSaveClickAction;
        public static Action SaveChangesBoxButtonResetClickAction;
        public static void ShowSaveChangesBox()
        {
            if (DontShowSaveChangesBox)
                return;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation translateY = new DoubleAnimation();
            translateY.Duration = TimeSpan.FromMilliseconds(120);
            translateY.To = 0;
            storyboard.Children.Add(translateY);

            Storyboard.SetTargetProperty(translateY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            Storyboard.SetTarget(translateY, window.SaveChangesBox);

            storyboard.Begin();

            IsSaveChangesBoxOpened = true;

            window.SaveChangesBoxButtonSave.Click += SaveChangesBoxButtonSave_Click;
            window.SaveChangesBoxButtonReset.PreviewMouseLeftButtonUp += SaveChangesBoxButtonReset_Click;
        }

        private static void SaveChangesBoxButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveChangesBoxButtonSaveClickAction();
        }

        private static void SaveChangesBoxButtonReset_Click(object sender, RoutedEventArgs e)
        {
            SaveChangesBoxButtonResetClickAction();
        }

        public static void HideSaveChangesBox()
        {
            Storyboard storyboard = new Storyboard();

            DoubleAnimation translateY = new DoubleAnimation();
            translateY.Duration = TimeSpan.FromMilliseconds(120);
            translateY.To = 72;
            storyboard.Children.Add(translateY);

            Storyboard.SetTargetProperty(translateY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            Storyboard.SetTarget(translateY, window.SaveChangesBox);

            storyboard.Begin();

            IsSaveChangesBoxOpened = false;

            window.SaveChangesBoxButtonSave.Click -= SaveChangesBoxButtonSave_Click;
            window.SaveChangesBoxButtonReset.PreviewMouseLeftButtonUp -= SaveChangesBoxButtonReset_Click;
        }
        #endregion SaveChangesBox
    }
}
