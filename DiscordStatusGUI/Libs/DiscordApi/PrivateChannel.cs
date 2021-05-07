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

        public PrivateChannels(JsonArray obj, UsersCache cache)
        {
            foreach (var ch in obj)
            {
                var d = ch.Get<Json>();
                Channels.Insert(0, new PrivateChannel(d, cache));
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
                else if (ID != null)
                {
                    if (Type == 1)
                    {
                        return _Name = UsersCache.GetUser(RecipientIDs[0]).UserName;
                    } else if (Type == 3)
                    {
                        if (RecipientIDs.Length == 0)
                            _Name = UsersCache.GetUser(OwnerID).UserName;
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
                        return _Name;
                    }
                }
                return $"channel(ID)";
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

        UsersCache UsersCache;
        ImageSource DefaultGroupIcon = BitmapEx.ToImageSource(Discord.GetImageByUrl("https://discord.com/assets/1531b79c2f2927945582023e1edaaa11.png"));

        public PrivateChannel()
        {
            DefaultGroupIcon.Freeze();
        }

        public PrivateChannel(Json json, UsersCache cache) : this()
        {
            Type = json["type"].Get<int>();
            RecipientIDs = JsonArray.ToArray<string>(json["recipient_ids"].Get<JsonArray>());
            ID = json["id"].Get<string>();

            if (Type == 3)
            {
                OwnerID = json["owner_id"].Get<string>();
                Name = json["name"].Get<string>();
                IconId = json["icon"].Get<string>();
            }

            UsersCache = cache;
        }
    }
}
