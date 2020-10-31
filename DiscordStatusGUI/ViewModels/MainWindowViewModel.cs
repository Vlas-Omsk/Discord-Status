using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.Properties;
using DiscordStatusGUI.Views;
using DiscordStatusGUI.Views.Discord;

namespace DiscordStatusGUI.ViewModels
{
    class MainWindowViewModel : TemplateViewModel
    {
        public string Title { get => Static.Titile; }

        Brush _GlobalBackground = new ImageBrush() { ImageSource = BitmapEx.ToImageSource(Resources.Background), Stretch = Stretch.UniformToFill };
        public Brush GlobalBackground
        {
            get => _GlobalBackground;
            set
            {
                _GlobalBackground = value;
                OnPropertyChanged("GlobalBackground");
            }
        }

        UserControl _CurrentPage;
        public UserControl CurrentPage
        {
            get => _CurrentPage;
            set
            {
                if (_CurrentPage != null)
                {
                    var anim = Animations.VisibleOff(_CurrentPage);
                    anim.Completed += (s, e) => On();
                    anim.Begin();
                }
                else
                    On();

                void On()
                {
                    _CurrentPage = value;
                    OnPropertyChanged("CurrentPage");
                    if (_CurrentPage != null)
                        Animations.VisibleOn(_CurrentPage).Begin();
                }
            }
        }


        public MainWindowViewModel()
        {
            
        }
    }
}
