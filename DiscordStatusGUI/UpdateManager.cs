using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEBLib;
using PinkJson;

namespace DiscordStatusGUI
{
    class UpdateManager
    {
        const string release_latest = "https://api.github.com/repos/Vlas-Omsk/Discord-Status/releases/latest";
        public const string download_latest = "https://vlas-omsk.github.io/Discord-Status";

        public static bool IsUpdateAvailable(out double newversion)
        {
            string tag_name = null;
            try
            {
                tag_name = new Json(WEB.Get(release_latest))["tag_name"].Value.ToString();
            }
            catch
            {
                newversion = double.NaN;
                return false;
            }
            if (double.TryParse(tag_name.TrimStart('v').Replace('.', ','), out double version))
            {
                newversion = version;
                return version > Static.Version;
            }
            else
            {
                newversion = double.NaN;
                return false;
            }
        }
    }
}