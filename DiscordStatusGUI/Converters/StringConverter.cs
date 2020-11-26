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
    public class StringConverter : MarkupExtension, IValueConverter
    {
        private static StringConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            var outt = System.Convert.ToString(parameter)
                .Replace("[Value]", value?.ToString())
                .Replace("[Value.ToUpper]", System.Convert.ToString(value).ToUpper())
                .Replace("[Value.ToStars]", new string('*', value.ToString().Length))
                .Replace("[Value.ReplaceFields]", Static.ReplaceFilds(value + ""));
            try
            {
                outt = System.Convert.ToString(outt)
                    .Replace("[DoubleValue.Round]", System.Convert.ToString(System.Convert.ToInt64(value)));
            }
            catch { }

            if (outt.Contains("[Value.Split,"))
            {
                var s = outt.IndexOf("[Value.Split,");
                var l = s + "[Value.Split,".Length;
                var e = outt.IndexOf("]", l);
                var i = System.Convert.ToInt32(outt.Substring(l, e - l));
                outt = outt.Remove(s, e - s + 1).Insert(s, SplitCommandLineParams(value.ToString())[i]);
            }

            return outt;
        }

        private static string[] SplitCommandLineParams(string str)
        {
            var result = new List<string>();
            var locked = false;
            var p = "";
            for (var i = 0; i < str.Length; i++)
            {
                p += str[i];
                if (str[i] == '\"') locked = !locked;
                if (str[i] == ' ' && !locked)
                {
                    result.Add(p.Trim().Trim('"'));
                    p = "";
                }
            }
            result.Add(p.Trim().Trim('"'));
            return result.ToArray();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new StringConverter());
        }
    }
}
