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

        public static DateTime FromUNIX(long unix)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddMilliseconds(unix).ToLocalTime();
            return dtDateTime;
        }
    }
}
