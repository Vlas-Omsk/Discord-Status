using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PinkJson;
using System.Windows.Media;
using DiscordStatusGUI.Extensions;

namespace DiscordStatusGUI.Libs.DiscordApi
{
    public class PrivateChannels
    {
        public List<PrivateChannel> Channels = new List<PrivateChannel>();
        
        public PrivateChannels()
        {
        }

        public void AddRange(JsonArray obj, UsersCache cache)
        {
            foreach (var ch in obj)
            {
                var json = ch.Get<Json>();
                var lastMsgId = json["last_message_id"].Get<double>();
                json["last_message_id"].Value = lastMsgId;
                var i = 0;
                for (; i < Channels.Count; i++)
                    if (lastMsgId > Channels[i].LastMsgId)
                        break;

                Channels.Insert(i, new PrivateChannel(json, cache));
            }
        }
    }

    public class PrivateChannel
    {
        public int Type { get; set; }
        string _Name;
        public string Name
        {
            get
            {
                if (_Name != null)
                    return _Name;

                if (ID == null)
                    _Name = $"channel({ID})";
                else if (Type == 1)
                    _Name = UsersCache.GetUser(RecipientIDs[0]).UserName;
                else if (Type == 3)
                {
                    if (RecipientIDs.Length == 0)
                        _Name = "No name";
                    else
                    {
                        _Name = "";
                        for (var i = 0; i < RecipientIDs.Length; i++)
                        {
                            _Name += UsersCache.GetUser(RecipientIDs[i]).UserName;
                            if (i != RecipientIDs.Length - 1)
                                _Name += ", ";
                        }
                    }
                }
                return _Name;
            }
            set => _Name = value;
        }
        string _IconId;
        public string IconId
        {
            get => _IconId;
            set => _IconId = value;
        }
        ImageSource _Icon;
        public ImageSource Icon
        {
            get
            {
                if (_Icon != null)
                    return _Icon;
                else if (Type == 1)
                    return _Icon = UsersCache.GetUser(RecipientIDs[0]).Avatar;
                else if (_IconId != null && Type == 3) {
                    _Icon = BitmapEx.ToImageSource(Discord.GetImageByUrl($"https://cdn.discordapp.com/channel-icons/{ID}/{IconId}.png"));
                    _Icon.Freeze();
                    return _Icon;
                }
                else return DefaultGroupIcon;
            }
        }
        public string ID { get; set; }
        public string OwnerID { get; set; }
        public string[] RecipientIDs { get; set; }
        public double LastMsgId { get; set; }
        public string RecipientCount
        {
            get
            {
                if (RecipientIDs == null)
                    return "";
                var str = (RecipientIDs.Length + 1).ToString();
                var result = str + " участник";
                switch (str.Last())
                {
                    case '1':
                        break;
                    case '2':
                    case '3':
                    case '4':
                        result += "a";
                        break;
                    case '0':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        result += "ов";
                        break;
                }
                return result;
            }
        }
        public bool IsGroup
        {
            get => Type == 3;
        }
        public bool IsOnline
        {
            get => IsStatusEq(UserStatus.online);
        }
        public bool IsIdle
        {
            get => IsStatusEq(UserStatus.idle);
        }
        public bool IsDnd
        {
            get => IsStatusEq(UserStatus.dnd);
        }
        public bool IsInvisible
        {
            get => IsStatusEq(UserStatus.invisible);
        }
        public bool IsOffline
        {
            get => IsStatusEq(UserStatus.offline);
        }

        public UsersCache UsersCache;
        ImageSource DefaultGroupIcon = BitmapEx.ToImageSource(Discord.GetImageByUrl("https://discord.com/assets/1531b79c2f2927945582023e1edaaa11.png"));

        public PrivateChannel()
        {
            DefaultGroupIcon.Freeze();
        }

        public PrivateChannel(UsersCache cache)
        {
            DefaultGroupIcon.Freeze();
            UsersCache = cache;
        }

        public PrivateChannel(Json json, UsersCache cache) : this(cache)
        {
            Type = json["type"].Get<int>();
            RecipientIDs = JsonArray.ToArray<string>(json["recipient_ids"].Get<JsonArray>());
            ID = json["id"].Get<string>();
            LastMsgId = json["last_message_id"].Get<double>();

            if (Type == 3)
            {
                OwnerID = json["owner_id"].Get<string>();
                Name = json["name"].Get<string>();
                IconId = json["icon"].Get<string>();
            }
        }

        bool IsStatusEq(UserStatus userStatus)
        {
            if (IsGroup || RecipientIDs == null || UsersCache == null)
                return false;
            //if (IsGroup && userStatus == UserStatus.offline)
            //    return true;
            //else if (IsGroup)
            //    return false;
            var us = UsersCache.GetUser(RecipientIDs[0]).UserStatus;
            return us == userStatus;
        }
    }
}
