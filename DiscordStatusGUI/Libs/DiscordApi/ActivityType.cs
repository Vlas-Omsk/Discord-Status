using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DiscordStatusGUI.Libs.DiscordApi
{
    public struct ActivityType
    {
        public string Format { get; set; }
        public int ID { get; set; }

        public ActivityType(int id, string format)
        {
            ID = id;
            Format = format;
        }

        //public static ActivityType ByID(int id)
        //{
        //    switch (id)
        //    {
        //        case 0: return Game;
        //        case 1: return Streaming;
        //        case 2: return Listening;
        //        case 4: return Custom;
        //        case 5: return Competing;
        //    }

        //    return null;
        //}

        public string ToString(Activity activity)
        {
            var tmp = Format;
            var pattern = @"\{(.*?)\}";
            var matches = Regex.Matches(tmp, pattern);

            foreach (Match m in matches)
            {
                var repl = "";
                foreach (var a in Static.ActivityFields)
                    if (a.Name == $"{m.Groups[1].Value}")
                        repl = a.GetValue(activity)?.ToString() + "";
                tmp = tmp.Replace($"{{{m.Groups[1].Value}}}", repl);
            }

            return tmp;
        }

        public override string ToString()
        {
            return Format;
        }

        public static readonly ActivityType Game = new ActivityType(0, "Playing {Name}");
        public static readonly ActivityType Streaming = new ActivityType(1, "Streaming {Details}");
        public static readonly ActivityType Listening = new ActivityType(2, "Listening to {Name}");
        public static readonly ActivityType Custom = new ActivityType(4, "{Emoji} {Name}");
        public static readonly ActivityType Competing = new ActivityType(5, "Competing in {Name}");
    }
}
