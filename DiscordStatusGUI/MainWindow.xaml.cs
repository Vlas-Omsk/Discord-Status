using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Drawing.Printing;
using System.Configuration;
using System.Reflection;
using PinkJson;
using WarfaceStatus;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;
using WEBLib;
using DiscordStatusGUI.locales;

namespace DiscordStatusGUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Directory.SetCurrentDirectory(CurrentDir);
            lang.Init();

            InitializeComponent();
            TopStatus.Text = "Инициализация";

            c.init(console);
            Animations.Init();
            Dialogs.Init(this);
            Ni_Init();

            var cuiu = lang.InitForm_initialization;

            Icon = Animations.ImageSourceFromBitmap(Properties.Resources.export_small);

            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-tray") != -1)
                this.Hide();

            bool IsWindows10or8()
            {
                if ((Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2) || Environment.OSVersion.Version.Major == 10)
                    return true;
                else
                    return false;
            }

            if (!IsWindows10or8())
                WindowStyle = WindowStyle.None;


            BG_Plug_Location = new Thickness(BG_Plug.Margin.Left, BG_Plug.Margin.Top, BG_Plug.Margin.Right, BG_Plug.Margin.Bottom);

            //VisualiserTimer
            GameTimeUpdater.Elapsed += GameTimeUpdater_Elapsed;
            GameTimeUpdater.AutoReset = true;
            GameTimeUpdater.Enabled = true;
            GameTimeUpdater.Start();
        }

        public static double AppVer = 0.5;
        static string CurrentExe = Environment.GetCommandLineArgs()[0];
        static string CurrentDir = System.IO.Path.GetDirectoryName(CurrentExe);

        #region NotifyIcon
        private void Ni_Init()
        {
            var ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = System.Drawing.Icon.FromHandle(Properties.Resources.export_small_tray.GetHicon());
            ni.Visible = true;
            ni.Text = "Discord Status";
            ni.MouseUp += Ni_MouseUp;
        }

        private void Ni_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.Show();
                this.Activate();
                this.WindowState = WindowState.Normal;
            } else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                NotifyPopup.IsOpen = true;
            }
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NotifyPopup.IsOpen = false;

            clearStatus();
            PreferencesSave();
            discord.WSDisconnect();
            this.Close();
        }
        #endregion NotifyIcon

        #region Window Style
        IntPtr Handle;
        int xborder;
        int yborder;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct NCCALCSIZE_PARAMS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public RECT[] rgrc;
            public IntPtr lppos;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle,
        bool bMenu, uint dwExStyle);

        [DllImport("dwmapi.dll")]
        static extern bool DwmDefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref IntPtr plResult);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(uint smIndex);

        static int GET_X_LPARAM(IntPtr lp)
        {
            short loword = (short)((ulong)lp & 0xffff);
            return loword;
        }

        static int GET_Y_LPARAM(IntPtr lp)
        {
            short hiword = (short)((((ulong)lp) >> 16) & 0xffff);
            return hiword;
        }

        const uint WM_NCCALCSIZE = 0x0083;
        const uint WM_NCHITTEST = 0x0084;
        const uint WM_ACTIVATE = 0x0006;
        const uint WM_NCACTIVATE = 0x0086;
        const uint WM_NCPAINT = 0x85;

        const uint WS_OVERLAPPED = 0x00000000;
        const uint WS_CAPTION = 0x00C00000;
        const uint WS_SYSMENU = 0x00080000;
        const uint WS_THICKFRAME = 0x00040000;
        const uint WS_MINIMIZEBOX = 0x00020000;
        const uint WS_MAXIMIZEBOX = 0x00010000;
        const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU |
              WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

        const uint HTTOPLEFT = 13;
        const uint HTTOPRIGHT = 14;
        const uint HTTOP = 12;
        const uint HTCAPTION = 2;
        const uint HTLEFT = 10;
        const uint HTNOWHERE = 0;
        const uint HTRIGHT = 11;
        const uint HTBOTTOM = 15;
        const uint HTBOTTOMLEFT = 16;
        const uint HTBOTTOMRIGHT = 17;

        const uint SM_CXSIZEFRAME = 32;
        const uint SM_CYSIZEFRAME = 33;

        //обработка координат мыши для неклиентской области
        IntPtr HitTestNCA(IntPtr hWnd, IntPtr wParam, IntPtr lParam)
        {
            // Get the point coordinates for the hit test.
            var ptMouse = new Point(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));

            // Get the window rectangle.
            RECT rcWindow;
            GetWindowRect(hWnd, out rcWindow);

            // Get the frame rectangle, adjusted for the style without a caption.
            RECT rcFrame = new RECT();
            AdjustWindowRectEx(ref rcFrame, WS_OVERLAPPEDWINDOW & ~WS_CAPTION, false, 0);

            // Determine if the hit test is for resizing. Default middle (1,1).
            ushort uRow = 1;
            ushort uCol = 1;
            bool fOnResizeBorder = false;

            // Determine if the point is at the top or bottom of the window.
            if (ptMouse.Y >= rcWindow.top && ptMouse.Y < rcWindow.top + yborder)
            {
                fOnResizeBorder = (ptMouse.Y < (rcWindow.top - rcFrame.top));
                uRow = 0;
            }
            else if (ptMouse.Y < rcWindow.bottom && ptMouse.Y >= rcWindow.bottom - yborder)
            {
                uRow = 2;
            }

            // Determine if the point is at the left or right of the window.
            if (ptMouse.X >= rcWindow.left && ptMouse.X < rcWindow.left + xborder)
            {
                uCol = 0; // left side
            }
            else if (ptMouse.X < rcWindow.right && ptMouse.X >= rcWindow.right - xborder)
            {
                uCol = 2; // right side
            }

            // Hit test (HTTOPLEFT, ... HTBOTTOMRIGHT)
            IntPtr[,] hitTests = new IntPtr[,]
            {
                { (IntPtr)HTTOPLEFT, fOnResizeBorder? (IntPtr)HTTOP : (IntPtr)HTCAPTION, (IntPtr)HTTOPRIGHT },
                { (IntPtr)HTLEFT,  (IntPtr)HTNOWHERE, (IntPtr)HTRIGHT},
                { (IntPtr)HTBOTTOMLEFT, (IntPtr)HTBOTTOM, (IntPtr)HTBOTTOMRIGHT },
            };

            return hitTests[uRow, uCol];
        }

        //обработчик сообщений для окна
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            bool fCallDWP = true;
            IntPtr lRet = IntPtr.Zero;

            if (msg == WM_NCCALCSIZE)
            {
                if (wParam != (IntPtr)0)
                {
                    //убираем стандартную рамку сверху
                    lRet = IntPtr.Zero;

                    NCCALCSIZE_PARAMS pars = (NCCALCSIZE_PARAMS)Marshal.PtrToStructure(lParam, typeof(NCCALCSIZE_PARAMS));

                    pars.rgrc[0].top = pars.rgrc[0].top;
                    pars.rgrc[0].left = pars.rgrc[0].left + xborder;
                    pars.rgrc[0].right = pars.rgrc[0].right - xborder * 2;
                    pars.rgrc[0].bottom = pars.rgrc[0].bottom - yborder;

                    Marshal.StructureToPtr(pars, lParam, false);

                    handled = true;
                    return lRet;
                }
            }

            if (msg == WM_NCACTIVATE)
            {
                lRet = (IntPtr)1;
                handled = true;
                return lRet;
            }

            fCallDWP = !DwmDefWindowProc(hwnd, msg, wParam, lParam, ref lRet);

            if (msg == WM_NCHITTEST && lRet == IntPtr.Zero)
            {
                //обработка нажатий мыши
                lRet = HitTestNCA(hwnd, wParam, lParam);

                if (lRet != (IntPtr)HTNOWHERE)
                {
                    fCallDWP = false;
                }
            }

            //если сообщение не обработано, передаем базовой процедуре
            if (fCallDWP) handled = false;
            else handled = true;

            return lRet;
        }

        private void Window_Loaded2()
        {
            WindowInteropHelper h = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(h.Handle);
            Handle = h.Handle;
            source.AddHook(new HwndSourceHook(WndProc));//регистрируем обработчик сообщений
            xborder = GetSystemMetrics(SM_CXSIZEFRAME);
            yborder = GetSystemMetrics(SM_CYSIZEFRAME);
        }
        #endregion Window Style

        public Thickness BG_Plug_Location;

        #region CustomWindowEvents
        private void WindowDragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
                preferences["AppData"]["MainWindowWidth"].Value = e.NewSize.Width;
            if (e.HeightChanged)
                preferences["AppData"]["MainWindowHeight"].Value = e.NewSize.Height;
        }

        private void WindowLocationChanged(object sender, EventArgs e)
        {
            if (IsPreferencesLoaded)
            {
                preferences["AppData"]["MainWindowLeft"].Value = window.Left;
                preferences["AppData"]["MainWindowTop"].Value = window.Top;
            }
        }

        private void MinimizeWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //var opacity = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(100)));
                //opacity.Completed += (s, a) => this.WindowState = WindowState.Minimized;
                //this.BeginAnimation(Window.OpacityProperty, opacity);
                this.WindowState = WindowState.Minimized;
            }
        }

        private void MaximizeWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                }
        }

        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            if (e == null || e.LeftButton == MouseButtonState.Pressed)
            {
                this.Hide();
                this.WindowState = WindowState.Minimized;

                PreferencesSave();
                //Dialogs.ShowMessageBoxOk("Выход", "Вы же не хотели закрыть это окно?\r\n\r\nТогда, я думаю, у нас всё сложится");//\r\n              ТЕСТ              В наше время трудно найти человека, который не слышал бы о черных дырах. Вместе с тем, пожалуй, не менее трудно отыскать того, кто смог бы объяснить, что это такое. Впрочем, для специалистов черные дыры уже перестали быть фантастикой - астрономические наблюдения давно доказали существование как \"малых\" черных дыр (с массой порядка солнечной), которые образовались в результате гравитационного сжатия звезд, так и сверхмассивных (до 109 масс Солнца), которые породил коллапс целых звездных скоплений в центрах многих галактик, включая нашу. В настоящее время микроскопические черные дыры ищут в потоках космических лучей сверхвысоких энергий (международная лаборатория Pierre Auger, Аргентина) и даже предполагают \"наладить их производство\" на Большом адронном коллайдере (LHC), который планируют запустить в 2007 году в ЦЕРНе. Однако подлинная роль черных дыр, их \"предназначение\" для Вселенной, находится далеко за рамками астрономии и физики элементарных частиц. При их изучении исследователи глубоко продвинулись в научном понимании прежде сугубо философских вопросов - что есть пространство и время, существуют ли границы познания Природы, какова связь между материей и информацией.");
            }
        }

        private void WindowStateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized)
                preferences["AppData"]["MainWindowState"].Value = (int)WindowState;
            if (this.WindowState != WindowState.Minimized)
            {
                //var opacity = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));
                //this.BeginAnimation(Window.OpacityProperty, opacity);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window_Loaded2();

            new Thread(() =>
            {
                discord = new Discord(this);
                myGames = new MyGames();
                Dispatcher.Invoke(() => {
                    Animations.VisibleOn(InitForm).Begin();
                });
                var ShowLogin = PreferencesLoad();
                Warface.Init(this);
                Dispatcher.Invoke(() => {
                    TopStatus.Text = "";
                    Animations.VisibleOff(InitForm).Begin();
                    if (ShowLogin)
                        Animations.VisibleOn(LoginForm).Begin();
                    c.i("Working directory: " + Directory.GetCurrentDirectory());
                    if (!string.IsNullOrEmpty(c.CRITICAL))
                        SendReport();
                });
                setStatus();
            }).Start();
        }

        void SendReport()
        {
            Dialogs.ShowMessageBoxOk("Что-то пошло не так...", c.CRITICAL.Substring(c.CRITICAL.IndexOf("[CRITICAL ERROR]")));

            var t = new Thread(() =>
            {
                while (true)
                {
                    Dispatcher.Invoke(() => c.u("Bug Report", "Trying to send"));
                    string response = "null";
                    var content = c.CRITICAL.Replace("\\", "\\\\").Replace("\r", "").Replace("\n", "\\n").Replace("\"", "\\\"");
                    if (content.Length <= 1999)
                    {
                        var message = new Json(new
                        {
                            content = "```\\n" + content + "\\n```",
                            tts = false
                        });

                        response = WEB.Post("https://discordapp.com/api/webhooks/748822274462842930/TM6tDHZcB-EFrs9MHgsmeXbIMXXlO_vdqFwJGwJ6o8vxfTajmtnv0EXwYWWeRSCmFsuA", new string[] {
                        "content-type: application/json",
                        "content-length: " + message.ToString().Length },
                            message.ToString(), "POST");
                    }
                    else
                    {
                        var truncatedContent = "Report.txt";
                        try
                        {
                            truncatedContent = c.CRITICAL.Substring(c.CRITICAL.IndexOf("[CRITICAL ERROR]")).Substring(0, 1990);
                        }
                        catch
                        {
                            if (c.CRITICAL.Substring(c.CRITICAL.IndexOf("[CRITICAL ERROR]")).Length <=1990)
                                truncatedContent = c.CRITICAL.Substring(c.CRITICAL.IndexOf("[CRITICAL ERROR]"));
                        }
                        var message = "------WebKitFormBoundary8Vs8W8uiXYBIkUa4\n" +
                            "Content-Disposition: form-data; name=\"file\"; filename=\"Report.txt\"\n" +
                            "Content-Type: text/plain\n\n" + content + "\n" +
                            "------WebKitFormBoundary8Vs8W8uiXYBIkUa4\n" +
                            "Content-Disposition: form-data; name=\"content\"\n\n" + "```" + truncatedContent + "```" + "\n" +
                            "------WebKitFormBoundary8Vs8W8uiXYBIkUa4\n" +
                            "Content-Disposition: form-data; name=\"tts\"\n\n" + "false" + "\n" +
                            "------WebKitFormBoundary8Vs8W8uiXYBIkUa4--";
                        response = WEB.Post("https://discordapp.com/api/webhooks/748822274462842930/TM6tDHZcB-EFrs9MHgsmeXbIMXXlO_vdqFwJGwJ6o8vxfTajmtnv0EXwYWWeRSCmFsuA", new string[] {
                            "content-type: multipart/form-data; boundary=----WebKitFormBoundary8Vs8W8uiXYBIkUa4",
                            "content-length: " + message.ToString().Length },
                            message.ToString(), "POST");
                    }

                    if (string.IsNullOrEmpty(response) || response.IndexOf("url", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        Dispatcher.Invoke(() => c.u("Bug Report", "Success"));
                        break;
                    }
                    Thread.Sleep(10000);
                }
            });
            t.IsBackground = true;
            t.Start();
        }
        #endregion CustomWindowEvents

        #region CatEmotions
        int clicks_login_cat = 0;
        public static ImageSource[] cat_go_frames = new ImageSource[]
            {
                Animations.ImageSourceFromBitmap(Properties.Resources.pixel_cat_frame11),
                Animations.ImageSourceFromBitmap(Properties.Resources.pixel_cat_frame21),
                Animations.ImageSourceFromBitmap(Properties.Resources.pixel_cat_frame31),
                Animations.ImageSourceFromBitmap(Properties.Resources.pixel_cat_frame41)
            };
        private void CatLoginClick(object sender, MouseButtonEventArgs e)
        {
            if (clicks_login_cat <= 7)
            {
                Animations.Shake(2, 15, new TimeSpan(00, 00, 00, 00, 500), cat_emotion_login).Begin();
                if (clicks_login_cat == 5)
                    cat_emotion_login.Text = "Хватит!";
            }
            else
            {
                cat_login.MouseUp -= CatLoginClick;

                var opacity = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(100)));
                opacity.Completed += (s, a) => cat_emotion_login.Visibility = Visibility.Hidden;
                cat_emotion_login.BeginAnimation(Window.OpacityProperty, opacity);
                Animations.CreateScaleTransform(1, 0.7, 1, 0.7, cat_emotion_login).Begin();

                var t = new Thread(() =>
                {
                    var i = 0;
                    while (true)
                    {
                        Thread.Sleep(200);

                        Dispatcher.Invoke(() =>
                        {
                            cat_login.Source = cat_go_frames[i];
                        });

                        i++;
                        if (i >= 4)
                            i = 0;
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.IsBackground = true;
                t.Start();

                ThicknessAnimation go_out = new ThicknessAnimation();
                go_out.Duration = TimeSpan.FromMilliseconds(ActualWidth * 50);
                go_out.To = new Thickness(-(ActualWidth * 2), 0, 0, 0);
                go_out.Completed += (ss, ee) =>
                {
                    t.Abort();
                    cat_login.Visibility = Visibility.Hidden;
                };

                cat_login.BeginAnimation(MarginProperty, go_out);
            }
            clicks_login_cat += 1;
        }

        private void CatCodeClick(object sender, MouseButtonEventArgs e)
        {
            Animations.Shake(1, 15, new TimeSpan(00, 00, 00, 00, 500), cat_emotion_code).Begin();
            Animations.Shake(1, 15, new TimeSpan(00, 00, 00, 00, 500), cat_code).Begin();
        }
        #endregion CatEmotions

        public Discord discord;
        public MyGames myGames;

        #region AuthForm Events
        private void ButtonLoginClick(object sender, RoutedEventArgs e)
        {
            ButtonLogin.IsEnabled = false;
            discord.Email = Email.Text;
            discord.Password = Passw.Text;

            var t = new Thread(() =>
            {
                Dispatcher.Invoke(() => RestoreStyleForEmailPassword());
                var auth = discord.Auth();
                if (auth == AuthErrors.LoginError)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var resp = new Json(discord.LastError.Message);
                        EmailLabelInfo.Content = PasswLabelInfo.Content = "";
                        if (resp.IndexByKey("email") != -1)
                        {
                            EmailLabelInfo.Content = resp["email"][0].Value;
                            EmailLabel.BeginAnimation(ForegroundProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                            Email.BeginAnimation(BorderBrushProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                        }
                        if (resp.IndexByKey("password") != -1)
                        {
                            PasswLabelInfo.Content = resp["password"][0].Value;
                            PasswLabel.BeginAnimation(ForegroundProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                            Passw.BeginAnimation(BorderBrushProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                        }
                        return;
                    });
                } else if (auth == AuthErrors.Error)
                {
                    Dispatcher.Invoke(() =>
                    {
                        EmailLabelInfo.Content = PasswLabelInfo.Content = "";
                        EmailLabelInfo.Content = discord.LastError.Message;
                    });
                } else if (auth == AuthErrors.MultiFactorAuthentication)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Animations.VisibleOff(LoginForm).Begin();
                        Animations.VisibleOn(CodeForm).Begin();
                        //LoginForm.Visibility = Visibility.Hidden;
                        //CodeForm.Visibility = Visibility.Visible;
                    });
                } else if (auth == AuthErrors.Successful)
                {
                    Dispatcher.Invoke(() =>
                    {
                        FinishLogin();
                    });
                }
                Dispatcher.Invoke(() => { ButtonLogin.IsEnabled = true; });
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void ForgotPasswordClick(object sender, MouseButtonEventArgs e)
        {
            discord.Email = Email.Text;

            var t = new Thread(() =>
            {
                Dispatcher.Invoke(() => RestoreStyleForEmailPassword());
                var forgot = discord.ForgotPassword();
                if (forgot == ForgotPasswordErrors.DataError)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var resp = new Json(discord.LastError.Message);
                        EmailLabelInfo.Content = PasswLabelInfo.Content = "";
                        if (resp.IndexByKey("email") != -1)
                        {
                            EmailLabelInfo.Content = resp["email"][0].Value;
                            EmailLabel.BeginAnimation(ForegroundProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                            Email.BeginAnimation(BorderBrushProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                        }
                        return;
                    });
                }
                else if (forgot == ForgotPasswordErrors.Error)
                {
                    Dispatcher.Invoke(() =>
                    {
                        EmailLabelInfo.Content = PasswLabelInfo.Content = "";
                        EmailLabelInfo.Content = discord.LastError.Message;
                    });
                }
                else if (forgot == ForgotPasswordErrors.Successful)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Dialogs.ShowMessageBoxOk("Инструкции отправлены", "Мы отправили инструкции по смене пароля на " + Email.Text + ", пожалуйста, проверьте папки «Входящие» и «Спам».");
                    });
                }
                Dispatcher.Invoke(() => { ButtonLogin.IsEnabled = true; });
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void RegistrationClick(object sender, MouseButtonEventArgs e)
        {
            var proc = new Process();
            proc.StartInfo.FileName = "cmd";
            proc.StartInfo.Arguments = "/c start https://discord.com/register";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
        }

        private void SkipClick(object sender, MouseButtonEventArgs e)
        {
            FinishLogin();
        }

        private void Email_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Email.Text.Length > 3 && Email.Text.IndexOf('@') != -1)
            {
                EmailLabelInfo.Content = "";
                EmailLabel.BeginAnimation(Label.ForegroundProperty, Animations.LPRestoreLabel);
                Email.BeginAnimation(TextBox.BorderBrushProperty, Animations.LPRestoreTextBox);
            }
        }

        private void Passw_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Passw.Text.Length > 4)
            {
                PasswLabelInfo.Content = "";
                PasswLabel.BeginAnimation(Label.ForegroundProperty, Animations.LPRestoreLabel);
                Passw.BeginAnimation(TextBox.BorderBrushProperty, Animations.LPRestoreTextBox);
            }
        }

        private void RestoreStyleForEmailPassword()
        {
            PasswLabelInfo.Content = EmailLabelInfo.Content = "";
            PasswLabel.BeginAnimation(Label.ForegroundProperty, Animations.LPRestoreLabel);
            Passw.BeginAnimation(TextBox.BorderBrushProperty, Animations.LPRestoreTextBox);
            EmailLabel.BeginAnimation(Label.ForegroundProperty, Animations.LPRestoreLabel);
            Email.BeginAnimation(TextBox.BorderBrushProperty, Animations.LPRestoreTextBox);
        }
        #endregion AuthForm Events

        #region CodeForm Events
        private void Code_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Code.Text.Length >= 6)
            {
                CodeLabelInfo.Content = "";
                CodeLabel.BeginAnimation(Label.ForegroundProperty, Animations.LPRestoreLabel);
                Code.BeginAnimation(TextBox.BorderBrushProperty, Animations.LPRestoreTextBox);
            }
        }

        public MFAuthType discordMFAuthType = MFAuthType.Code;
        private void ButtonLoginCodeClick(object sender, RoutedEventArgs e)
        {
            ButtonLoginCode.IsEnabled = false;

            var t = new Thread(() =>
            {
                int code = 0;
                Dispatcher.Invoke(() => {
                    try
                    {
                        code = Convert.ToInt32(Code.Text);
                    }
                    catch { }
                });
                MFAuthErrors auth = discord.MFAuth(code, discordMFAuthType);

                if (auth == MFAuthErrors.InvalidData)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var resp = new Json(discord.LastError.Message);
                        EmailLabelInfo.Content = "";
                        CodeLabelInfo.Content = resp[0].Value;
                        CodeLabel.BeginAnimation(ForegroundProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                        Code.BeginAnimation(BorderBrushProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                        return;
                    });
                }
                else if (auth == MFAuthErrors.Error)
                {
                    Dispatcher.Invoke(() =>
                    {
                        CodeLabelInfo.Content = "";
                        CodeLabelInfo.Content = discord.LastError.Message;
                    });
                }
                else if (auth == MFAuthErrors.Successful)
                {
                    Dispatcher.Invoke(() =>
                    {
                        FinishLogin();
                    });
                }
                Dispatcher.Invoke(() => { ButtonLoginCode.IsEnabled = true; });
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void MFAuthSendSMS(object sender, MouseButtonEventArgs e)
        {
            var t = new Thread(() =>
            {
                MFAuthErrors auth = discord.MFAuthSendSMS();

                if (auth == MFAuthErrors.InvalidData)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var resp = new Json(discord.LastError.Message);
                        EmailLabelInfo.Content = "";
                        CodeLabelInfo.Content = resp[0].Value;
                        CodeLabel.BeginAnimation(ForegroundProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                        Code.BeginAnimation(BorderBrushProperty, Animations.LPWrongOrEmptyForegroundAnimation);
                        return;
                    });
                }
                else if (auth == MFAuthErrors.Error)
                {
                    Dispatcher.Invoke(() =>
                    {
                        CodeLabelInfo.Content = "";
                        CodeLabelInfo.Content = discord.LastError.Message;
                    });
                }
                else if (auth == MFAuthErrors.Successful)
                {
                    Dispatcher.Invoke(() =>
                    {
                        FormCaption.Text = "Мы отправили сообщение на " + discord.Phone + ". Пожалуйста, введите полученный код.";
                        discordMFAuthType = MFAuthType.SMS;
                    });
                }
                Dispatcher.Invoke(() => { ButtonLoginCode.IsEnabled = true; });
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void BackToLoginClick(object sender, MouseButtonEventArgs e)
        {
            Animations.VisibleOff(CodeForm).Begin();
            Animations.VisibleOn(LoginForm).Begin();
            //CodeForm.Visibility = Visibility.Hidden;
            //LoginForm.Visibility = Visibility.Visible;
        }
        #endregion AuthForm Events

        #region ContentForm Events
        private void ContentFormOpen()
        {
            new Thread(() =>
            {
                while (Dialogs.IsMessageBoxOkOpened)
                    Thread.Sleep(100);
                Dispatcher.Invoke(() => {
                    Animations.VisibleOn(ContentForm).Begin();
                    //VisualiserCheck(Visualiser, null);
                });
                Thread.Sleep(2000);
                Dispatcher.Invoke(() =>
                    ContentFormOnOpen());
            }).Start();
        }

        private void ContentFormOnOpen()
        {
            //Dialogs.ShowMessageBoxOk("Внимание!", "Для корректной работы программы, пожалуйста, отключите функцию «Отображать в статусе игру, в которую вы сейчас играете» в своем основном Discord приложении.");
        }

        private void HideAllScreens()
        {
            if (Visualiser != null && Visualiser.Visibility == Visibility.Visible)
                Animations.VisibleOff(Visualiser).Begin();
            if (Settings != null && Settings.Visibility == Visibility.Visible)
                Animations.VisibleOff(Settings).Begin();
            if (WarfaceScr != null && WarfaceScr.Visibility == Visibility.Visible)
                Animations.VisibleOff(WarfaceScr).Begin();
            if (Console != null && Console.Visibility == Visibility.Visible)
                Animations.VisibleOff(Console).Begin();
        }

        #region Visualiser
        private void VisualiserCheck(object sender, RoutedEventArgs e)
        {
            HideAllScreens();

            if (Visualiser != null)
                Animations.VisibleOn(Visualiser).Begin();
        }

        private void VisualizerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth - 72 - 510 < 370)
            {
                Grid.SetRow(VisualiserOptions, 3);
                Grid.SetColumnSpan(VisualiserOptions, 2);
                Grid.SetColumn(VisualiserTabControl, 1);
                Grid.SetColumnSpan(VisualiserTabControl, 2);
            }
            else
            {
                Grid.SetRow(VisualiserOptions, 1);
                Grid.SetColumnSpan(VisualiserOptions, 1);
                Grid.SetColumn(VisualiserTabControl, 2);
                Grid.SetColumnSpan(VisualiserTabControl, 1);
            }
        }

        private void VisualizerStateTextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                VisualiserTabControlGameParty.Text = VisualiserState.Text + " (" + Convert.ToInt32(VisualiserPartySize.Text) + " из " + Convert.ToInt32(VisualiserPartyMax.Text) + ")";
            }
            catch
            {
                VisualiserTabControlGameParty.Text = VisualiserState.Text;
            }
            AutoUpdateDiscord(null, null);
        }

        System.Timers.Timer GameTimeUpdater = new System.Timers.Timer(500);
        private void GameTimeUpdater_Elapsed(object sender, ElapsedEventArgs e)
        {
            var TicksNow = DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks;
            var TimeNow = (long)new TimeSpan(TicksNow).TotalMilliseconds;

            string end_timestamp = "";
            string start_timestamp = "";
            Dispatcher.Invoke(() =>
            {
                end_timestamp = Visualiser_Timestamp_End.Text;
                start_timestamp = Visualiser_Timestamp_Start.Text;
            });

            try
            {
                if (string.IsNullOrEmpty(end_timestamp))
                {
                    var timeElapsed = new TimeSpan((TimeNow - Convert.ToInt64(start_timestamp)) * 10000);
                    if (timeElapsed.TotalMilliseconds >= 0)
                        Dispatcher.Invoke(() =>
                            VisualiserTabControlGameTime.Text = "Прошло " + string.Format("{0:00}:{1:00}:{2:00}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds));
                    else
                        Dispatcher.Invoke(() =>
                            VisualiserTabControlGameTime.Text = "Прошло " + "00:00:00");
                }
                else
                {
                    var timeElapsed = new TimeSpan((Convert.ToInt64(end_timestamp) - TimeNow) * 10000);
                    if (timeElapsed.TotalMilliseconds >= 0)
                        Dispatcher.Invoke(() =>
                            VisualiserTabControlGameTime.Text = "Осталось " + string.Format("{0:00}:{1:00}:{2:00}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds));
                    else
                        Dispatcher.Invoke(() =>
                            VisualiserTabControlGameTime.Text = "Осталось " + "00:00:00");
                }
            }
            catch
            {
                Dispatcher.Invoke(() =>
                            VisualiserTabControlGameTime.Text = "");
            }
        }

        private void VisualizerTimeTextChanged(object sender, TextChangedEventArgs e)
        {
            AutoUpdateDiscord(null, null);
        }

        private void VisualiserApplicationIDTextChanged(object sender, TextChangedEventArgs e)
        {
            AutoUpdateDiscord(null, null);
            var t = new Thread(() =>
            {
                string AppId = "";
                try
                {
                    string LargeImgKey = "";
                    this.Dispatcher.Invoke(() =>
                    {
                        LargeImgKey = VisualiserImageLargeKey.Text;
                        AppId = VisualiserApplicationID.Text;
                    });
                    LargeImgKey = ReplaceSpecialWords(LargeImgKey);
                    AppId = ReplaceSpecialWords(AppId);

                    var bigImgId = Discord.GetImageIdByName(LargeImgKey, AppId);
                    var bigImgBmp = Discord.GetImageById(bigImgId, AppId);

                    this.Dispatcher.Invoke(() =>
                        VisualiserTabControlGameAvatar.Fill = new ImageBrush(Animations.ImageSourceFromBitmap(bigImgBmp)));
                }
                catch
                {
                    this.Dispatcher.Invoke(() =>
                        VisualiserTabControlGameAvatar.Fill = null);//new ImageBrush(Animations.ImageSourceFromBitmap(Properties.Resources.default_avatar)));
                }

                try
                {
                    string SmallImgKey = "";
                    this.Dispatcher.Invoke(() =>
                    {
                        SmallImgKey = VisualiserImageSmallKey.Text;
                    });
                    SmallImgKey = ReplaceSpecialWords(SmallImgKey);

                    var smallImgId = Discord.GetImageIdByName(SmallImgKey, AppId);
                    var smallImgBmp = Discord.GetImageById(smallImgId, AppId);

                    this.Dispatcher.Invoke(() =>
                        VisualiserTabControlGameSmallAvatar.Fill = new ImageBrush(Animations.ImageSourceFromBitmap(smallImgBmp)));
                }
                catch
                {
                    this.Dispatcher.Invoke(() =>
                        VisualiserTabControlGameSmallAvatar.Fill = null);//new ImageBrush(Animations.ImageSourceFromBitmap(Properties.Resources.default_avatar)));
                }
            });
            t.Start();
        }

        private void OpenDocsHow_to_use_pictures(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://vlas-omsk.github.io/documentation.html#how_to_use_pictures");
        }
        #endregion Visualiser

        #region Settings
        private void SettindsCheck(object sender, RoutedEventArgs e)
        {
            HideAllScreens();

            if (Settings != null)
                Animations.VisibleOn(Settings).Begin();
        }

        private void StreamStatusDiscordChecked(object sender, RoutedEventArgs e)
        {
            StreamStatusDiscordClick(StreamStatusDiscord, null);
        }

        private void StreamStatusDiscordClick(object sender, RoutedEventArgs e)
        {
            if (StreamStatusDiscord.IsChecked == true)
            {
                if (e == null)
                {
                    if (string.IsNullOrEmpty(discord.Token))
                    {
                        AuthDiscordClick(AuthDiscord, null);
                        new Thread(() =>
                        {
                            Thread.Sleep(250);
                            Dispatcher.Invoke(() =>
                                StreamStatusDiscord.IsChecked = false);
                        }).Start();
                        return;
                    }

                    preferences["AppData"]["StreamStatusDiscord"].Value = true;

                    discord.WSConnect();

                    c.i("StreamStatusDiscord = true");
                }
            }
            else
            {
                clearStatus();

                preferences["AppData"]["StreamStatusDiscord"].Value = false;

                discord.WSDisconnect();

                c.i("StreamStatusDiscord = false");
            }
            PreferencesSave();
        }

        private void AutoRunChecked(object sender, RoutedEventArgs e)
        {
            AutoRunClick(AutoRun, null);
        }

        private void AutoRunClick(object sender, RoutedEventArgs e)
        {
            if (AutoRun.IsChecked == true)
            {
                if (e == null)
                {
                    var reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\");
                    try
                    {
                        reg.SetValue("WarfaceStatusGUI", "\"" + CurrentExe + "\" -tray");
                    }
                    catch { }
                    reg.Close();

                    preferences["AppData"]["AutoRun"].Value = true;

                    c.i("AutoRun = true");
                }
            }
            else
            {
                var reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\");
                try
                {
                    reg.DeleteValue("WarfaceStatusGUI");
                }
                catch { }
                reg.Close();

                preferences["AppData"]["AutoRun"].Value = false;

                c.i("AutoRun = false");
            }
            PreferencesSave();
        }

        private void NotifyOnNewCaseChecked(object sender, RoutedEventArgs e)
        {
            NotifyOnNewCaseClick(NotifyOnNewCase, null);
        }

        private void NotifyOnNewCaseClick(object sender, RoutedEventArgs e)
        {
            if (e == null)
            {
                if (NotifyOnNewCase.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(myGames.PHPSESSID))
                    {
                        AuthMyGamesClick(AuthMyGames, null);
                        new Thread(() =>
                        {
                            Thread.Sleep(250);
                            Dispatcher.Invoke(() =>
                                NotifyOnNewCase.IsChecked = false);
                        }).Start();
                        return;
                    }

                    preferences["AppData"]["NotifyOnNewCase"].Value = true;
                    _CaseChangeChecker = new System.Timers.Timer(5 * 60 * 1000);
                    _CaseChangeChecker.Elapsed += _CaseChangeChecker_Elapsed;
                    _CaseChangeChecker.AutoReset = true;
                    _CaseChangeChecker.Enabled = true;
                    _CaseChangeChecker.Start();

                    c.i("NotifyOnNewCase = true");
                }
            }
            else
            {
                if (NotifyOnNewCase.IsChecked == false)
                {
                    preferences["AppData"]["NotifyOnNewCase"].Value = false;
                    if (_CaseChangeChecker != null)
                        _CaseChangeChecker.Stop();

                    c.i("NotifyOnNewCase = false");
                }
            }
            PreferencesSave();
        }

        public System.Timers.Timer _CaseChangeChecker;
        public bool vipcaseNotify = false;
        public bool caseNotify = false;
        public static MediaPlayer mediaPlayer = new MediaPlayer();
        private void _CaseChangeChecker_Elapsed(object sender, ElapsedEventArgs e)
        {
            Json wallets = new Json("{}");
            try
            {
                wallets = new Json(WEB.Post("https://ru.warface.com/minigames/battlepass/wallets", new string[] { "User-Agent: " + WEB.DefaultUserAgent, "Cookie: PHPSESSID=" + myGames.PHPSESSID }, "", "GET"));
                c.i("Succefully get event wallets: " + wallets);
            } catch (Exception ex)
            {
                myGames.Validate();
                c.w("Error while get event wallets: " + ex);
                return;
            }
            Dispatcher.Invoke(() =>
            {
                if (wallets["state"].Value != "Success")
                {
                    AuthMyGamesClick(AuthMyGames, null);
                    c.w("Get wallet state not 'Success'");
                }
                if (wallets["data"]["victory"].Value >= 5 && caseNotify == false)
                {
                    Dialogs.ShowMessageBoxOk("", DateTime.Now + "\r\nДоступен кейс за победы");
                    PlaySound(preferences["AppData"]["NotifyOnNewCase_Sound"].Value);
                    c.i("Available victory case");
                    caseNotify = true;
                }
                else if (wallets["data"]["victory"].Value <= 5 && caseNotify == true)
                {
                    c.i("RESET Available victory case");
                    caseNotify = false;
                }
                if (wallets["data"]["victory_vip"].Value >= 5 && vipcaseNotify == false)
                {
                    Dialogs.ShowMessageBoxOk("", DateTime.Now + "\r\nДоступен vip-кейс за победы");
                    PlaySound(preferences["AppData"]["NotifyOnNewVipCase_Sound"].Value);
                    c.i("Available vip-victory case");
                    vipcaseNotify = true;
                }
                else if (wallets["data"]["victory_vip"].Value <= 5 && vipcaseNotify == true)
                {
                    c.i("RESET Available vip-victory case");
                    vipcaseNotify = false;
                }
            });
        }

        private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = e.NewValue / 100;
            preferences["AppData"]["Volume"].Value = e.NewValue;
        }

        private void Volume_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PlaySound(preferences["AppData"]["NotifyOnNewCase_Sound"].Value);
        }

        private void AuthMyGamesClick(object sender, RoutedEventArgs e)
        {
            if (myGames.BrowserOpened == false && myGames.Auth())
            {
                Animations.SetGreenStyle(AuthMyGames, (Style)FindResource("ButtonStyleGreen"));

                preferences["MyGames"]["PHPSESSID"].Value = myGames.PHPSESSID;
                preferences["MyGames"]["CODE"].Value = myGames.CODE;

                PreferencesSave();
            }
        }
        private void AuthDiscordClick(object sender, RoutedEventArgs e)
        {
            HideAllScreens();
            Animations.VisibleOff(ContentForm).Begin();
            ShowDiscordLogin();
        }

        private void RefindGameClick(object sender, RoutedEventArgs e)
        {
            Process game_process;
            if ((game_process = WarfaceGame.GetGameProcess()) != null)
            {
                var game_cmd = WarfaceGame.GetGameCommandLine(game_process);
                var game_directory = game_cmd[0].Substring(0, game_cmd[0].LastIndexOf('\\'));
                //GamePath.Text = game_directory;
            }
            else
            {
                //GamePath.Text = "not found";
            }
        }

        private void GamePathTextChanged(object sender, TextChangedEventArgs e)
        {
            //preferences["AppData"]["GamePath"].Value = GamePath.Text;
            //PreferencesSave();
        }

        private System.Timers.Timer _SendStatusTimer;
        private bool UpdateAuto = false;
        private void UpdateTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_SendStatusTimer != null)
                _SendStatusTimer.Stop();
            UpdateAuto = false;
            if (UpdateType.SelectedIndex == 0)
            {
                UpdateAuto = true;
            }
            if (UpdateType.SelectedIndex == 2)
            {
                _SendStatusTimer = new System.Timers.Timer(Convert.ToInt32(UpdateN.Text));
                _SendStatusTimer.Elapsed += _SendStatusTimer_Elapsed; ;
                _SendStatusTimer.AutoReset = true;
                _SendStatusTimer.Enabled = true;
                _SendStatusTimer.Start();
            }

            c.i("UpdateType = " + UpdateType.SelectedIndex);
        }

        private void _SendStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            setStatus();
        }

        public void AutoUpdateDiscord(object sender, RoutedEventArgs e)
        {
            Dialogs.ShowSaveChangesBox();
        }

        public void AutoUpdateDiscord2(object sender, RoutedEventArgs e)
        {
            if (UpdateAuto)
                setStatus();
        }
        #endregion Settings

        #region WarfaceScr 
        private void WarfaceScrCheck(object sender, RoutedEventArgs e)
        {
            HideAllScreens();

            if (WarfaceScr != null)
                Animations.VisibleOn(WarfaceScr).Begin();
        }

        #region Analytics
        public class Match
        {
            public int No { get; set; }
            public string Карта { get; set; }
            public string Начало { get; set; }
            public string Конец { get; set; }
        }
        public class Matches : List<Match>
        {
            public void Add(string map, string begin, string end)
            {
                base.Add(new Match()
                {
                    No = base.Count + 1,
                    Карта = map,
                    Начало = begin,
                    Конец = end
                });
            }
        }
        public Matches matches = new Matches();

        private void AnalyticsDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            AnalyticsDataGrid.ItemsSource = matches;
        }

        public void MatchesSave()
        {
            var res = new JsonObjectArray();

            foreach (var match in matches)
            {
                res.Add(new Json(new
                {
                    no = match.No,
                    map = match.Карта,
                    begin = match.Начало,
                    end = match.Конец
                }));
            }

            File.WriteAllText("matches.json", res.Stringify());
        }

        public void MatchesLoad()
        {
            if (!File.Exists("matches.json"))
                return;

            var js = new Json(File.ReadAllText("matches.json"));

            foreach (var arr in js[0].Value)
            {
                matches.Add(new Match()
                {
                    No = Convert.ToInt32(arr["no"].Value),
                    Карта = arr["map"].Value,
                    Начало = arr["begin"].Value,
                    Конец = arr["end"].Value
                });
            }
        }
        #endregion Analytics

        #region WarfaceProp
        public class WFProp
        {
            public string Параметр { get; set; }
            public string Описание { get; set; }
        }
        public class WFProps : List<WFProp>
        {
            public void Add(string par, string opisanie)
            {
                base.Add(new WFProp()
                {
                    Параметр = par,
                    Описание = opisanie,
                });
            }
        }

        private void WarfacePropDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var thisDG = sender as DataGrid;
            var WarfaceProps = new WFProps();
            thisDG.ItemsSource = WarfaceProps;

            WarfaceProps.Add("{AppName}", "Название приложения");
            WarfaceProps.Add("{AppID}", "ID приложения");
            WarfaceProps.Add("{Map}", "Текущая карта");
            WarfaceProps.Add("{State}", "Текущее состояние игры");
            WarfaceProps.Add("{StateStartTime}", "Время в мс с которого вы находитесь в этом состоянии");
            WarfaceProps.Add("{InGameServerName}", "Имя внутриигрового сервера, например: Ветераны 15");
            WarfaceProps.Add("{ServerIP}", "Адрес сервера");
            WarfaceProps.Add("{ServerName}", "Имя сервера");
            WarfaceProps.Add("{ServerRegion}", "Ваш регион");
            WarfaceProps.Add("{PlayerNickname}", "Ваше игровое имя на текущем сервере");
            WarfaceProps.Add("{PlayerRank}", "Ваш ранк на текущем сервере");
            WarfaceProps.Add("{PlayerRankName}", "Ваш ранк на текущем сервере");
            WarfaceProps.Add("{PlayerUserID}", "Ваш уникальный ID персонажа");
        }
        #endregion WarfaceProp

        #endregion WarfaceScr

        #region Console
        private void ConsoleCheck(object sender, RoutedEventArgs e)
        {
            HideAllScreens();

            if (Console != null)
                Animations.VisibleOn(Console).Begin();
        }

        private void console_TextChanged(object sender, TextChangedEventArgs e)
        {
            console.ScrollToEnd();
        }
        #endregion Console

        #endregion ContentForm Events

        #region Preferences
        public Json preferences = new Json(new
        {
            MyGames = new
            {
                PHPSESSID = "",
                CODE = ""
            },
            Discord = new
            {
                TOKEN = ""
            },
            Visualizer = new
            {
                ndef = new
                {
                    VisualiserGameName = "{AppName} ({ServerName}: {PlayerNickname})",
                    VisualiserApplicationID = "{AppID}",
                    VisualiserState = "Карта: {Map}",
                    VisualiserDetails = "{State}",
                    Visualiser_Timestamp_Start = "{StateStartTime}",
                    Visualiser_Timestamp_End = "",
                    VisualiserPartySize = "",
                    VisualiserPartyMax = "",
                    VisualiserImageLargeKey = "logo",
                    VisualiserImageLargeText = "Warface Status",
                    VisualiserImageSmallKey = "rank{PlayerRank}",
                    VisualiserImageSmallText = "Ранк: {PlayerRank} {PlayerRankName}"
                },
                nclr = empt
            },
            AppData = new
            {
                FirstRun = true,
                AutoRun = false,
                ver = AppVer,
                Volume = 50,
                MainWindowWidth = 800,
                MainWindowHeight = 450,
                MainWindowLeft = 0,
                MainWindowTop = 0,
                MainWindowState = 0,
                CurrentProfile = "ndef",
                NotifyOnNewCase = false,
                NotifyOnNewCase_Sound = "NotifyOnNewCase_Sound.mp3",
                NotifyOnNewVipCase_Sound = "NotifyOnNewVipCase_Sound.mp3",
                StreamStatusDiscord = false
            }
        });

        static dynamic empt = new
        {
            VisualiserGameName = "",
            VisualiserApplicationID = "",
            VisualiserState = "",
            VisualiserDetails = "",
            Visualiser_Timestamp_Start = "",
            Visualiser_Timestamp_End = "",
            VisualiserPartySize = "",
            VisualiserPartyMax = "",
            VisualiserImageLargeKey = "",
            VisualiserImageLargeText = "",
            VisualiserImageSmallKey = "",
            VisualiserImageSmallText = ""
        };

        string CurrentProfile = "ndef";
        private void ProfileCheckedChange(object sender, RoutedEventArgs e)
        {
            CurrentProfile = (sender as RadioButton).Name as string;
            if (Dialogs.IsSaveChangesBoxOpened == true)
                Dialogs.SaveChangesBoxButtonResetClickAction();
            if (IsInitialized)
            {
                Dialogs.DontShowSaveChangesBox = true;
                LoadVisualizerPreferences();
                Dialogs.DontShowSaveChangesBox = false;
            }
            setStatus();
        }

        public void SaveVisualizerPreferences()
        {
            if (CurrentProfile == "ndef" || CurrentProfile == "nclr")
                return;

            if (preferences["Visualizer"].Value.IndexByKey(CurrentProfile) == -1)
                preferences["Visualizer"].Value.Add(new JsonObject(CurrentProfile, new Json(empt)));

            preferences["Visualizer"][CurrentProfile]["VisualiserGameName"].Value = VisualiserGameName.Text;
            preferences["Visualizer"][CurrentProfile]["VisualiserApplicationID"].Value = VisualiserApplicationID.Text;
            preferences["Visualizer"][CurrentProfile]["VisualiserState"].Value = VisualiserState.Text;
            preferences["Visualizer"][CurrentProfile]["VisualiserDetails"].Value = VisualiserDetails.Text;
            preferences["Visualizer"][CurrentProfile]["Visualiser_Timestamp_Start"].Value = Visualiser_Timestamp_Start.Text;
            preferences["Visualizer"][CurrentProfile]["Visualiser_Timestamp_End"].Value = Visualiser_Timestamp_End.Text;
            preferences["Visualizer"][CurrentProfile]["VisualiserPartySize"].Value = VisualiserPartySize.Text;
            preferences["Visualizer"][CurrentProfile]["VisualiserPartyMax"].Value = VisualiserPartyMax.Text;
            preferences["Visualizer"][CurrentProfile]["VisualiserImageLargeKey"].Value = VisualiserImageLargeKey.Text;
            preferences["Visualizer"][CurrentProfile]["VisualiserImageLargeText"].Value = VisualiserImageLargeText.Text;
            preferences["Visualizer"][CurrentProfile]["VisualiserImageSmallKey"].Value = VisualiserImageSmallKey.Text;
            preferences["Visualizer"][CurrentProfile]["VisualiserImageSmallText"].Value = VisualiserImageSmallText.Text;
        }

        public void LoadVisualizerPreferences()
        {
            var tmp = CurrentProfile;
            if (preferences["Visualizer"].Value.IndexByKey(CurrentProfile) == -1)
                tmp = "ndef";

            VisualiserGameName.Text =         preferences["Visualizer"][tmp]["VisualiserGameName"].Value;
            VisualiserApplicationID.Text =    preferences["Visualizer"][tmp]["VisualiserApplicationID"].Value;
            VisualiserState.Text =            preferences["Visualizer"][tmp]["VisualiserState"].Value;
            VisualiserDetails.Text =          preferences["Visualizer"][tmp]["VisualiserDetails"].Value;
            Visualiser_Timestamp_Start.Text = preferences["Visualizer"][tmp]["Visualiser_Timestamp_Start"].Value;
            Visualiser_Timestamp_End.Text =   preferences["Visualizer"][tmp]["Visualiser_Timestamp_End"].Value;
            VisualiserPartySize.Text =        preferences["Visualizer"][tmp]["VisualiserPartySize"].Value;
            VisualiserPartyMax.Text =         preferences["Visualizer"][tmp]["VisualiserPartyMax"].Value;
            VisualiserImageLargeKey.Text =    preferences["Visualizer"][tmp]["VisualiserImageLargeKey"].Value;
            VisualiserImageLargeText.Text =   preferences["Visualizer"][tmp]["VisualiserImageLargeText"].Value;
            VisualiserImageSmallKey.Text =    preferences["Visualizer"][tmp]["VisualiserImageSmallKey"].Value;
            VisualiserImageSmallText.Text =   preferences["Visualizer"][tmp]["VisualiserImageSmallText"].Value;
        }

        public void PreferencesSave()
        {
            preferences["AppData"]["CurrentProfile"].Value = CurrentProfile;
            MatchesSave();
            File.WriteAllText("preferences.json", preferences.Stringify());
        }

        bool IsPreferencesLoaded = false;
        public bool PreferencesLoad()
        {
            IsPreferencesLoaded = false;
            var ShowLogin = true;

            Dialogs.DontShowSaveChangesBox = true;
            if (PreferencesExist())
            {
                var p = new Json(File.ReadAllText("preferences.json"));
                double ver = 0.0;
                try
                {
                    ver = p["AppData"]["ver"].Value;
                }
                catch { }
                if (ver == AppVer)
                    preferences = p;
            }
            CurrentProfile = preferences["AppData"]["CurrentProfile"].Value;
            //try
            //{
                if (!string.IsNullOrEmpty(preferences["MyGames"]["PHPSESSID"].Value))
                {
                    myGames.PHPSESSID = preferences["MyGames"]["PHPSESSID"].Value;
                    myGames.CODE = preferences["MyGames"]["CODE"].Value;
                    Dispatcher.Invoke(() =>
                    {
                        Animations.SetGreenStyle(AuthMyGames, (Style)FindResource("ButtonStyleGreen"));
                    });
                }
                if (!string.IsNullOrEmpty(preferences["Discord"]["TOKEN"].Value))
                {
                    if (Discord.IsTokenValid(preferences["Discord"]["TOKEN"].Value))
                        discord.Token = preferences["Discord"]["TOKEN"].Value;
                    ShowLogin = false;
                    Dispatcher.Invoke(() =>
                    {
                        FinishLogin();
                    });
                }
                new Thread(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        window.Width = preferences["AppData"]["MainWindowWidth"].Value;
                        window.Height = preferences["AppData"]["MainWindowHeight"].Value;
                        if (preferences["AppData"]["MainWindowTop"].Value != 0)
                            Top = preferences["AppData"]["MainWindowTop"].Value;
                        if (preferences["AppData"]["MainWindowLeft"].Value != 0)
                            Left = preferences["AppData"]["MainWindowLeft"].Value;
                        WindowState = (WindowState)preferences["AppData"]["MainWindowState"].Value;
                    });
                    Thread.Sleep(1000);
                    MatchesLoad();
                    Dispatcher.Invoke(() =>
                    {
                        NotifyOnNewCase.IsChecked = preferences["AppData"]["NotifyOnNewCase"].Value.Value;
                        StreamStatusDiscord.IsChecked = preferences["AppData"]["StreamStatusDiscord"].Value.Value;
                        Volume.Value = preferences["AppData"]["Volume"].Value;
                        AutoRun.IsChecked = preferences["AppData"]["AutoRun"].Value.Value;
                        if (CurrentProfile != "ndef")
                            (FindName(CurrentProfile) as RadioButton).IsChecked = true;

                        LoadVisualizerPreferences();
                        //GamePath.Text = preferences["AppData"]["GamePath"].Value;

                        Dialogs.DontShowSaveChangesBox = false;
                    });
                    IsPreferencesLoaded = true;
                }).Start();
            //}
            //catch { }

            return ShowLogin;
        }
        public bool PreferencesExist()
        {
            return File.Exists("preferences.json");
        }
        #endregion Preferences

        #region Status
        public string ReplaceSpecialWords(string str)
        {
            return str
                .Replace("{AppName}", "SWarface")
                .Replace("{AppID}", "735872877148110939")
                .Replace("{Map}", WarfaceGame.config.map)
                .Replace("{State}", WarfaceGame.config.screen)
                .Replace("{InGameServerName}", WarfaceGame.config.server)
                .Replace("{StateStartTime}", WarfaceGame.config.since.ToString())
                .Replace("{ServerIP}", WarfaceGame.config.player.OnlineServer)
                .Replace("{PlayerNickname}", WarfaceGame.config.player.PlayerInfo.Nickname)
                .Replace("{PlayerRank}", WarfaceGame.config.player.PlayerInfo.Rank.ToString())
                .Replace("{PlayerRankName}", WarfaceGame.config.player.PlayerInfo.RankName)
                .Replace("{ServerName}", WarfaceGame.config.player.PlayerInfo.Server)
                .Replace("{ServerRegion}", WarfaceGame.config.player.Region)
                .Replace("{PlayerUserID}", WarfaceGame.config.player.Uid.ToString());
        }

        public void setStatus()
        {
            new Thread(() =>
            {
                string Name = "", Details = "", State = "", Start = "", End = "", ApplicationID = "", ImageLargeKey = "", ImageLargeText = "", ImageSmallKey = "", ImageSmallText = "";
                long PartyMax = 0, PartySize = 0;
                Dispatcher.Invoke(() =>
                {
                    Name = VisualiserGameName.Text;
                    Details = VisualiserDetails.Text;
                    State = VisualiserState.Text;
                    Start = Visualiser_Timestamp_Start.Text;
                    End = Visualiser_Timestamp_End.Text;
                    try
                    {
                        PartyMax = Convert.ToInt64(ReplaceSpecialWords(VisualiserPartyMax.Text));
                        PartySize = Convert.ToInt64(ReplaceSpecialWords(VisualiserPartySize.Text));
                    }
                    catch { }
                    ApplicationID = VisualiserApplicationID.Text;
                    ImageLargeKey = VisualiserImageLargeKey.Text;
                    ImageLargeText = VisualiserImageLargeText.Text;
                    ImageSmallKey = VisualiserImageSmallKey.Text;
                    ImageSmallText = VisualiserImageSmallText.Text;
                });

                Name = ReplaceSpecialWords(Name);
                Details = ReplaceSpecialWords(Details);
                State = ReplaceSpecialWords(State);
                Start = ReplaceSpecialWords(Start);
                End = ReplaceSpecialWords(End);
                ApplicationID = ReplaceSpecialWords(ApplicationID);
                ImageLargeKey = Discord.GetImageIdByName(ReplaceSpecialWords(ImageLargeKey), ApplicationID);
                ImageLargeText = ReplaceSpecialWords(ImageLargeText);
                ImageSmallKey = Discord.GetImageIdByName(ReplaceSpecialWords(ImageSmallKey), ApplicationID);
                ImageSmallText = ReplaceSpecialWords(ImageSmallText);

                c.i("New Status");

                var statusJson = new Json(new
                {
                    op = 3,
                    d = new
                    {
                        status = "online",
                        activities = new[]
                        {
                        new
                        {
                            type = 0,
                            name = Name,
                            assets = new
                            {
                                empty = ""
                            },
                            timestamps = new
                            {
                                empty = ""
                            }
                        }
                    },
                        active = true,
                        since = 0,
                        afk = false
                    }
                });

                if (!string.IsNullOrEmpty(ApplicationID))
                    statusJson["d"]["activities"][0].Add(new JsonObject("application_id", ApplicationID));
                if (!string.IsNullOrEmpty(Details))
                    statusJson["d"]["activities"][0].Add(new JsonObject("details", Details));
                if (!string.IsNullOrEmpty(State))
                    statusJson["d"]["activities"][0].Add(new JsonObject("state", State));
                if (!string.IsNullOrEmpty(ImageLargeKey))
                    statusJson["d"]["activities"][0]["assets"].Value.Add(new JsonObject("large_image", ImageLargeKey));
                if (!string.IsNullOrEmpty(ImageLargeText))
                    statusJson["d"]["activities"][0]["assets"].Value.Add(new JsonObject("large_text", ImageLargeText));
                if (!string.IsNullOrEmpty(ImageSmallKey))
                    statusJson["d"]["activities"][0]["assets"].Value.Add(new JsonObject("small_image", ImageSmallKey));
                if (!string.IsNullOrEmpty(ImageSmallText))
                    statusJson["d"]["activities"][0]["assets"].Value.Add(new JsonObject("small_text", ImageSmallText));
                if (!string.IsNullOrEmpty(Start) && Start != "0")
                    statusJson["d"]["activities"][0]["timestamps"].Value.Add(new JsonObject("start", Start));
                if (!string.IsNullOrEmpty(End) && End != "0")
                    statusJson["d"]["activities"][0]["timestamps"].Value.Add(new JsonObject("end", End));

                if (discord.WebSocket != null)
                    discord.WebSocket.Send(statusJson.ToString());
            }).Start();
        }

        public void clearStatus()
        {
            var statusJson = new Json(new
            {
                op = 3,
                d = new
                {
                    status = "online",
                    activities = new[]
                        {
                        new
                        {
                            type = 0,
                            name = "",
                        }
                    },
                    active = true,
                    since = 0,
                    afk = true
                }
            });
            if (discord.WebSocket != null)
                discord.WebSocket.Send(statusJson.ToString());
        }
        #endregion Status

        #region HelpText
        private void HelpText_MouseEnter(object sender, MouseEventArgs e)
        {
            var help = FindName((sender as FrameworkElement).Name + "_HelpText") as FrameworkElement;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation opacity = new DoubleAnimation();
            opacity.Duration = TimeSpan.FromMilliseconds(120);
            opacity.To = 1;
            storyboard.Children.Add(opacity);

            DoubleAnimation scaleX = new DoubleAnimation();
            scaleX.Duration = TimeSpan.FromMilliseconds(120);
            scaleX.To = 1;
            storyboard.Children.Add(scaleX);

            DoubleAnimation scaleY = new DoubleAnimation();
            scaleY.Duration = TimeSpan.FromMilliseconds(120);
            scaleY.To = 1;
            storyboard.Children.Add(scaleY);

            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
            Storyboard.SetTarget(opacity, help);

            Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(scaleX, help);

            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(scaleY, help);

            storyboard.Begin();
        }

        private void HelpText_MouseLeave(object sender, MouseEventArgs e)
        {
            var help = FindName((sender as FrameworkElement).Name + "_HelpText") as FrameworkElement;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation opacity = new DoubleAnimation();
            opacity.Duration = TimeSpan.FromMilliseconds(120);
            opacity.To = 0;
            storyboard.Children.Add(opacity);

            DoubleAnimation scaleX = new DoubleAnimation();
            scaleX.Duration = TimeSpan.FromMilliseconds(120);
            scaleX.To = 0.8;
            storyboard.Children.Add(scaleX);

            DoubleAnimation scaleY = new DoubleAnimation();
            scaleY.Duration = TimeSpan.FromMilliseconds(120);
            scaleY.To = 0.8;
            storyboard.Children.Add(scaleY);

            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
            Storyboard.SetTarget(opacity, help);

            Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(scaleX, help);

            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(scaleY, help);

            storyboard.Begin();
        }
        #endregion HelpText

        #region Other
        public void HideLoginBackground()
        {
            MainGrid.Background = new SolidColorBrush(Color.FromRgb(54, 57, 63));
            bg_abyss.Visibility = Visibility.Hidden;
            LoginForm.Visibility = Visibility.Hidden;
            CodeForm.Visibility = Visibility.Hidden;
        }

        public void ShowDiscordLogin()
        {
            MainGrid.Background = new ImageBrush(Animations.ImageSourceFromBitmap(Properties.Resources.Background));
            bg_abyss.Visibility = Visibility.Visible;
            LoginForm.Visibility = Visibility.Visible;
        }

        public void FinishLogin()
        {
            preferences["Discord"]["TOKEN"].Value = discord.Token;
            PreferencesSave();
            if (!string.IsNullOrEmpty(discord.Token))
                Animations.SetGreenStyle(AuthDiscord, (Style)FindResource("ButtonStyleGreen"));

            BG_Plug.Margin = new Thickness(-(this.ActualWidth * 1.5), -(this.ActualHeight * 1.5), 0, 0);
            BG_Plug.Visibility = Visibility.Visible;
            var location = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Duration(TimeSpan.FromMilliseconds(1000)));
            location.Completed += (s, ev) =>
            {
                if (preferences["AppData"]["FirstRun"].Value.Value == true)
                {
                    Dialogs.ShowMessageBoxOk("Status Warface", "Спасибо за участие в beta-тесте.");
                    preferences["AppData"]["FirstRun"].Value = false;
                    PreferencesSave();
                }
                HideLoginBackground();
                BG_Plug.Visibility = Visibility.Hidden;
                ContentFormOpen();
            };
            location.FillBehavior = FillBehavior.Stop;
            BG_Plug.BeginAnimation(Window.MarginProperty, location);
        }

        public static void PlaySound(string path)
        {
            try
            {
                mediaPlayer.Open(new Uri(path));
            }
            catch
            {
                mediaPlayer.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\" + path));
            }
            mediaPlayer.Play();
        }
        #endregion Other

        #region Temp
        private void HideNotifyMenu(object sender, MouseButtonEventArgs e)
        {
            NotifyPopup.IsOpen = false;
        }
        #endregion Temp
    }
}
