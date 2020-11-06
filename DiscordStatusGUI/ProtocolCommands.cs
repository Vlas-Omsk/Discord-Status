using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PinkJson.Parser;

namespace DiscordStatusGUI
{
    public class ProtocolCommands
    {
        private static readonly string open_command = $"\"{Environment.GetCommandLineArgs()[0]}\" --url \"%1\"";
        private static readonly RegistryKey CLASSES_ROOT = Registry.ClassesRoot;

        private static bool IsProtocolRegistered()
        {
            return CLASSES_ROOT.OpenSubKey("discordstatus")?.OpenSubKey("shell")?.OpenSubKey("open")?.OpenSubKey("command")?.GetValue("")?.ToString() == open_command;
        }

        public static void CreateProtocol()
        {
            if (!IsProtocolRegistered())
            {
                var discordstatus = CLASSES_ROOT.CreateSubKey("discordstatus");
                var shell = discordstatus.CreateSubKey("shell");
                var command = shell.CreateSubKey("open").CreateSubKey("command");

                discordstatus.SetValue("", "URL:DiscordStatus");
                discordstatus.SetValue("URL Protocol", "");
                shell.SetValue("", "open");
                command.SetValue("", open_command);
            }
        }

        public static void SetPropertiesByURL(string url)
        {
            Uri myUri = new Uri(url.Trim(1));
            var get_params = System.Web.HttpUtility.ParseQueryString(myUri.Query);

            Static.MainWindow.Dispatcher.Invoke(() =>
            {
                foreach (var s in get_params.AllKeys)
                {
                    var value = get_params[s].ToLower();
                    switch (s.ToLower())
                    {
                        case "windowstate":
                            switch (value)
                            {
                                case "opened": Static.Window.Normalize(); break;
                                case "closed": Static.Window.Close(); break;
                            }
                            break;
                        case "currentactivityindex":
                            if (int.TryParse(value, out int result))
                                Preferences.CurrentActivityIndex = result;
                            break;
                    }
                }
            });
        }
    }
}
