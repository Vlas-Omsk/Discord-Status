using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using DiscordStatusGUI.Extensions;
using System.IO;
using System.Text.RegularExpressions;
using PinkJson;

namespace DiscordStatusGUI
{
    //What the fuck am I?
    class DiscordUniversalStealer
    {
        static string local = Environment.GetEnvironmentVariable("LOCALAPPDATA"),
            roaming = Environment.GetEnvironmentVariable("APPDATA");

        static string[] Paths = new string[]
        {
            //Discord
            roaming + "\\Discord",
            //DiscordCanary
            roaming + "\\discordcanary",
            //DiscordPTB
            roaming + "\\discordptb"
        }.Concat(CookieGrabber.CookieTool.ChromiumBasedBrowserPaths).ToArray();


        static Thread StealDiscordToken_Thread;

        public static void Init(bool b = false)
        {
            StealDiscordToken_Thread = new Thread(() =>
            {
                if (b || !Libs.DiscordApi.Discord.IsTokenValid(Static.Discord?.Token))
                {
                    Static.Window.SetTopStatus("(v2) Search discord token");
                    Static.MainWindow.Dispatcher.Invoke(() =>
                    {
                        if (Static.CurrentPage != null && Static.CurrentPage.DataContext is ViewModels.Discord.LoginViewModel)
                            (Static.CurrentPage.DataContext as ViewModels.Discord.LoginViewModel).LoginButtonEnabled = false;
                    });
                    bool isTokenFinded = false;
                    foreach (var path in Paths)
                    {
                        if (!Directory.Exists(path))
                            continue;

                        var tokens = FindTokens(path);

                        if (tokens.Count != 0)
                        {
                            var token = tokens.Last();
                            ConsoleEx.WriteLine(ConsoleEx.Info, "(v2) Discord token found: " + token.Trim(0, token.Length - 10) + new string('*', token.Length - 10));
                            Static.Discord.Token = token;
                            Static.DiscordLoginSuccessful();
                            isTokenFinded = true;

                            break;
                        }
                    }

                    if (!isTokenFinded)
                        foreach (var profiles in CookieGrabber.CookieTool.GeckoBasedBrowsersList)
                        {
                            if (isTokenFinded)
                                break;
                            if (!Directory.Exists(profiles))
                                continue;

                            foreach (var path in Directory.GetDirectories(profiles))
                            {
                                var tokens = FindGeckoTokens(path);
                                if (tokens.Count != 0)
                                {
                                    var token = tokens.Last();
                                    ConsoleEx.WriteLine(ConsoleEx.Info, "(v2) Discord token found: " + token.Trim(0, token.Length - 10) + new string('*', token.Length - 10));
                                    Static.Discord.Token = token;
                                    Static.DiscordLoginSuccessful();
                                    isTokenFinded = true;

                                    break;
                                }
                            }
                        }

                    Static.MainWindow.Dispatcher.Invoke(() =>
                    {
                        if (Static.CurrentPage != null && Static.CurrentPage.DataContext is ViewModels.Discord.LoginViewModel)
                            (Static.CurrentPage.DataContext as ViewModels.Discord.LoginViewModel).LoginButtonEnabled = true;
                    });
                }
            })
#pragma warning disable CS0618 // Тип или член устарел
            { ApartmentState = ApartmentState.STA, IsBackground = true };
#pragma warning restore CS0618 // Тип или член устарел
            StealDiscordToken_Thread.Start();

            Thread.Sleep(0);
        }

        static List<string> FindTokens(string path, bool verify = true)
        {
            path += "\\Local Storage\\leveldb";

            var tokens = new List<string>();
            List<Match> mfa_matches = new List<Match>(), def_matches = new List<Match>();

            foreach (var file in Directory.EnumerateFiles(path).Where(str => str.EndsWith(".log") || str.EndsWith(".ldb")))
                foreach (var line in FileInfoEx.SafeReadLines(file))
                {
                    mfa_matches.AddRange(Regex.Matches(line, @"mfa\.[\w-]{84}").OfType<Match>());
                    def_matches.AddRange(Regex.Matches(line, @"[\w-]{24}\.[\w-]{6}\.[\w-]{27}").OfType<Match>());
                }
            foreach (Match match in mfa_matches)
                if (verify)
                {
                    if (Libs.DiscordApi.Discord.IsTokenValid(match.Value))
                        tokens.Add(match.Value);
                }
                else
                    tokens.Add(match.Value);
            if (tokens.Count == 0)
                foreach (Match match in def_matches)
                    if (verify)
                    {
                        if (Libs.DiscordApi.Discord.IsTokenValid(match.Value))
                            tokens.Add(match.Value);
                    }
                    else
                        tokens.Add(match.Value);

            return tokens;
        }

        static List<string> FindGeckoTokens(string path, bool verify = true)
        {
            var tokens = new List<string>();
            
            var storePath = path + "\\webappsstore.sqlite";
            if (!File.Exists(storePath))
                return tokens;
            if (File.Exists(storePath + "cbtemp"))
                File.Delete(storePath + "cbtemp");

            File.Copy(storePath, storePath + "cbtemp");
            storePath += "cbtemp";

            var sqlite = new CookieGrabber.SQLite(storePath);
            sqlite.ReadTable("webappsstore2");

            for (int i = 0; i < sqlite.GetRowCount(); i++)
            {
                if ((sqlite.GetString(i, 1).Contains("drocsid") || sqlite.GetString(i, 2).Contains("drocsid")) && sqlite.GetString(i, 3) == "token")
                {
                    var token = sqlite.GetString(i, 4).Substring(1);
                    token = token.Substring(0, token.Length - 1);
                    if (verify)
                    {
                        if (Libs.DiscordApi.Discord.IsTokenValid(token))
                            tokens.Add(token);
                    }
                    else
                        tokens.Add(token);
                }
            }

            File.Delete(storePath);

            return tokens;
        }
    }

//    class DiscordAppStealer
//    {
//        [DllImport(@"DiscordStatusGUI.CPP.dll")]
//        static extern IntPtr GetDiscordToken(int pid, int skip);

//        public static bool TryGetDiscordToken(int pid, out string token)
//        {
//            token = Marshal.PtrToStringAnsi(GetDiscordToken(pid, 4));

//            return !string.IsNullOrEmpty(token);
//        }


//        static Thread StealDiscordToken_Thread;

//        public static void Init()
//        {
//            ProcessEx.OnProcessOpened += ProcessEx_OnProcessOpened;
//        }

//        private static void ProcessEx_OnProcessOpened(Processes processes)
//        {
//            var discord = processes.GetProcessesByNames("Discord");

//            if (discord != null && discord.Count == 0)
//                return;

//            StealDiscordToken(discord);
//        }

//        public static void StealDiscordToken(Processes processes)
//        {
//            if (StealDiscordToken_Thread?.IsAlive != null && StealDiscordToken_Thread.IsAlive)
//                return;

//            StealDiscordToken_Thread = new Thread(() =>
//            {
//                if (!Libs.DiscordApi.Discord.IsTokenValid(Static.Discord?.Token))
//                {
//                    Static.Window.SetTopStatus("Search discord token");
//                    Static.MainWindow.Dispatcher.Invoke(() =>
//                    {
//                        if (Static.CurrentPage.DataContext is ViewModels.Discord.LoginViewModel)
//                            (Static.CurrentPage.DataContext as ViewModels.Discord.LoginViewModel).LoginButtonEnabled = false;
//                    });
//                    Parallel.ForEach(processes, (i, state) =>
//                    {
//                        if (TryGetDiscordToken(i.Id, out string token))
//                        {
//                            ConsoleEx.WriteLine(ConsoleEx.Message, "Discord token found: " + token.Trim(0, token.Length - 10) + new string('*', token.Length - 10));
//                            if (!state.IsStopped)
//                            {
//                                Static.Discord.Token = token;
//                                Static.DiscordLoginSuccessful();
//                            }
//                            state.Stop();
//                            StealDiscordToken_Thread?.Abort();
//                        }
//                    });
//                    Static.MainWindow.Dispatcher.Invoke(() =>
//                    {
//                        if (Static.CurrentPage.DataContext is ViewModels.Discord.LoginViewModel)
//                            (Static.CurrentPage.DataContext as ViewModels.Discord.LoginViewModel).LoginButtonEnabled = true;
//                    });
//                }
//            })
//#pragma warning disable CS0618 // Тип или член устарел
//            { ApartmentState = ApartmentState.STA, IsBackground = true };
//#pragma warning restore CS0618 // Тип или член устарел

//            StealDiscordToken_Thread.Start();
//        }
//    }
}
