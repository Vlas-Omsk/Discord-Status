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
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolInverter : MarkupExtension, IValueConverter
    {
        private static BoolInverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new BoolInverter());
        }
    }
}
