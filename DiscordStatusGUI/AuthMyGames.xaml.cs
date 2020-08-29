using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using mshtml;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WarfaceStatusGUI
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class AuthMyGames : Window
    {
        public AuthMyGames(types type, string cookie = "")
        {
            InitializeComponent();

            this.type = type;
            Application.SetCookie(new Uri("https://ru.warface.com"), cookie);
        }

        public enum types
        {
            Auth,
            Validate
        }

        static string Validate = "https://ru.warface.com/validate/?ref_url=ru.warface.com";
        static string OAuth = "https://account.my.games/oauth2/?client_id=ru.warface.com&amp;redirect_uri=https%3A%2F%2Fru.warface.com%2Fdynamic%2Fauth%2F%3Fo2%3D1&amp;response_type=code&amp;signup_method=email%2Cphone&amp;signup_social=fb%2Cvk%2Cg%2Cok%2Ctwitch%2Ctw&amp;lang=ru_RU&amp;gc_id=0.1177";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (type == types.Auth)
                browser.Navigate(OAuth);
            else if (type == types.Validate)
            {
                browser.Navigate(Validate);
            }
        }

        string cookie;
        types type;
        public string PHPSESSID;
        public string CODE;
        bool redirBack = false;
        private void browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (type == types.Auth)
            {
                if (redirBack == true)
                {
                    browser.Navigate(OAuth);
                    redirBack = false;
                }
                if (e.Uri.ToString().IndexOf("o2=1&code=") != -1)
                {
                    Regex regex = new Regex(@"(?<=PHPSESSID=)[^;]*");
                    PHPSESSID = regex.Match((browser.Document as HTMLDocument).cookie).Value;
                    regex = new Regex(@"(?<=code=).*");
                    CODE = regex.Match((browser.Document as HTMLDocument).cookie).Value;
                    this.Close();
                }
                if (e.Uri.ToString().IndexOf("validate") != -1)
                {
                    redirBack = true;
                }
            }
            else if (type == types.Validate)
            {
                if (e.Uri.ToString().IndexOf("validate") == -1)
                {
                    var cookiess = (browser.Document as HTMLDocument).cookie;
                    this.Close();
                }
            }
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SetSilent(browser, true);
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                return;

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }
}
