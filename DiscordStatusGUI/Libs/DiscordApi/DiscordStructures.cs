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

    public enum UserStatus
    {
        online,
        idle,
        dnd,
        invisible
    }
}