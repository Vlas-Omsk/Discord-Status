using DiscordStatusGUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.ViewModels.Dialogs
{
    class MessageBoxViewModel : TemplateViewModel
    {
        private ObservableCollection<ButtonItem> _Buttons;
        public ObservableCollection<ButtonItem> Buttons
        {
            get => _Buttons;
            set
            {
                _Buttons = value;
                OnPropertyChanged("Buttons");
            }
        }
        private string _Title = "YOUR TITLE HERE";
        public string Title
        {
            get => _Title;
            set
            {
                _Title = value;
                OnPropertyChanged("Title");
            }
        }
        private string _Text = "YOUR \r\n TEXT HERE";
        public string Text
        {
            get => _Text;
            set
            {
                _Text = value;
                OnPropertyChanged("Text");
            }
        }
        private double _Width = 440;
        public double Width
        {
            get => _Width;
            set
            {
                _Width = value;
                OnPropertyChanged("Width");
            }
        }
        private double _Height = 160;
        public double Height
        {
            get => _Height;
            set
            {
                _Height = value;
                OnPropertyChanged("Height");
            }
        }
        private string _ImagePath;
        public string ImagePath
        {
            get => _ImagePath;
            set
            {
                _ImagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }
        private double _ImageScale;
        public double ImageScale
        {
            get => _ImageScale;
            set
            {
                _ImageScale = value;
                OnPropertyChanged("ImageScale");
            }
        }
        private System.Windows.HorizontalAlignment _ButtonsAligment = System.Windows.HorizontalAlignment.Right;
        public System.Windows.HorizontalAlignment ButtonsAligment
        {
            get => _ButtonsAligment;
            set
            {
                _ButtonsAligment = value;
                OnPropertyChanged("ButtonsAligment");
            }
        }


        public Command BackCommand { get; set; }


        public MessageBoxViewModel()
        {
        }
    }
}
