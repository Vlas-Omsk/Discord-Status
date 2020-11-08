using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace DiscordStatusGUI.Converters
{
    [ValueConversion(typeof(bool), typeof(Thickness))]
    public class BoolToMarginConverter : MarkupExtension, IValueConverter
    {
        private static BoolToMarginConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            //var b = System.Convert.ToBoolean(parameter) ? !(bool)value : (bool)value;
            var t = parameter.ToString().Split('|')[0].Split('.');
            var f = parameter.ToString().Split('|')[1].Split('.');
            return System.Convert.ToBoolean(value) ? new Thickness(int.Parse(t[0]), int.Parse(t[1]), int.Parse(t[2]), int.Parse(t[3])) : new Thickness(int.Parse(f[0]), int.Parse(f[1]), int.Parse(f[2]), int.Parse(f[3]));
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new BoolToMarginConverter());
        }
    }
}
