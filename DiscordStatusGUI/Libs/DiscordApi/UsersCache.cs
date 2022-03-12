using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PinkJson;
using WEBLib;

namespace DiscordStatusGUI.Libs.DiscordApi
{
    public class UsersCache
    {
        public List<UserInfo> Users { get; private set; } = new List<UserInfo>();

        Discord _Discord;

        public UsersCache(Discord discord)
        {
            _Discord = discord;
        }

        public void AddRange(JsonArray jsonArray)
        {
            foreach (JsonArrayObject obj in jsonArray)
            {
                GetUser(obj.Get<Json>());
            }
        }

        public void SetUserStatus(string uid, string status, bool useinvisible)
        {
            var user = GetUser(uid);
            switch (status)
            {
                case "online":
                    user.UserStatus = UserStatus.online;
                    break;
                case "idle":
                    user.UserStatus = UserStatus.idle;
                    break;
                case "dnd":
                    user.UserStatus = UserStatus.dnd;
                    break;
                case "offline":
                    if (useinvisible)
                        user.UserStatus = UserStatus.invisible;
                    else
                        user.UserStatus = UserStatus.offline;
                    break;
            }
        }

        public UserInfo GetUser(string id)
        {
            foreach (var u in Users)
            {
                if (u.Id == id)
                    return u;
            }
            return GetUserById(id);
        }

        UserInfo GetUserById(string id)
        {
            var result = new Json(WEB.Post("https://discord.com/api/v" + Discord.DiscordApiVersion + "/users/" + id, new string[] { "authorization: " + _Discord.Token }, null, "GET"));
            var user = GetUser(result);
            return user;
        }

        UserInfo GetUser(Json json)
        {
            var ui = new UserInfo();
            ui.UserName = json["username"].Get<string>();
            //if (json.IndexByKey("public_flags") != -1)
            //PublicFlags = json["public_flags"].Get<int>();
            ui.Id = json["id"].Get<string>();
            ui.Discriminator = json["discriminator"].Get<string>();
            ui.AvatarId = json["avatar"].Get<string>();
            ui.UserStatus = UserStatus.offline;
            Users.Add(ui);
            return ui;
        }
    }
}
