using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarfaceStatusGUI
{
    public class MyGames
    {
        public string PHPSESSID;
        public string CODE;

        public bool BrowserOpened = false;

        public bool Auth()
        {
            var cl = new AuthMyGames(AuthMyGames.types.Auth);
            BrowserOpened = true;
            cl.ShowDialog();
            BrowserOpened = false;
            if (!string.IsNullOrEmpty(cl.PHPSESSID))
            {
                PHPSESSID = cl.PHPSESSID;
                CODE = cl.CODE;
                return true;
            }
            return false;
        }

        public void Validate()
        {
            var cl = new AuthMyGames(AuthMyGames.types.Validate, "PHPSESSID=" + PHPSESSID);
            BrowserOpened = true;
            cl.ShowDialog();
            BrowserOpened = false;
        }
    }
}
