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

        public UsersCache(JsonArray jsonArray, Discord discord)
        {
            _Discord = discord;
            foreach (JsonArrayObject obj in jsonArray)
            {
                Users.Add(GetUser(obj.Get<Json>()));
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
            Users.Add(user);
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
            return ui;
        }
    }
}
