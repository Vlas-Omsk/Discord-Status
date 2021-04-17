using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using GitHashes;
using WEBLib;
using PinkJson;
using DiscordStatusGUI.Extensions;

namespace DiscordStatusGUI
{
    class UpdateManager
    {
        const string user = "Vlas-Omsk";
        const string repository = "Discord-Status";
        const string branch = "dev";
        const string token = "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";

        static string release_latest = $"https://api.github.com/repos/{user}/{repository}/releases/latest";
        public const string download_latest = "https://vlas-omsk.github.io/Discord-Status";

        static Thread ResourcesSyncThread = new Thread(ResourcesSync);
        static string[] SourceData = { branch, "DiscordStatusGUI", "Data" };
        static string DestinationData = "Data\\";
        static string[] SourceLocales = { branch, "DiscordStatusGUI", "locales" };
        static string DestinationLocales = "Locales\\";

        public static bool IsUpdateAvailable(out double newversion, out string tagname)
        {
            tagname = null;
            try
            {
                tagname = new Json(WEB.Get(release_latest))["tag_name"].Value.ToString();
            }
            catch
            {
                newversion = double.NaN;
                return false;
            }
            if (double.TryParse(tagname.TrimStart('v').Replace('.', ','), out double version))
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

        public static void Init()
        {
#if DEBUG == false
            ResourcesSyncThread.IsBackground = true;
            ResourcesSyncThread.Start();
#endif
        }

        static void ResourcesSync()
        {
            try
            {
                Directory.CreateDirectory(DestinationData);
                Directory.CreateDirectory(DestinationLocales);

                var data_hash = Hashes.GetHashByPath(user, repository, SourceData, token);
                ConsoleEx.WriteLine(ConsoleEx.UpdateManager, "data_hash: " + data_hash);
                var locales_hash = Hashes.GetHashByPath(user, repository, SourceLocales, token);
                ConsoleEx.WriteLine(ConsoleEx.UpdateManager, "locales_hash: " + locales_hash);

                foreach (var change in Hashes.CompareWithGithub(DestinationData, user, repository, data_hash, "trees", token))
                {
                    ConsoleEx.WriteLine(ConsoleEx.UpdateManager, "DataChange: " + change);
                    change.Sync(token);
                }
                foreach (var change in Hashes.CompareWithGithub(DestinationLocales, user, repository, locales_hash, "trees", token, new string[] { DestinationLocales + "lang.cs" }))
                {
                    ConsoleEx.WriteLine(ConsoleEx.UpdateManager, "LocaleChange: " + change);
                    change.Sync(token);
                }
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteLine(ConsoleEx.Warning, "ResourcesSync() -> " + ex);
            }
        }
    }
}