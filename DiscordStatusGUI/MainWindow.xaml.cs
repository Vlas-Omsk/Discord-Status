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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Runtime.InteropServices;
using System.Windows.Interop;
using DiscordStatusGUI.ViewModels;
using System.Threading;
using System.Diagnostics;
using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.Libs;
using System.Text.RegularExpressions;
using PinkJson.Parser;
using System.IO;

namespace DiscordStatusGUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            RegistryCommands.CreateProtocol();

            Static.MainWindow = this;
            Static.MainWindowViewModel = new MainWindowViewModel();

            InitializeComponent();

            DataContext = Static.MainWindowViewModel;
            Title = Static.Titile;
            Icon = Static.Icon;

#if DEBUG == false
            InstallHook();
#endif
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeNoBorderWindow();

            Static.Discord.Socket.OnWorkingStatusChanged += Socket_OnWorkingStatusChanged;

            await Task.Run(() =>
            {
                Preferences.LoadProfiles();
                Preferences.Load();
                Preferences.SetPropertiesByCmdLine(Environment.GetCommandLineArgs());
            });
            
            await Task.Run(() =>
            {
                WarfaceApi.Init();
                DiscordStiller.Init();
                ProcessEx.Init();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnInstallHook();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Preferences.Save();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Preferences.Save();
        }


        private void Socket_OnWorkingStatusChanged(string msg)
        {
            Static.Window.SetTopStatus(msg);
        }

#region NoBorderWindow
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

        private void InitializeNoBorderWindow()
        {
            WindowInteropHelper h = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(h.Handle);
            Handle = h.Handle;
            source.AddHook(new HwndSourceHook(WndProc));//регистрируем обработчик сообщений
            xborder = GetSystemMetrics(SM_CXSIZEFRAME);
            yborder = GetSystemMetrics(SM_CYSIZEFRAME);
        }
#endregion Window Style

#region MouseHook
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, [In] IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        public enum WM
        {
            LBUTTONDOWN = 0x201,
            LBUTTONUP = 0x202,
            MOUSEMOVE = 0x0200,
            MOUSEWHEEL = 0x020A,
            RBUTTONDOWN = 0x0204,
            RBUTTONUP = 0x0205,
            MBUTTONUP = 0x208,
            MBUTTONDOWN = 0x207,
            XBUTTONDOWN = 0x20B,
            XBUTTONUP = 0x20C
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn,
        IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, [In] IntPtr lParam);

        static IntPtr hHook = IntPtr.Zero;
        static IntPtr hModule = IntPtr.Zero;
        static bool hookInstall = false;
        static HookProc hookDel;

        public void InstallHook()
        {
            hModule = Marshal.GetHINSTANCE(AppDomain.CurrentDomain.GetAssemblies()[0].GetModules()[0]);
            hookDel = new HookProc(HookProcFunction);

            //hHook = SetWindowsHookEx(HookType.WH_MOUSE,
                //hookDel, IntPtr.Zero, AppDomain.GetCurrentThreadId());
            hHook = SetWindowsHookEx(HookType.WH_MOUSE_LL,
                hookDel, hModule, 0);

            if (hHook != IntPtr.Zero)
                hookInstall = true;
            else
                throw new Exception("Can't install low level mouse hook!");
        }

        public static bool IsHookInstalled
        {
            get { return hookInstall && hHook != IntPtr.Zero; }
        }

        public static void UnInstallHook()
        {
            if (IsHookInstalled)
            {
                if (!UnhookWindowsHookEx(hHook))
                    throw new Exception("Can't uninstall low level mouse hook!");
                hHook = IntPtr.Zero;
                hModule = IntPtr.Zero;
                hookInstall = false;
            }
        }

        IntPtr HookProcFunction(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == 0)
            {
                MSLLHOOKSTRUCT mhs = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                switch ((WM)wParam.ToInt32())
                {
                    case WM.MOUSEMOVE:
                        Static.MouseMove(mhs.pt.X, mhs.pt.Y);
                        break;
                    case WM.LBUTTONUP:
                        ConsoleEx.WriteLine("LBUTTONUP", mhs.pt.X + " " + mhs.pt.Y);
                        Static.MouseButtonClick(mhs.pt.X, mhs.pt.Y, MouseButton.Left);
                        break;
                    case WM.RBUTTONUP:
                        ConsoleEx.WriteLine("RBUTTONUP", mhs.pt.X + " " + mhs.pt.Y);
                        Static.MouseButtonClick(mhs.pt.X, mhs.pt.Y, MouseButton.Right);
                        break;
                    case WM.MBUTTONUP:
                        ConsoleEx.WriteLine("MBUTTONUP", mhs.pt.X + " " + mhs.pt.Y);
                        Static.MouseButtonClick(mhs.pt.X, mhs.pt.Y, MouseButton.Middle);
                        break;
                }
            }

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }
#endregion

        public void ReplaceWithWaves(UserControl newControl)
        {
            var save = Static.MainWindowViewModel._CurrentPage;
            Static.MainWindowViewModel._CurrentPage = newControl;
            Static.MainWindowViewModel.OnPropertyChanged("CurrentPage");
            if (save != null)
                Container.Children.Add(save);

            Animations.ReplaceWithWaves(Container, save, newControl, true);
        }
    }
}
