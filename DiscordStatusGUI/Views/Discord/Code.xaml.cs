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
    public partial class Code : UserControl
    {
        public Code()
        {
            InitializeComponent();

            (DataContext as CodeViewModel).CodeView = this;
        }

        private void CatClick(object sender, MouseButtonEventArgs e)
        {
            Animations.Shake(1, 15, new TimeSpan(00, 00, 00, 00, 500), Caption).Begin();
            Animations.Shake(1, 15, new TimeSpan(00, 00, 00, 00, 500), CatImage).Begin();
        }

        private void CodeField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length >= 6)
            {
                (DataContext as CodeViewModel).CodeError = "";
                RestoreCode();
            }
        }


        public void WrongOrEmptyCode()
        {
            Dispatcher.Invoke(() =>
            {
                Animations.BrushColorTo(Animations.DiscordLPWrongOrEmpty, Label.ForegroundProperty, CodeLabel).Begin();
                Animations.BrushColorTo(Animations.DiscordLPWrongOrEmpty, TextBox.BorderBrushProperty, CodeField).Begin();
            });
        }

        public void RestoreCode()
        {
            Dispatcher.Invoke(() =>
            {
                Animations.BrushColorTo(Animations.DiscordLPLabelDefault, Label.ForegroundProperty, CodeLabel).Begin();
                Animations.BrushColorTo(Animations.DiscordLPTextBoxDefault, TextBox.BorderBrushProperty, CodeField).Begin();
            });
        }
    }
}
