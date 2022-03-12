using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DiscordStatusGUI.Extensions;

namespace DiscordStatusGUI.Libs.DiscordApi
{
    public class UserInfo
    {
        public string UserName;
        public string Phone;
        public string Id;
        public string Email;
        public string Discriminator;
        public string AvatarId;
        public UserStatus UserStatus;

        public ImageSource _Avatar;
        public ImageSource Avatar
        {
            get
            {
                if (_Avatar != null)
                    return _Avatar;

                if (AvatarId == null)
                    _Avatar = BitmapEx.ToImageSource(Properties.Resources.DefaultAvatar);
                else
                    _Avatar = BitmapEx.ToImageSource(Discord.GetImageByUrl($"https://cdn.discordapp.com/avatars/{Id}/{AvatarId}.png?size=128"));
                _Avatar.Freeze();
                return _Avatar;
            }
        }
    }
}
