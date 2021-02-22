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

namespace DiscordStatusGUI.Models
{
    /// <summary>
    /// Логика взаимодействия для Notification.xaml
    /// </summary>
    public partial class Notification : UserControl
    {
        public Notification(string title, string description, bool visible)
        {
            InitializeComponent();

            Title = title;
            Description = description;
            link.MouseUp += (s, e) => LinkAction?.Invoke();

            if (visible)
                Loaded += Notification_Loaded_Visible;
        }

        private void Notification_Loaded_Visible(object sender, RoutedEventArgs e)
        {
            IsVisible = true;
        }

        System.Timers.Timer HideTimer;

        public Notification(string title, string description, bool visible, int ms) : this(title, description, visible)
        {
            HideTimer = new System.Timers.Timer(ms) { Enabled = true, AutoReset = false };
            HideTimer.Elapsed += HideTimer_Elapsed;
        }

        private void HideTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            IsVisible = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsVisible = false;
        }

        public bool IsClosable
        {
            get => close.Visibility == Visibility.Visible;
            set => close.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        }

        public string Title
        {
            get => title.Content.ToString();
            set => title.Content = value;
        }

        public string Description
        {
            get => description.Text;
            set => description.Text = value;
        }

        Action _LinkAction;
        public Action LinkAction
        {
            get => _LinkAction;
            set => _LinkAction = value;
        }
        public string LinkText
        {
            get => link.Text;
            set => link.Text = value;
        }

        public new Brush Background
        {
            get => root.Background;
            set => root.Background = value;
        }

        public Brush TitleForeground
        {
            get => title.Foreground;
            set => title.Foreground = value;
        }

        public Brush DescriptionForeground
        {
            get => description.Foreground;
            set => description.Foreground = value;
        }

        const double DefaultHeight = 90;

        public new bool IsVisible
        {
            get => root.Visibility == Visibility.Visible;
            set
            {
                Dispatcher.Invoke(() =>
                {
                    if (value && !root.IsVisible)
                    {
                        root.Visibility = Visibility.Visible;
                        IsVisibleChanged?.Invoke(this, new EventArgs());

                        Storyboard storyboard = new Storyboard();

                        DoubleAnimation height = new DoubleAnimation(DefaultHeight, TimeSpan.FromMilliseconds(200));
                        storyboard.Children.Add(height);

                        DoubleAnimation opacity = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(200));
                        storyboard.Children.Add(opacity);

                        Storyboard.SetTargetProperty(height, new PropertyPath(Control.HeightProperty));
                        Storyboard.SetTarget(height, this);

                        Storyboard.SetTargetProperty(opacity, new PropertyPath(Control.OpacityProperty));
                        Storyboard.SetTarget(opacity, root);

                        storyboard.Begin();
                    }
                    else if (!value && root.IsVisible)
                    {
                        Storyboard storyboard = new Storyboard();

                        DoubleAnimation height = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
                        storyboard.Children.Add(height);

                        DoubleAnimation opacity = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
                        opacity.Completed += (s, e) =>
                        {
                            root.Visibility = Visibility.Hidden;
                            IsVisibleChanged?.Invoke(this, new EventArgs());
                        };
                        storyboard.Children.Add(opacity);

                        Storyboard.SetTargetProperty(height, new PropertyPath(Control.HeightProperty));
                        Storyboard.SetTarget(height, this);

                        Storyboard.SetTargetProperty(opacity, new PropertyPath(Control.OpacityProperty));
                        Storyboard.SetTarget(opacity, root);

                        storyboard.Begin();
                    }
                });
            }
        }

        public new event EventHandler IsVisibleChanged;
    }
}
