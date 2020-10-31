using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
