using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using DiscordStatusGUI.Extensions;
using WEBLib;
using CookieGrabber;

namespace DiscordStatusGUI.Libs
{
    public class SteamApi
    {
        public const string SteamUserAgent = "Mozilla/5.0 (Windows; U; Windows NT 10.0; en-US; Valve Steam Client/default/1613176728; ) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36";
        public const string DataFolder = "Data\\Steam\\";

        static readonly string UserDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Steam\\userdata";

        public static SteamProfile CurrentSteamProfile = new SteamProfile();
        public static Thread UpdateProfileThread = new Thread(UpdateProfileThreadHandler);
        static string LastGameName = null;
        static int ErrorCount = 0;

        public static void UpdateProfileThreadHandler()
        {
            while (true)
            {
                if (CurrentSteamProfile.Refresh())
                    ErrorCount = 0;
                else
                    ErrorCount++;

                if (ErrorCount == 6)
                    CurrentSteamProfile.Clear();

                if (LastGameName == null && CurrentSteamProfile.GameName != null)
                    OnGameProcessStateChanged?.Invoke(true);
                else if (LastGameName != null && CurrentSteamProfile.GameName == null)
                    OnGameProcessStateChanged?.Invoke(false);

                LastGameName = CurrentSteamProfile.GameName;
                Thread.Sleep(CurrentSteamProfile.GameName == null ? 10000 : 8000);
            }
        }

        public static void Init()
        {
            if (string.IsNullOrEmpty(CurrentSteamProfile.ID))
            {
                if ((TryGetSteamIDs(out string[] ids) && string.IsNullOrEmpty(CurrentSteamProfile.SteamLoginSecure)) || string.IsNullOrEmpty(CurrentSteamProfile.SteamLoginSecure))
                {
                    CurrentSteamProfile.ResearchSteamLoginSecure();
                }
                CurrentSteamProfile.ID = ids[0];
            }
            UpdateProfileThread.IsBackground = true;
            UpdateProfileThread.Start();
        }

        public static bool TryGetSteamIDs(out string[] ids)
        {
            if (Directory.Exists(UserDataFolder))
            {
                ids = new DirectoryInfo(UserDataFolder).GetDirectories()
                    .OrderByDescending(di => di.LastWriteTime).Select(di => di.Name).ToArray();
                return ids.Length == 0 ? false : true;
            }
            ids = null;
            return false;
        }

        public delegate void OnGameProcessStateChangedEventHandler(bool opened);
        public static event OnGameProcessStateChangedEventHandler OnGameProcessStateChanged;
    }

    public class SteamProfile
    {
        public const string Miniprofile = "https://steamcommunity.com/miniprofile/";

        private string _ImageUrl;
        private string _Nickname;
        private string _Status;
        private SteamProfileDetail[] _Details;
        private string _GameLogoUrl;
        private string _GameState;
        private string _GameName;
        private string _RichPresence;

        public string ImageUrl
        {
            get => _ImageUrl;
            set
            {
                InvokeOnPropertyChanged(_ImageUrl != value, nameof(ImageUrl));
                _ImageUrl = value;
            }
        }
        public string Nickname
        {
            get => _Nickname;
            set
            {
                InvokeOnPropertyChanged(_Nickname != value, nameof(Nickname));
                _Nickname = value;
            }
        }
        public string Status
        {
            get => _Status;
            set
            {
                InvokeOnPropertyChanged(_Status != value, nameof(Status));
                _Status = value;
            }
        }
        public string GameLogoUrl
        {
            get => _GameLogoUrl;
            set
            {
                InvokeOnPropertyChanged(_GameLogoUrl != value, nameof(GameLogoUrl));
                _GameLogoUrl = value;
            }
        }
        public string GameState
        {
            get => _GameState;
            set
            {
                InvokeOnPropertyChanged(_GameState != value, nameof(GameState));
                _GameState = value;
            }
        }
        public string GameName
        {
            get => _GameName;
            set
            {
                InvokeOnPropertyChanged(_GameName != value, nameof(GameName));
                _GameName = value;
            }
        }
        public string RichPresence
        {
            get => _RichPresence;
            set
            {
                InvokeOnPropertyChanged(_RichPresence != value, nameof(RichPresence));
                _RichPresence = value;
            }
        }
        public SteamProfileDetail[] Details
        {
            get => _Details;
            set
            {
                //InvokeOnPropertyChanged(_Details != value);
                _Details = value;
            }
        }
        public string ID { get; set; }

        public string Cookies => "steamLoginSecure=" + SteamLoginSecure;

        private string _SteamLoginSecure = "";
        public string SteamLoginSecure
        {
            get => _SteamLoginSecure;
            set
            {
                _SteamLoginSecure = value;
                Static.InvokeAsync(PropertyChanged, new PropertyChangedEventArgs("SteamLoginSecure"), this);
            }
        }


        public SteamProfile()
        {
        }

        public SteamProfile(string id)
        {
            ID = id;
            Refresh();
        }

        public void Clear()
        {
            ImageUrl = null;
            Status = null;
            Details = null;
            GameLogoUrl = null;
            GameState = null;
            GameName = null;
            RichPresence = null;

            InvokeOnPropertyChanged(false, null, true);
        }

        public bool Refresh()
        {
            var data = WEB.Post(Miniprofile + ID, new string[] { 
                "User-Agent: " + SteamApi.SteamUserAgent, 
                "accept-language: " + Locales.Lang.CurrentWebLanguage,
                "Cookie: " + Cookies}, null, "GET");
            if (data == null) { return false; }
            data = Regex.Replace(data, @"\<img.*?\>", new MatchEvaluator((obj) => obj.Value + "</img>"));

            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(data);
            }
            catch { return false; }
            foreach (XmlNode div in doc.GetElementsByTagName("div"))
            {
                if (div.Attributes["class"].Value.Contains("miniprofile_playersection"))
                    foreach (XmlNode cdiv in div.ChildNodes)
                    {
                        if (cdiv is XmlComment)
                            continue;
                        else if (cdiv.Attributes["class"].Value.Contains("playersection_avatar"))
                            ImageUrl = cdiv.FirstChild.Attributes["srcset"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).OrderByDescending(str => str[1]).First()[0];
                        else if (cdiv.Attributes["class"].Value.Contains("player_content"))
                            foreach (XmlNode pdiv in cdiv.ChildNodes)
                            {
                                if (pdiv.Attributes["class"].Value.Contains("persona"))
                                    Nickname = pdiv.InnerText;
                                else if (pdiv.Attributes["class"].Value.Contains("friend_status"))
                                    Status = pdiv.InnerText;
                            }
                    }
                else if (div.Attributes["class"].Value.Contains("miniprofile_gamesection"))
                    foreach (XmlNode cdiv in div.ChildNodes)
                    {
                        if (cdiv.Attributes["class"].Value.Contains("game_logo"))
                            GameLogoUrl = cdiv.Attributes["src"].Value;
                        else if (cdiv.Attributes["class"].Value.Contains("game_details"))
                            foreach (XmlNode span in cdiv.ChildNodes)
                            {
                                if (span.Attributes["class"].Value.Contains("game_state"))
                                    GameState = span.InnerText;
                                else if (span.Attributes["class"].Value.Contains("game_name"))
                                    GameName = span.InnerText;
                                else if (span.Attributes["class"].Value.Contains("rich_presence"))
                                    RichPresence = span.InnerText;
                            }
                    }
                else if (div.Attributes["class"].Value.Contains("miniprofile_detailssection"))
                {
                    var details = new List<SteamProfileDetail>();
                    foreach (XmlNode cdiv in div.ChildNodes)
                        if (cdiv.Attributes["class"].Value.Contains("miniprofile_featuredcontainer"))
                        {
                            var detail = new SteamProfileDetail();
                            foreach (XmlNode pdiv in cdiv.ChildNodes)
                            {
                                if (pdiv.Attributes["class"].Value.Contains("badge_icon"))
                                    detail.ImageUrl = pdiv.Attributes["src"].Value;
                                else if (pdiv.Attributes["class"].Value.Contains("description"))
                                    foreach (XmlNode zdiv in pdiv.ChildNodes)
                                    {
                                        if (zdiv.Attributes["class"].Value.Contains("name"))
                                            detail.Name = zdiv.InnerText;
                                        else if (zdiv.Attributes["class"].Value.Contains("xp"))
                                            detail.Xp = zdiv.InnerText;
                                    }
                                else if (pdiv.Attributes["class"].Value.Contains("friendPlayerLevel"))
                                    foreach (XmlNode span in pdiv.ChildNodes)
                                        if (span.Attributes["class"].Value.Contains("friendPlayerLevelNum") && int.TryParse(span.InnerText, out int level))
                                            detail.Level = level;
                            }
                            details.Add(detail);
                        }
                    Details = details.ToArray();
                }
            }

            //XmlNode root = doc.GetByClassName("miniprofile_container")[0];
            //XmlNode[] nodes;

            //if ((nodes = root.GetByClassName("miniprofile_playersection")).Length != 0)
            //{
            //    XmlNode[] path1;
            //    if ((path1 = nodes[0].GetByClassName("playersection_avatar")).Length != 0)
            //        ImageUrl = path1[0].FirstChild.Attributes["srcset"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).OrderByDescending(str => str[1]).First()[0];
            //    else ImageUrl = null;
            //    if ((path1 = nodes[0].GetByClassName("player_content")).Length != 0)
            //    {
            //        XmlNode[] path2;
            //        if ((path2 = path1[0].GetByClassName("persona")).Length != 0)
            //            Nickname = path2[0].InnerText;
            //        else Nickname = null;
            //        if ((path2 = path1[0].GetByClassName("persona")).Length != 0)
            //            Status = path2[0].InnerText;
            //        else Status = null;
            //    }
            //    else
            //    {
            //        Nickname = null;
            //        Status = null;
            //    }
            //}
            //else
            //{
            //    ImageUrl = null;
            //    Nickname = null;
            //    Status = null;
            //}

            InvokeOnPropertyChanged(false, null, true);
            return true;
        }

        List<string> notremovedProperties = new List<string>();
        List<string> changedProperties = new List<string>();
        string[] clearProperties = { nameof(ImageUrl), nameof(Status), nameof(GameLogoUrl), nameof(GameState), nameof(GameName), nameof(RichPresence) };
        private void InvokeOnPropertyChanged(bool changed, string property, bool invoke = false)
        {
            notremovedProperties.Add(property);
            if (changed)
                changedProperties.Add(property);
            if (invoke && notremovedProperties.Count != 0)
            {
                foreach (var clrProp in clearProperties)
                    if (!notremovedProperties.Contains(clrProp))
                        typeof(SteamProfile).GetProperty(clrProp).SetValue(this, null);
                notremovedProperties.Clear();
            }
            if (invoke && changedProperties.Count != 0)
            {
                changedProperties.Clear();
                Static.InvokeAsync(PropertyChanged, new PropertyChangedEventArgs(null), this);
            }
        }

        public void ResearchSteamLoginSecure()
        {
            SteamLoginSecure = CookieTool.GetNewestCookieByKey("steamLoginSecure")?.Value;
        }

        public void ResearchSteamLoginSecure(System.Windows.Controls.WebBrowser webbrowser)
        {
            var cookies = WebBrowserTools.GetUriCookies("https://steampowered.com")["steamLoginSecure"];
            if (cookies != null)
            {
                SteamLoginSecure = cookies.Value;
                return;
            }
            cookies = WebBrowserTools.GetUriCookies("https://store.steampowered.com")["steamLoginSecure"];
            if (cookies != null)
            {
                SteamLoginSecure = cookies.Value;
                return;
            }
        }

        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;
    }



    public class SteamProfileDetail
    {
        public string ImageUrl { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public string Xp { get; set; }
    }
}
