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
    [ValueConversion(typeof(bool), typeof(int))]
    public class BoolToIntConverter : MarkupExtension, IValueConverter
    {
        private static BoolToIntConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var split = parameter.ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var tr = System.Convert.ToInt32(split[0]);
            var fl = System.Convert.ToInt32(split[1]);
            var invert = System.Convert.ToBoolean(split[2]);

            return (bool)value ? (invert ? fl : tr) : (invert ? tr : fl);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new BoolToIntConverter());
        }
    }
}
