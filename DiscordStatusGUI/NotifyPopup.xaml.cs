using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiscordStatusGUI
{
    /// <summary>
    /// Логика взаимодействия для TopPanel.xaml
    /// </summary>
    public partial class NotifyPopup : Popup
    {
        public NotifyPopup()
        {
            InitializeComponent();

            this.PopupAnimation = PopupAnimation.Fade;

            NotifyIcon = new System.Windows.Forms.NotifyIcon();
            NotifyIcon.Icon = Properties.Resources.logo;
            NotifyIcon.Visible = true;
            NotifyIcon.Text = Static.Title;
            NotifyIcon.MouseUp += Ni_MouseUp;

            MouseHook.OnMouseButtonDown += Static_OnMouseButtonClick;
        }

        public System.Windows.Forms.NotifyIcon NotifyIcon { get; private set; }

        private void Static_OnMouseButtonClick(object sebder, MouseButtonEventArgsEx e)
        {
            if (!this.IsMouseOver)
                this.IsOpen = false;
        }

        private void Ni_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                this.IsOpen = true;
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
                Static.Window.Normalize();
        }

        Action BalloonTipClicked;
        public void ShowBalloon(int timeout, string tipTitle, string tipText, System.Windows.Forms.ToolTipIcon tipIcon, Action clicked)
        {
            NotifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
            BalloonTipClicked = clicked;
            NotifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            NotifyIcon.BalloonTipClicked -= NotifyIcon_BalloonTipClicked;
            BalloonTipClicked?.Invoke();
        }
    }
}