using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordStatusGUI.Libs.DiscordApi
{
    public enum AuthErrors
    {
        LoginError,
        Error,
        MultiFactorAuthentication,
        EmptyEmailOrPassword,
        Successful
    }

    public enum ForgotPasswordErrors
    {
        DataError,
        Error,
        Successful
    }

    public enum MFAuthType
    {
        Code,
        SMS
    }

    public enum MFAuthErrors
    {
        InvalidData,
        Error,
        Successful
    }

    public struct Activity
    {
        public string ProfileName { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string State { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ApplicationID { get; set; }
        public string ImageLargeKey { get; set; }
        public string ImageLargeText { get; set; }
        public string ImageSmallKey { get; set; }
        public string ImageSmallText { get; set; }
        public string PartySize { get; set; }
        public string PartyMax { get; set; }
        public bool IsAvailableForChange { get; set; }

        private object _SavedState;
        public object SavedState
        {
            get => _SavedState ?? (_SavedState = this);
            set => _SavedState = value;
        }
    }


    public struct UserInfo
    {
        public string UserName;
        public string Phone;
        public string Id;
        public string Email;
        public string Discriminator;
        public string AvatarId;
    }

    public enum UserStatus
    {
        online,
        idle,
        dnd,
        invisible
    }

    public class ActivityType
    {
        public string Format;
        public int ID;

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

        public string ToFormatString(Activity activity)
        {
            var tmp = Format;
            var pattern = @"\{(.*?)\}";
            var matches = Regex.Matches(tmp, pattern);
            
            foreach (Match m in matches)
            {
                var repl = "";
                foreach(var a in Static.ActivityFields)
                    if (a.Name == $"<{m.Groups[1].Value}>k__BackingField")
                        repl = a.GetValue(activity)?.ToString() + "";
                tmp = tmp.Replace($"{{{m.Groups[1].Value}}}", repl);
            }

            return tmp;
        }

        public static readonly ActivityType Game = new ActivityType(0, "Playing {Name}");
        public static readonly ActivityType Streaming = new ActivityType(1, "Streaming {Details}");
        public static readonly ActivityType Listening = new ActivityType(2, "Listening to {Name}");
        public static readonly ActivityType Custom = new ActivityType(4, "{Emoji} {Name}");
        public static readonly ActivityType Competing = new ActivityType(5, "Competing in {Name}");
    }    
}
