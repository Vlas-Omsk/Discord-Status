using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PinkJson;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Resolvers;
using System.Drawing;
using WarfaceStatusGUI;
using System.Text.RegularExpressions;
using WEBLib;

namespace WarfaceStatus
{
    class Warface
    {
        public static void Init(MainWindow parent)
        {
            Parent = parent;
            try { Warface.Levels = new Json(File.ReadAllText("strings\\Levels.json")); c.i("Successfully load resource 'strings\\Levels.json'"); } catch (Exception ex) { c.w("Error loading resource 'strings\\Levels.json' " + ex.ToString()); }
            try { Warface.Screens_open = new Json(File.ReadAllText("strings\\Screens_open.json")); c.i("Successfully load resource 'strings\\Screens_open.json'"); } catch (Exception ex) { c.w("Error loading resource 'strings\\Screens_open.json' " + ex.ToString()); }
            try { Warface.Servers = new Json(File.ReadAllText("strings\\Servers.json")); c.i("Successfully load resource 'strings\\Servers.json'"); } catch (Exception ex) { c.w("Error loading resource 'strings\\Servers.json' " + ex.ToString()); }
            try { Warface.States = new Json(File.ReadAllText("strings\\States.json")); c.i("Successfully load resource 'strings\\States.json'"); } catch (Exception ex) { c.w("Error loading resource 'strings\\States.json' " + ex.ToString()); }
            try { Warface.Ranks = new Json(File.ReadAllText("strings\\Ranks.json")); c.i("Successfully load resource 'strings\\Ranks.json'"); } catch (Exception ex) { c.w("Error loading resource 'strings\\Ranks.json' " + ex.ToString()); }
            WarfaceGame.InitChecker();
        }

        public static Json Screens_open;
        public static Json Levels;
        public static Json Servers;
        public static Json States;
        public static Json Ranks;

        public static MainWindow Parent;
        /*public static Json Screens_open = new Json(new
        {
            //messagebox = "Сообщение",
            //lobby_ = "Лобби",

            _PREFIX = "[ScreenManager] Open screen:",
            
            lobby_preinvite = "Создание приглашения в команду",
            lobby_quick_play = "Поиск быстрой игры",
            lobby_pvp_game_room = "Находится в PVP комнате",
            lobby_pvp_rating = "Лобби PVP",
            lobby_inventory = "Склад",
            WeaponCustomizer = "Полигон",
            HUD = "Полигон",
            loading_pvp = "Загрузка PVP матча",
            loading = "Загрузка"
        });

        public static Json Levels = new Json(new
        {
            _PREFIX = "*LOADING: Level",

            bw_hideout = "Лобби",
            pvp_stm_blackwood = "База BlackWood / Штурм",
            pvp_tdm_oildepotv3 = "Нефтебаза / Командный бой",
            pvp_stm_blackgold_up = "Черное Золото / Штурм",
            pvp_tdm_ghost_town = "Район #4 / Командный бой"
        });

        public static Json Servers = new Json(new
        {
            _PREFIX = "[COnlineChannel] Selecting",

            MasterServer = "Ветераны {0}"
        });

        public static Json States = new Json(new
        {
            System_Quit = "Выход из игры"
        });*/
    }

    class WarfaceGame
    {
        public static Process GetGameProcess()
        {
            if (Process.GetProcessesByName("Game").Length != 0)
            {
                return Process.GetProcessesByName("Game")[0];
            }
            return null;
        }

        public static string[] GetGameCommandLine(Process game_process)
        {
            try //try read game process
            {
                return ProcessExtension.GetCommandLine(game_process.Id);
            }
            catch (Exception ex)
            {
                c.i("Error while read game process: " + ex.Message);

                var log_file_launcher_lines = GetLauncherLogFile();

                if (log_file_launcher_lines != null)
                {
                    for (var i = log_file_launcher_lines.Length - 1; i > 0; i--)
                        if (log_file_launcher_lines[i].IndexOf("RunGameProcess (id=0.1177") != -1)
                        {
                            try
                            {
                                return ProcessExtension.GetCommandLineParams(log_file_launcher_lines[i].Substring(log_file_launcher_lines[i].IndexOf(')') + 1).Trim());
                            }
                            catch (Exception exx) { c.i("Error while read launcher process: " + exx.Message); }
                        }
                }
                else
                {
                    c.i("Error: could'n read player id");
                }
                return new string[] { "!ERROR!" };
            }
        }

        public static string userprofile = ProcessExtension.GetConsoleCommandOut("cmd", "/c echo %USERPROFILE%").Trim();
        public static string[] GetLauncherLogFile()
        {
            string[] log_file_launcher_lines = null;
            if (Process.GetProcessesByName("GameCenter").Length != 0)
            {
                var launcher_process = Process.GetProcessesByName("GameCenter")[0];
                var launcher_process_cmd = ProcessExtension.GetCommandLine(launcher_process.Id);
                var launcher_directory = launcher_process_cmd[0].Substring(1, launcher_process_cmd[0].LastIndexOf('\\') - 1);
                log_file_launcher_lines = new FileInfo(launcher_directory + "\\main.log").ReadLines();
            }
            else if (File.Exists(userprofile + @"\AppData\Local\GameCenter\main.log"))
            {
                log_file_launcher_lines = new FileInfo(userprofile + @"\AppData\Local\GameCenter\main.log").ReadLines();
            }
            return log_file_launcher_lines;
        }

        public static System.Timers.Timer _GameChecker;
        public static void InitChecker()
        {
            _GameChecker = new System.Timers.Timer(5000);
            _GameChecker.Elapsed += _GameChecker_Elapsed;
            _GameChecker.AutoReset = true;
            _GameChecker.Enabled = true;
            _GameChecker.Start();
        }

        public static Config config = new Config();
        public static Process game_process;
        public static string[] game_cmd;
        public static string game_directory = "";
        public static FileInfo LogFile;
        private static bool GameLaunched = false;
        private static void _GameChecker_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (GetGameProcess() != null && GameLaunched == false)
            {
                if ((game_process = WarfaceGame.GetGameProcess()) != null)
                {
                    GameLaunched = true;
                    game_cmd = WarfaceGame.GetGameCommandLine(game_process);
                    c.i("Game process found id: " + game_process.Id + " Command line: " + string.Join(" ", game_cmd));
                    var prew = "";
                    foreach (var p in game_cmd)
                    {
                        switch (prew.Trim())
                        {
                            case "-region_id":
                                config.player.Region = p.Trim();
                                break;
                            case "-uid":
                                config.player.Uid = Convert.ToInt32(p);
                                break;
                            case "-onlineserver":
                                config.player.OnlineServer = p.Trim();
                                break;
                            default:
                                if (p.IndexOf("--shard_id") != -1)
                                    config.player.Shard = Convert.ToInt32(p.Split('=')[1]) + 1;
                                break;
                        }
                        prew = p;
                    }
                    config.player.InitPlayer();
                    game_directory = game_cmd[0].Substring(0, game_cmd[0].LastIndexOf('\\'));
                    c.i("Founded game directory: " + game_directory);
                    LogFile = new FileInfo(game_directory.Substring(0, game_directory.LastIndexOf('\\')) + "\\Game.log");
                    _last = LogFile.ReadText();
                    LogFile.OnChange += LogFile_OnChange;
                    LogFile.StartTimer();
                    c.i("Change event started");
                    Warface.Parent.AutoUpdateDiscord2(null, null);
                }
                
            }
            else if (GetGameProcess() == null && GameLaunched == true)
            {
                if (LogFile != null)
                    LogFile.StopTimer();
                GameLaunched = false;
            }
        }

        private static string _last;
        private static void LogFile_OnChange(ChangeEventArgs e)
        {
            var updLines = e.Sender.ReadText().SubStrDel(_last).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var IsScreenChanged = false;
            var PreviewMap = config.map;

            foreach (var line in updLines)
            {
                try
                {
                    if (line.IndexOf(Warface.Servers["_PREFIX"].Value) != -1)
                    {
                        c.wf("Server: " + line);
                        Regex regex = new Regex(@"\((.*?)\)");
                        MatchCollection matches = regex.Matches(line);
                        var val1 = matches[0].Value.Substring(1, matches[0].Value.Length - 2).Trim();
                        regex = new Regex(@"[ MasterServer]\s(.*?)\s");
                        matches = regex.Matches(line);
                        var val2 = matches[0].Value.Substring(1).Trim();
                        try
                        {
                            if (Warface.Servers[val1] != null)
                                config.server = string.Format(Warface.Servers[val1].Value, val2);
                        }
                        catch { }
                    }
                    else if (line.IndexOf(Warface.Screens_open["_PREFIX"].Value) != -1)
                    {
                        c.wf("Screen open: " + line);
                        Regex regex = new Regex(@"'(.*?)'");
                        MatchCollection matches = regex.Matches(line);
                        var val = matches[0].Value.Substring(1, matches[0].Value.Length - 2);
                        try
                        {
                            if (Warface.Screens_open[val] != null)
                                config.screen = Warface.Screens_open[val].Value;
                        }
                        catch { }
                        IsScreenChanged = true;
                    }
                    else if (line.IndexOf(Warface.Levels["_PREFIX"].Value) != -1)
                    {
                        c.wf("Level: " + line);
                        Regex regex = new Regex(@"[ Level]\s(.*?)\s");
                        MatchCollection matches = regex.Matches(line);
                        var val = matches[0].Value.Substring(1).Trim();
                        try
                        {
                            if (Warface.Levels[val] != null)
                                config.map = Warface.Levels[val].Value;
                        }
                        catch { }
                        config.screen = string.Format(Warface.States["InGame"].Value, val.Substring(0, val.IndexOf('/')).ToUpper());
                        IsScreenChanged = true;
                    }
                    //States optional
                    if (line.IndexOf("MS is insufficient to perform the desired action(s), trying to switch to another MS in search of a better life") != -1)
                    {
                        c.wf("Screen: " + line);
                        config.screen = Warface.States["Lobby_PVE"].Value;
                        IsScreenChanged = true;
                    }
                    if (line.IndexOf("System:Quit") != -1)
                    {
                        c.wf("Status: " + line);
                        config.screen = Warface.States["System_Quit"].Value;
                        IsScreenChanged = true;
                        Warface.Parent.clearStatus();
                    }
                }
                catch (Exception ex)
                {
                    c.u("StringParser", "Error: " + ex.ToString());
                }
            }
            //Console.Title = config.ToString();

            _last = e.Sender.ReadText();

            if (IsScreenChanged)
            {
                var dt = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks).TotalMilliseconds;
                config.since = Convert.ToInt64(dt);
            }

            if (PreviewMap != config.map)
            {
                var dtnow = DateTime.Now;
                Warface.Parent.matches.Add(PreviewMap, config.sinceMap.ToString("dd.MM.yyyy HH:mm:ss"), dtnow.ToString("dd.MM.yyyy HH:mm:ss"));
                config.sinceMap = dtnow;
            }

            Warface.Parent.AutoUpdateDiscord2(null, null);
        }
    }

    public class Config
    {
        public WarfaceApi.Player player = new WarfaceApi.Player();
        public string server = "-";
        public string screen = "-";
        public long since = 0;
        public DateTime sinceMap = DateTime.Now;
        public string map = "-";
    }

    public class WarfaceApi
    {
        const string MailUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Downloader/15810 MyComGameCenter/1581 Safari/537.36";

        public enum Server : int
        {
            Альфа = 1,
            Браво = 2,
            Чарли = 3
        }

        string[] links = new string[]
        {
            "https://store.my.games/gamecenter/game/?id=0.1177&gcgameid=0.1177&uid=668158617&clientstate=installed",
            "https://ruwf.my.games/dynamic/top/?a=get_ladder_info_gamecenter&server=1&category=1&userid=668158617",
            "https://wf.cdn.gmru.net/static/wf.mail.ru/gamecenter/right_new/img/ranks_all.png"
        };

        public class Player
        {
            public Player() { }

            public void InitPlayer()
            {
                var link = "https://ruwf.my.games/dynamic/top/?a=get_ladder_info_gamecenter&server=" + Shard + "&category=1&userid=" + Uid;
                var inf = WEB.Get(link, MailUserAgent);

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
                                PlayerInfo.Rank = Convert.ToInt32(val.Substring(val.LastIndexOf('_') + 1));
                                PlayerInfo.RankName = Warface.Ranks[PlayerInfo.Rank].Value;
                            }
                            if (n.Attributes.Count != 0 && val.IndexOf("nickname") != -1)
                                PlayerInfo.Nickname = n.InnerText;
                        }
                    }
                }

                PlayerInfo.Server = ((Server)Shard).ToString();
                /*var rect = new Rectangle(0, (PlayerInfo.Rank - 1)*32, 32, 32);

                System.Net.HttpWebRequest request =
                    (System.Net.HttpWebRequest)System.Net.WebRequest.Create(
                        //"https://wf.cdn.gmru.net/static/wf.mail.ru/gamecenter/right_new/img/ranks_all.png" MINI
                        "https://wfts.su/ranks/ranks_all.png");
                request.UserAgent = MailUserAgent;
                System.Net.WebResponse response = request.GetResponse();
                Stream responseStream =
                    response.GetResponseStream();
                Bitmap ranks_all = new Bitmap(responseStream);

                PlayerInfo.Rank_icon = new Bitmap(ranks_all).Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);*/
            }

            public PlayerInfo PlayerInfo = new PlayerInfo();
            public int Uid;
            public int Shard;
            public string Region;
            public string OnlineServer;

            public override string ToString()
            {
                return PlayerInfo.ToString();
            }
        }

        public class PlayerInfo
        {
            public PlayerInfo() { }

            public string Nickname;
            public string Server;
            public int Rank;
            public string RankName;

            public override string ToString()
            {
                return "[ " + Nickname + " " + Server + " " + Rank + " ]";
            }
        }
    }
}
