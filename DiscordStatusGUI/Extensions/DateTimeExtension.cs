using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.Extensions
{
    public class DateTimeEx
    {
        public static long ToUNIX(DateTime dt)
        {
            var dt2 = new DateTimeOffset(dt);
            return dt2.ToUnixTimeMilliseconds();
        }

        public static bool TryFromUNIX(long unix, out DateTime result)
        {
            try
            {
                result = FromUNIX(unix);
                return true;
            }
            catch { result = DateTime.Now; return false; }
        }

        public static DateTime FromUNIX(long unix)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddMilliseconds(unix).ToLocalTime();
            return dtDateTime;
        }
    }
}
