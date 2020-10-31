using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using PinkJson.Parser;

namespace DiscordStatusGUI.Locales
{
    public static class JsonExtensions
    {
        public static string GetString(this Json resources, string resourceString, object resourceCulture)
        {
            if (Lang.CurrentLanguage != null && Lang.CurrentLanguage.IndexByKey(resourceString) != -1)
                return Lang.CurrentLanguage[resourceString].Value.ToString();
            else
                if (Lang.DefaultLanguage != null && Lang.DefaultLanguage.IndexByKey(resourceString) != -1)
                    return Lang.DefaultLanguage[resourceString].Value.ToString();
                else 
                    return $"\"{resourceString}\" resource not found";
        }
    }

    class Lang
    {
#if DEBUG
        public static Json DefaultLanguage = LoadLocale(@"G:\CSharp\_WPF\DiscordStatusGUI\DiscordStatusGUI\locales\default.json");
#else
        public static Json DefaultLanguage;
#endif
        public static CultureInfo DefaultCultureInfo = CultureInfo.GetCultureInfo("en");
        public static Json CurrentLanguage = null;
        public static CultureInfo CurrentCultureInfo = CultureInfo.CurrentCulture;

        private static Json ResourceManager
        {
            get
            {
                if (CurrentLanguage != null)
                    return CurrentLanguage;
                return DefaultLanguage;
            }
        }
        private static CultureInfo resourceCulture = CultureInfo.CurrentCulture;

        public static void Init()
        {
            if (Directory.Exists("Locales"))
            {
#if DEBUG
#else
                DefaultLanguage = LoadLocale(@"locales\default.json");
#endif

                if (File.Exists("locales\\" + CurrentCultureInfo.Name + ".json"))
                {
                    CurrentLanguage = LoadLocale("locales\\" + CurrentCultureInfo.Name + ".json");
                }
                else if (File.Exists("locales\\" + CurrentCultureInfo.Parent + ".json"))
                {
                    CurrentLanguage = LoadLocale("locales\\" + CurrentCultureInfo.Parent.Name + ".json");
                }
            }

            Static.InitializationSteps.IsLanguageInitialized = true;
        }

        private static Json LoadLocale(string path)
        {
            return new Json(File.ReadAllText(path));
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Mrrr....
        /// </summary>
        public static string CatEmotion_login_default
        {
            get
            {
                return ResourceManager.GetString("CatEmotion_login_default", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Back to login.
        /// </summary>
        public static string CodeForm_backToLoginForm
        {
            get
            {
                return ResourceManager.GetString("CodeForm_backToLoginForm", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Two-factor authentication.
        /// </summary>
        public static string CodeForm_caption
        {
            get
            {
                return ResourceManager.GetString("CodeForm_caption", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на ENTER AUTHENTICATION CODE OR BACKUP CODE DISCORD.
        /// </summary>
        public static string CodeForm_header
        {
            get
            {
                return ResourceManager.GetString("CodeForm_header", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на You can use a backup code or your two-factor authentication mobile app..
        /// </summary>
        public static string CodeForm_header2
        {
            get
            {
                return ResourceManager.GetString("CodeForm_header2", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Get authorization code by SMS.
        /// </summary>
        public static string CodeForm_sendSMS
        {
            get
            {
                return ResourceManager.GetString("CodeForm_sendSMS", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Login.
        /// </summary>
        public static string CodeForm_signIn
        {
            get
            {
                return ResourceManager.GetString("CodeForm_signIn", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Initialization.
        /// </summary>
        public static string InitForm_initialization
        {
            get
            {
                return ResourceManager.GetString("InitForm_initialization", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Please, wait....
        /// </summary>
        public static string InitForm_wait
        {
            get
            {
                return ResourceManager.GetString("InitForm_wait", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на en.
        /// </summary>
        public static string Language
        {
            get
            {
                return ResourceManager.GetString("Language", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на E-MAIL.
        /// </summary>
        public static string LoginForm_email
        {
            get
            {
                return ResourceManager.GetString("LoginForm_email", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Forgot password?.
        /// </summary>
        public static string LoginForm_forgotPassword
        {
            get
            {
                return ResourceManager.GetString("LoginForm_forgotPassword", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Login or create your own Discord account so that the application can set your status..
        /// </summary>
        public static string LoginForm_header
        {
            get
            {
                return ResourceManager.GetString("LoginForm_header", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Need an account?.
        /// </summary>
        public static string LoginForm_needAccount
        {
            get
            {
                return ResourceManager.GetString("LoginForm_needAccount", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на PASSWORD.
        /// </summary>
        public static string LoginForm_password
        {
            get
            {
                return ResourceManager.GetString("LoginForm_password", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Login.
        /// </summary>
        public static string LoginForm_signIn
        {
            get
            {
                return ResourceManager.GetString("LoginForm_signIn", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Sign Up.
        /// </summary>
        public static string LoginForm_signUp
        {
            get
            {
                return ResourceManager.GetString("LoginForm_signUp", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Skip.
        /// </summary>
        public static string LoginForm_skip
        {
            get
            {
                return ResourceManager.GetString("LoginForm_skip", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Status Visualizer.
        /// </summary>
        public static string ContentForm_Visualizer_caption
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Visualizer_caption", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Here you can see how your status will look like.
        /// </summary>
        public static string ContentForm_Visualizer_header
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Visualizer_header", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Full Profile.
        /// </summary>
        public static string ContentForm_Visualizer_TabControl_TabItem1
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Visualizer_TabControl_TabItem1", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на User Popout.
        /// </summary>
        public static string ContentForm_Visualizer_TabControl_TabItem2
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Visualizer_TabControl_TabItem2", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Playing a game.
        /// </summary>
        public static string ContentForm_Visualizer_TabControl_TabItem1_Playing
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Visualizer_TabControl_TabItem1_Playing", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на User info.
        /// </summary>
        public static string ContentForm_Visualizer_TabControl_TabItem1_TabControl_TabItem1
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Visualizer_TabControl_TabItem1_TabControl_TabItem1", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Mutual Servers.
        /// </summary>
        public static string ContentForm_Visualizer_TabControl_TabItem1_TabControl_TabItem2
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Visualizer_TabControl_TabItem1_TabControl_TabItem2", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Mutual Friends.
        /// </summary>
        public static string ContentForm_Visualizer_TabControl_TabItem1_TabControl_TabItem3
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Visualizer_TabControl_TabItem1_TabControl_TabItem3", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Profiles:.
        /// </summary>
        public static string ContentForm_Visualizer_Options_Profiles
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Visualizer_Options_Profiles", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Main.
        /// </summary>
        public static string ContentForm_Settings_MainBlock
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Settings_MainBlock", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Volume.
        /// </summary>
        public static string ContentForm_Settings_Volume
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Settings_Volume", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Open at Windows startup.
        /// </summary>
        public static string ContentForm_Settings_AutoRun
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Settings_AutoRun", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Broadcast status to Discord.
        /// </summary>
        public static string ContentForm_Settings_StreamStatusDiscord
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Settings_StreamStatusDiscord", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Status update.
        /// </summary>
        public static string ContentForm_Settings_StatusUpdate
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Settings_StatusUpdate", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Automatically.
        /// </summary>
        public static string ContentForm_Settings_StatusUpdate_Automatically
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Settings_StatusUpdate_Automatically", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Manually.
        /// </summary>
        public static string ContentForm_Settings_StatusUpdate_Manually
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Settings_StatusUpdate_Manually", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Every N seconds.
        /// </summary>
        public static string ContentForm_Settings_StatusUpdate_EveryNSeconds
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Settings_StatusUpdate_EveryNSeconds", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Connected accounts.
        /// </summary>
        public static string ContentForm_Settings_ConnectedAccounts
        {
            get
            {
                return ResourceManager.GetString("ContentForm_Settings_ConnectedAccounts", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Use these lines to broadcast your game status:.
        /// </summary>
        public static string ContentForm_WarfaceScr_header
        {
            get
            {
                return ResourceManager.GetString("ContentForm_WarfaceScr_header", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Event \"Dark Samurai\".
        /// </summary>
        public static string ContentForm_WarfaceScr_BlackSamurai
        {
            get
            {
                return ResourceManager.GetString("ContentForm_WarfaceScr_BlackSamurai", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Receive sound notifications if cases are available.
        /// </summary>
        public static string ContentForm_WarfaceScr_BlackSamurai_NotifyOnNewCase
        {
            get
            {
                return ResourceManager.GetString("ContentForm_WarfaceScr_BlackSamurai_NotifyOnNewCase", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Match analytics.
        /// </summary>
        public static string ContentForm_WarfaceScr_Analytics
        {
            get
            {
                return ResourceManager.GetString("ContentForm_WarfaceScr_Analytics", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Status Visualizer.
        /// </summary>
        public static string ContentForm_ScrollMenu_MenuVisualizer
        {
            get
            {
                return ResourceManager.GetString("ContentForm_ScrollMenu_MenuVisualizer", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Settings.
        /// </summary>
        public static string ContentForm_ScrollMenu_MenuSettings
        {
            get
            {
                return ResourceManager.GetString("ContentForm_ScrollMenu_MenuSettings", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Developer Console.
        /// </summary>
        public static string ContentForm_ScrollMenu_MenuConsole
        {
            get
            {
                return ResourceManager.GetString("ContentForm_ScrollMenu_MenuConsole", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на You forgot to save your changes!
        /// </summary>
        public static string SaveChangesBox_header
        {
            get
            {
                return ResourceManager.GetString("SaveChangesBox_header", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Save changes.
        /// </summary>
        public static string SaveChangesBox_SaveChanges
        {
            get
            {
                return ResourceManager.GetString("SaveChangesBox_SaveChanges", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Reset.
        /// </summary>
        public static string SaveChangesBox_Reset
        {
            get
            {
                return ResourceManager.GetString("SaveChangesBox_Reset", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Ok.
        /// </summary>
        public static string MessageBox_DftButtonOk
        {
            get
            {
                return ResourceManager.GetString("MessageBox_DftButtonOk", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Caption.
        /// </summary>
        public static string MessageBox_DftCaption
        {
            get
            {
                return ResourceManager.GetString("MessageBox_DftCaption", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Content.
        /// </summary>
        public static string MessageBox_DftContent
        {
            get
            {
                return ResourceManager.GetString("MessageBox_DftContent", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Hide this menu...
        /// </summary>
        public static string NotifyPopup_Hide
        {
            get
            {
                return ResourceManager.GetString("NotifyPopup_Hide", resourceCulture);
            }
        }

        /// <summary>
        ///   Ищет локализованную строку, похожую на Quit Warface Status.
        /// </summary>
        public static string NotifyPopup_Exit
        {
            get
            {
                return ResourceManager.GetString("NotifyPopup_Exit", resourceCulture);
            }
        }
    }
}
