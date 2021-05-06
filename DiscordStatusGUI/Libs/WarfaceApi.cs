using DiscordStatusGUI.Extensions;
using PinkJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using WEBLib;

namespace DiscordStatusGUI.Libs
{
    public class WarfaceApi
    {
        public const string MailUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Downloader/15810 MyComGameCenter/1581 Safari/537.36";
        public const string DataFolder = "Data\\WarfaceRU\\";

        public static Json Screens_open;
        public static Json Levels;
        public static Json Servers;
        public static Json States;
        public static Json Ranks;
        public static bool Screens_open_UseNativeNames = true, Levels_UseNativeNames = true;

        public static bool FastGameClientClose;

        public static Process GameProcess;
        public static string[] GameCommandLine;
        public static string GameDirectory;
        public static WarfacePlayer CurrentPlayer = new WarfacePlayer();
        public static WarfaceGameState CurrentGameState = new WarfaceGameState();

        private static string _UserProfile = ProcessEx.GetOutput("cmd", "/c echo %USERPROFILE%").Trim();
        private static FileInfoEx _GameLogFile = null;
        private static string _LastGameLogFileText;

        public delegate void OnGameProcessStateChangedEventHandler(bool opened);
        public static event OnGameProcessStateChangedEventHandler OnGameProcessStateChanged;

        public static void Init()
        {
            if (TryLoadResource("Levels.json", out Levels))
                Levels_UseNativeNames = Levels["_UseNativeNames"].Get<bool>();
            if (TryLoadResource("Screens_open.json", out Screens_open))
                Screens_open_UseNativeNames = Screens_open["_UseNativeNames"].Get<bool>();
            TryLoadResource("Servers.json", out Servers);
            TryLoadResource("States.json", out States);
            TryLoadResource("Ranks.json", out Ranks);

            ProcessEx.OnProcessOpened += ProcessEx_OnProcessOpened;
            ProcessEx.OnProcessClosed += ProcessEx_OnProcessClosed;

            Static.InitializationSteps.IsWarfaceApiInitialized = true;
        }

        private static void ProcessEx_OnProcessClosed(Processes processes)
        {
            if (processes.GetProcessesByNames("Game", "GameDX11").Count == 0)
                return;

            ConsoleEx.WriteLine(ConsoleEx.Warning, "GameLogFile.OnTextChanged Stopped");
            _GameLogFile?.StopTimer();

            OnGameProcessStateChanged?.Invoke(false);
        }

        private static void ProcessEx_OnProcessOpened(Processes processes)
        {
            if (GameProcess == null || GameProcess.HasExited)
            {
                GameProcess = processes.GetProcessesByNames("Game", "GameDX11").FirstOrDefault();

                if (GameProcess != null && !GameProcess.HasExited)
                {
                    GameCommandLine = GetGameCommandLine(GameProcess);
                    ConsoleEx.WriteLine(ConsoleEx.Info, "Game process found id: " + GameProcess.Id + " Command line: " + string.Join(" ", GameCommandLine));

                    var prew = "";
                    foreach (var p in GameCommandLine)
                    {
                        switch (prew.Trim())
                        {
                            case "-region_id":
                                CurrentPlayer.Region = p.Trim();
                                break;
                            case "-uid":
                                CurrentPlayer.Uid = Convert.ToInt32(p);
                                break;
                            case "-onlineserver":
                                CurrentPlayer.OnlineServer = p.Trim();
                                break;
                            default:
                                if (p.IndexOf("--shard_id") != -1)
                                    CurrentPlayer.Shard = Convert.ToInt32(p.Split('=')[1]) + 1;
                                break;
                        }
                        prew = p;
                    }

                    CurrentPlayer.UpdatePlayerInfo();

                    GameDirectory = GameCommandLine[0].Substring(0, GameCommandLine[0].LastIndexOf('\\'));
                    ConsoleEx.WriteLine(ConsoleEx.Warning, "Founded game directory: " + GameDirectory);

                    _GameLogFile = new FileInfoEx(GameDirectory.Substring(0, GameDirectory.LastIndexOf('\\')) + "\\Game.log");
                    _LastGameLogFileText = _GameLogFile.SafeReadText();
                    _GameLogFile.TextChanged += _GameLogFile_OnTextChanged;
                    _GameLogFile.StartTimer();
                    ConsoleEx.WriteLine(ConsoleEx.Warning, "GameLogFile.OnTextChanged Started");

                    OnGameProcessStateChanged?.Invoke(true);
                }
            }
        }

        private static void _GameLogFile_OnTextChanged(object sender, TextChangedEventArgsEx e)
        {
            var updLines = (sender as FileInfoEx).SafeReadText().SubStrDel(_LastGameLogFileText).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var IsScreenChanged = false;
            var PreviewMap = CurrentGameState.Map;

            foreach (var line in updLines)
            {
                try
                {
                    if (line.IndexOf(Servers["_PREFIX"].Value.ToString()) != -1)
                    {
                        ConsoleEx.WriteLine(ConsoleEx.WarfaceApi, "Server: " + line);
                        Regex regex = new Regex(@"\((.*?)\)");
                        MatchCollection matches = regex.Matches(line);
                        var val1 = matches[0].Value.Substring(1, matches[0].Value.Length - 2).Trim();
                        regex = new Regex(@"[ MasterServer]\s(.*?)\s");
                        matches = regex.Matches(line);
                        var val2 = matches[0].Value.Substring(1).Trim();
                        try
                        {
                            if (Servers[val1] != null)
                                CurrentGameState.Server = string.Format(Servers[val1].Value.ToString(), val2);
                        }
                        catch { }
                    }
                    else if (line.IndexOf(Screens_open["_PREFIX"].Value.ToString()) != -1)
                    {
                        ConsoleEx.WriteLine(ConsoleEx.WarfaceApi, "Screen open: " + line);
                        Regex regex = new Regex(@"'(.*?)'");
                        MatchCollection matches = regex.Matches(line);
                        var val = matches[0].Value.Substring(1, matches[0].Value.Length - 2);
                        try
                        {
                            if (Screens_open[val] != null)
                                CurrentGameState.Screen = Screens_open[val].Value.ToString();
                            else if (Screens_open_UseNativeNames)
                                CurrentGameState.Screen = val;
                        }
                        catch { }
                        IsScreenChanged = true;
                    }
                    else if (line.IndexOf(Levels["_PREFIX"].Value.ToString()) != -1)
                    {
                        ConsoleEx.WriteLine(ConsoleEx.WarfaceApi, "Level: " + line);
                        Regex regex = new Regex(@"[ Level]\s(.*?)\s");
                        MatchCollection matches = regex.Matches(line);
                        var val = matches[0].Value.Substring(1).Trim();
                        try
                        {
                            if (Levels[val] != null)
                                CurrentGameState.Map = Levels[val].Value.ToString();
                            else if (Levels_UseNativeNames)
                                CurrentGameState.Map = val;
                        }
                        catch { }
                        CurrentGameState.Screen = string.Format(States["InGame"].Value.ToString(), val.Substring(0, val.IndexOf('/')).ToUpper());
                        IsScreenChanged = true;
                    }

                    //States optional
                    if (line.IndexOf("MS is insufficient to perform the desired action(s), trying to switch to another MS in search of a better life") != -1)
                    {
                        ConsoleEx.WriteLine(ConsoleEx.WarfaceApi, "Screen: " + line);
                        CurrentGameState.Screen = States["Lobby_PVE"].Value.ToString();
                        IsScreenChanged = true;
                    }
                    if (line.IndexOf("System:Quit") != -1)
                    {
                        ConsoleEx.WriteLine(ConsoleEx.WarfaceApi, "Status: " + line);
                        CurrentGameState.Screen = States["System_Quit"].Value.ToString();
                        IsScreenChanged = true;
                    }
                    if (FastGameClientClose)
                    if ((line.Contains("result 2") && line.Contains("@messagebox_quit_question")) ||
                        (line.Contains("result 0") && line.Contains("@ui_cryonline_connection_lost")))
                    {
                        try
                        {
                            GameProcess.Kill();
                            ConsoleEx.WriteLine(ConsoleEx.WarfaceApi, "Fast client closing enabled, process " + GameProcess.Id + " killed");
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleEx.WriteLine(ConsoleEx.WarfaceStringParser, "Error: " + ex.ToString());
                }
            }
            //Console.Title = config.ToString();

            _LastGameLogFileText = (sender as FileInfoEx).SafeReadText();

            if (IsScreenChanged)
            {
                var dt = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks).TotalMilliseconds;
                CurrentGameState.Since = Convert.ToInt64(dt);
            }

            if (PreviewMap != CurrentGameState.Map)
            {
                var dtnow = DateTime.Now;
                //Warface.Parent.matches.Add(PreviewMap, config.sinceMap.ToString("dd.MM.yyyy HH:mm:ss"), dtnow.ToString("dd.MM.yyyy HH:mm:ss"));
                CurrentGameState.SinceMap = dtnow;
            }
        }


        public static string[] GetGameCommandLine(Process game_process)
        {
            if (TryGetStandartCMDLine(game_process.Id, out var args))
                return args;
            else
            {
                ConsoleEx.WriteLine(ConsoleEx.Warning, "Error while read game process");

                if (TryGetLauncherLogFile(out var log_file_launcher_lines))
                {
                    for (var i = log_file_launcher_lines.Length - 1; i > 0; i--)
                        if (log_file_launcher_lines[i].IndexOf("RunGameProcess (id=0.1177") != -1)
                        {
                            return ProcessEx.SplitParams(log_file_launcher_lines[i].Substring(log_file_launcher_lines[i].IndexOf(')') + 1).Trim());
                        }
                    ConsoleEx.WriteLine(ConsoleEx.Warning, "Error: could'n read player id");
                }
                else
                {
                    ConsoleEx.WriteLine(ConsoleEx.Warning, "Error: could'n read player id");
                }
                return new string[] { "!ERROR!" };
            }
        }

        private static bool TryGetLauncherLogFile(out string[] lines)
        {
            var processes = Process.GetProcessesByName("GameCenter");
            if (processes.Length != 0)
            {
                var launcher_process = processes[0];
                if (TryGetStandartCMDLine(launcher_process.Id, out var launcher_process_cmd))
                {
                    var launcher_directory = launcher_process_cmd[0].Substring(1, launcher_process_cmd[0].LastIndexOf('\\') - 1);
                    lines = new FileInfoEx(launcher_directory + "\\main.log").SafeReadLines();
                    return true;
                }
                else
                {
                    lines = null;
                    return false;
                }
            }
            else if (File.Exists(_UserProfile + @"\AppData\Local\GameCenter\main.log"))
            {
                lines = new FileInfoEx(_UserProfile + @"\AppData\Local\GameCenter\main.log").SafeReadLines();
                return true;
            }
            lines = null;
            return false;
        }

        private static bool TryGetStandartCMDLine(int pid, out string[] args)
        {
            if (!string.IsNullOrEmpty(Process.GetProcessById(pid).StartInfo.Arguments))
            {
                args = ProcessEx.SplitParams(Process.GetProcessById(pid).StartInfo.Arguments);
                return true;
            }
            else
            {
                var tmp = ProcessEx.GetProcessCommandLine(pid, out long ntstatus);
                if (ntstatus != 0)
                {
                    ConsoleEx.WriteLine(ConsoleEx.Warning, "GetProcessCommandLine error: " + ntstatus);
                    goto end;
                }

                args = ProcessEx.SplitParams(tmp);
                ConsoleEx.WriteLine(ConsoleEx.Info, "Yeah. (CPP) GetProcessCommandLine working");
                return true;
            }
            //else if (File.Exists("util\\GetProcessCommandLine32.exe"))
            //{
            //    var m = ProcessEx.GetOutput("util\\GetProcessCommandLine32", pid.ToString());
            //    if (m.StartsWith("Err"))
            //    {
            //        var err = Math.Abs(Convert.ToInt32(m.Substring(4, 2))) - 10;
            //        ConsoleEx.WriteLine(ConsoleEx.Warning, new JsonObjectArray(File.ReadAllText("util\\GetProcessCommandLineErrors.json"))[err].ToString());
            //        args = null;
            //        return false;
            //    }
            //    else
            //    {
            //        args = SplitCommandLineParams(m);
            //        return true;
            //    }
            //}
            end:
            args = null;
            return false;
        }

        private static bool TryLoadResource(string path, out Json json)
        {
            json = null;

            path = DataFolder + path;
            if (!File.Exists(path))
            {
                ConsoleEx.WriteLine(ConsoleEx.Warning, "Resource not found '" + path + "'");
                return false;
            }
            string data = null;
            if (string.IsNullOrEmpty(data = File.ReadAllText(path)))
            {
                ConsoleEx.WriteLine(ConsoleEx.Warning, "Empty resource '" + path + "'");
                return false;
            }
            try
            {
                json = new Json(data);
                ConsoleEx.WriteLine(ConsoleEx.Info, "Successfully load resource '" + path + "'");
                return true;
            } catch
            {
                ConsoleEx.WriteLine(ConsoleEx.Warning, "Error loading resource '" + path + "'");
                return false;
            }
        }
    }

    public class WarfaceGameState
    {
        private string _Server = "-";
        private string _Screen = "-";
        private long _Since = 0;
        private string _Map = "-";
        private DateTime _SinceMap = DateTime.Now;

        public string Server
        {
            get => _Server;
            set
            {
                _Server = value;
                Static.InvokeAsync(PropertyChanged, new PropertyChangedEventArgs("Server"), this);
            }
        }
        public string Screen
        {
            get => _Screen;
            set
            {
                _Screen = value;
                Static.InvokeAsync(PropertyChanged, new PropertyChangedEventArgs("Screen"), this);
            }
        }
        public long Since
        {
            get => _Since;
            set
            {
                _Since = value;
                Static.InvokeAsync(PropertyChanged, new PropertyChangedEventArgs("Since"), this);
            }
        }
        public string Map
        {
            get => _Map;
            set
            {
                _Map = value;
                Static.InvokeAsync(PropertyChanged, new PropertyChangedEventArgs("Map"), this);
            }
        }
        public DateTime SinceMap
        {
            get => _SinceMap;
            set
            {
                _SinceMap = value;
                Static.InvokeAsync(PropertyChanged, new PropertyChangedEventArgs("SinceMap"), this);
            }
        }

        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;
    }

    public class WarfacePlayer
    {
        public int Rank { get; private set; }
        public string RankName { get; private set; }
        public string Nickname { get; private set; }

        private int _Uid;
        private int _Shard;
        private string _Region;
        private string _OnlineServer;

        public int Uid
        {
            get => _Uid;
            set
            {
                _Uid = value;
                OnUserInfoChanged?.Invoke("Uid");
            }
        }
        public int Shard
        {
            get => _Shard;
            set
            {
                _Shard = value;
                OnUserInfoChanged?.Invoke("Shard");
            }
        }
        public string Region
        {
            get => _Region;
            set
            {
                _Region = value;
                OnUserInfoChanged?.Invoke("Region");
            }
        }
        public string OnlineServer
        {
            get => _OnlineServer;
            set
            {
                _OnlineServer = value;
                OnUserInfoChanged?.Invoke("OnlineServer");
            }
        }
        public Servers Server { get => (Servers)Shard; }

        public async void UpdatePlayerInfo()
        {
            await Task.Run(() =>
            {
                var link = "https://ruwf.my.games/dynamic/top/?a=get_ladder_info_gamecenter&server=" + Shard + "&category=1&userid=" + Uid;
                var inf = WEB.Get(link, WarfaceApi.MailUserAgent);

                inf = "<body>" + inf.Replace("&ndash;", "-") + "</body>";

                var doc = new XmlDocument();
                doc.LoadXml(inf);
                foreach (XmlNode li in doc.GetElementsByTagName("li"))
                {
                    if (li.Attributes.Count != 0 && li.Attributes.GetNamedItem("class").Value == "selected")
                    {
                        foreach (XmlNode n in li.ChildNodes)
                        {
                            var val = n.Attributes.GetNamedItem("class").Value;
                            if (n.Attributes.Count != 0 && val.IndexOf("rank_icon") != -1)
                            {
                                Rank = Convert.ToInt32(val.Substring(val.LastIndexOf('_') + 1));
                                if (WarfaceApi.Ranks != null)
                                    RankName = (WarfaceApi.Ranks[Rank] as JsonObject).Value.ToString();
                            }
                            if (n.Attributes.Count != 0 && val.IndexOf("nickname") != -1)
                                Nickname = n.InnerText;
                        }
                    }
                }

                Static.InvokeAsync(PlayerInfoChanged, EventArgs.Empty, this);
            });
        }

        public event EventHandler<EventArgs> PlayerInfoChanged;

        public delegate void OnUserInfoChangedEventHandler(string property);
        public event OnUserInfoChangedEventHandler OnUserInfoChanged;

        public override string ToString()
        {
            return "[ " + Nickname + " " + Server + " " + Rank + " ]";
        }
    }

    public enum Servers : int
    {
        Альфа = 1,
        Браво = 2,
        Чарли = 3
    }
}
