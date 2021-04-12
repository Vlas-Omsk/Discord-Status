using DiscordStatusGUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.ViewModels.Dialogs
{
    class DateTimePickerViewModel : TemplateViewModel
    {
        public ObservableCollection<string> Hours
        {
            get
            {
                var result = new ObservableCollection<string>();
                for (var i = 0; i <= 23; i++)
                    result.Add(i.ToString());
                return result;
            }
        }
        public ObservableCollection<string> Minutes
        {
            get
            {
                var result = new ObservableCollection<string>();
                for (var i = 0; i <= 59; i++)
                    result.Add(i.ToString());
                return result;
            }
        }
        public ObservableCollection<string> Seconds
        {
            get
            {
                var result = new ObservableCollection<string>();
                for (var i = 0; i <= 59; i++)
                    result.Add(i.ToString());
                return result;
            }
        }

        private int _SelectedHour = DateTime.Now.Hour;
        public int SelectedHour
        {
            get => _SelectedHour;
            set
            {
                _SelectedHour = value;
                if (_SelectedHour >= 24)
                    _SelectedHour = 0;
                OnPropertyChanged("SelectedHour");
                OnPropertyChanged("HourArrowAngle");
            }
        }
        private int _SelectedMinute = DateTime.Now.Minute;
        public int SelectedMinute
        {
            get => _SelectedMinute;
            set
            {
                _SelectedMinute = value;
                if (_SelectedMinute >= 60)
                    _SelectedMinute = 0;
                OnPropertyChanged("SelectedMinute");
                OnPropertyChanged("MinuteArrowAngle");
            }
        }
        private int _SelectedSecond = DateTime.Now.Second;
        public int SelectedSecond
        {
            get => _SelectedSecond;
            set
            {
                _SelectedSecond = value;
                if (_SelectedSecond >= 60)
                    _SelectedSecond = 0;
                OnPropertyChanged("SelectedSecond");
                OnPropertyChanged("SecondArrowAngle");
            }
        }

        private const int HourAngle = 360 / 12;
        private const int MinuteAngle = 360 / 60;
        public double HourArrowAngle
        {
            get => _SelectedHour * HourAngle;
        }
        public double MinuteArrowAngle
        {
            get => _SelectedMinute * MinuteAngle;
        }
        public double SecondArrowAngle
        {
            get => _SelectedSecond * MinuteAngle;
        }


        public ObservableCollection<string> Days
        {
            get
            {
                var result = new ObservableCollection<string>();
                for (var i = 1; i <= DateTime.DaysInMonth(1970 + SelectedYear, 1 + SelectedMonth); i++)
                    result.Add(i.ToString());
                return result;
            }
        }
        public ObservableCollection<string> Months
        {
            get
            {
                var result = new ObservableCollection<string>();
                for (var i = 1; i <= 12; i++)
                    result.Add(i.ToString());
                return result;
            }
        }
        public ObservableCollection<string> Years
        {
            get
            {
                var result = new ObservableCollection<string>();
                for (var i = 1970; i <= 2030; i++)
                    result.Add(i.ToString());
                return result;
            }
        }

        private int _SelectedDay = DateTime.Now.Day - 1;
        public int SelectedDay
        {
            get
            {
                if (_SelectedDay == -1)
                    _SelectedDay = 0;
                return _SelectedDay;
            }
            set
            {
                _SelectedDay = value;
                OnPropertyChanged("SelectedDay");
            }
        }
        private int _SelectedMonth = DateTime.Now.Month - 1;
        public int SelectedMonth
        {
            get => _SelectedMonth;
            set
            {
                _SelectedMonth = value;
                OnPropertyChanged("SelectedMonth");
                OnPropertyChanged("Days");
                OnPropertyChanged("SelectedDay");
            }
        }
        private int _SelectedYear = DateTime.Now.Year - 1970;
        public int SelectedYear
        {
            get => _SelectedYear;
            set
            {
                _SelectedYear = value;
                OnPropertyChanged("SelectedYear");
                OnPropertyChanged("Days");
                OnPropertyChanged("SelectedDay");
            }
        }

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


        public void SetDateTime(DateTime dt)
        {
            SelectedHour = dt.Hour;
            SelectedMinute = dt.Minute;
            SelectedSecond = dt.Second;
            SelectedDay = dt.Day - 1;
            SelectedMonth = dt.Month - 1;
            SelectedYear = dt.Year - 1970;
        }

        public DateTime GetDateTime()
        {
            return new DateTime(SelectedYear + 1970, SelectedMonth + 1, SelectedDay + 1, SelectedHour, SelectedMinute, SelectedSecond);
        }

        public DateTimePickerViewModel()
        {
        }
    }
}
