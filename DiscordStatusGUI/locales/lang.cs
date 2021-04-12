using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Dynamic;
using PinkJson;

namespace DiscordStatusGUI.Locales
{
    class Lang : System.Windows.StaticResourceExtension
    {
        public Lang() : base()
        {
        }

        public Lang(object resourceKey) : base(resourceKey)
        {
        }

        public static CultureInfo DefaultCultureInfo = CultureInfo.GetCultureInfo("en-US");
        public static CultureInfo CurrentCultureInfo = CultureInfo.CurrentCulture;
#if DEBUG
#warning Set your location to locale file for constructor
        public static Json DefaultLanguage = LoadLocale(@"G:\CSharp\_WPF\DiscordStatusGUI\DiscordStatusGUI\Locales\default.json");
        public static Json CurrentLanguage = LoadLocale(@"G:\CSharp\_WPF\DiscordStatusGUI\DiscordStatusGUI\Locales\ru.json");
#else
        public static Json DefaultLanguage = null;
        public static Json CurrentLanguage = null;
#endif
        public static string CurrentWebLanguage
        {
            get => string.Format("{0}, {1};q=0.9, en-US;q=0.8, en;q=0.7, *;q=0.6", CurrentCultureInfo.Name, CurrentCultureInfo.Parent.Name);
        }

        public static void Init()
        {
            if (Directory.Exists("locales"))
            {
                if (File.Exists(@"locales\default.json"))
                    DefaultLanguage = LoadLocale(@"locales\default.json");

                if (File.Exists("locales\\" + CurrentCultureInfo.Name + ".json"))
                    CurrentLanguage = LoadLocale("locales\\" + CurrentCultureInfo.Name + ".json");
                else if (File.Exists("locales\\" + CurrentCultureInfo.Parent.Name + ".json"))
                    CurrentLanguage = LoadLocale("locales\\" + CurrentCultureInfo.Parent.Name + ".json");
            }

            Static.InitializationSteps.IsLanguageInitialized = true;
        }

        private static Json LoadLocale(string path)
        {
            return new Json(File.ReadAllText(path));
        }

        private static bool TryGetResource(Json language, string key, out string value)
        {
            value = null;

            var path = key.Split(':');
            for (var i = 0; i < path.Length; i++)
            {
                if (language.IndexByKey(path[i]) == -1)
                    return false;
                if (i == path.Length - 1)
                {
                    value = language[path[i]].Value.ToString();
                    return true;
                }
                language = language[path[i]].Get<Json>();
            }

            return false;
        }

        public static string GetResource(string key)
        {
            string value;

            if (CurrentLanguage != null && TryGetResource(CurrentLanguage, key, out value))
                return value;
            else if (DefaultLanguage != null && TryGetResource(DefaultLanguage, key, out value))
                return value;
            else
                return $"@{key}";
        }

        public string this[string key]
        {
            get => GetResource(key);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return GetResource(this.ResourceKey.ToString());
        }
    }
}