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

            var ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = Properties.Resources.logo;
            ni.Visible = true;
            ni.Text = Static.Titile;
            ni.MouseUp += Ni_MouseUp;

            MouseHook.OnMouseButtonDown += Static_OnMouseButtonClick;
        }

        private void Static_OnMouseButtonClick(int x, int y, MouseButton button)
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
    }
}
