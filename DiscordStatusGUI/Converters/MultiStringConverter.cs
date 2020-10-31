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
    [ValueConversion(typeof(object), typeof(string))]
    public class MultiStringConverter : MarkupExtension, IMultiValueConverter
    {
        private static MultiStringConverter _instance;
        private static StringConverter _StringConverter = new StringConverter();

        #region IValueConverter Members
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
         => string.Join("", values.Select(e => e.ToString())).Replace(parameter.ToString(), "");

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
          => throw new NotImplementedException();

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new MultiStringConverter());
        }
    }
}
