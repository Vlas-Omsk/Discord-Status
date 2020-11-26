using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Drawing;

using DiscordStatusGUI.Extensions;

namespace DiscordStatusGUI
{
    class MouseHook
    {
        public static void Create()
        {
            CyclicCheckingEventsThread = new System.Timers.Timer(5);
            CyclicCheckingEventsThread.Elapsed += (s, e) => ProcFunction();
            CyclicCheckingEventsThread.AutoReset = true;
            CyclicCheckingEventsThread.Enabled = true;
            CyclicCheckingEventsThread.Start();
        }

        public static void Destroy()
        {
            CyclicCheckingEventsThread.Dispose();
        }

        #region WinAPI
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

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);

        public static bool IsKeyPushedDown(int vKey)
        {
            return 0 != GetAsyncKeyState(vKey);
        }
        #endregion

        #region Simple
        public static System.Timers.Timer CyclicCheckingEventsThread;
        private static Point Point = new Point(), PointOld = new Point();
        private static bool
            Left, LeftOld,
            Right, RightOld;

        private static void ProcFunction()
        {
            try
            {
                Point = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                Left = IsKeyPushedDown((int)MouseButton.Left);
                Right = IsKeyPushedDown((int)MouseButton.Right);

                if (Left != LeftOld)
                    Static.MainWindow.Dispatcher.Invoke(() =>
                    {
                        if (Left == false)
                            OnMouseButtonUp?.Invoke(Point.X, Point.Y, MouseButton.Left);
                        else
                            OnMouseButtonDown?.Invoke(Point.X, Point.Y, MouseButton.Left);
                    });
                if (Right != RightOld)
                    Static.MainWindow.Dispatcher.Invoke(() =>
                    {
                        if (Right == false)
                            OnMouseButtonUp?.Invoke(Point.X, Point.Y, MouseButton.Right);
                        else
                            OnMouseButtonDown?.Invoke(Point.X, Point.Y, MouseButton.Right);
                    });
                if (Point.X != PointOld.X || Point.Y != PointOld.Y)
                    Static.MainWindow.Dispatcher.Invoke(() =>
                        OnMouseMove?.Invoke(Point.X, Point.Y));

                PointOld = Point;
                LeftOld = Left;
                RightOld = Right;
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteLine(ConsoleEx.Warning, ex.ToString());
            }
        }
        #endregion

        #region LowLevel
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, [In] IntPtr lParam);

        static IntPtr hHook = IntPtr.Zero;
        static IntPtr hModule = IntPtr.Zero;
        static bool hookInstall = false;
        static HookProc hookDel;

        public static bool IsHookInstalled => hookInstall && hHook != IntPtr.Zero;

        public static void InstallHook()
        {
            hModule = Marshal.GetHINSTANCE(AppDomain.CurrentDomain.GetAssemblies()[0].GetModules()[0]);
            hookDel = new HookProc(HookProcFunction);

            //hHook = SetWindowsHookEx(HookType.WH_MOUSE,
            //hookDel, IntPtr.Zero, AppDomain.GetCurrentThreadId());
            hHook = SetWindowsHookEx(HookType.WH_MOUSE_LL,
                hookDel, hModule, 0);

            if (hHook != IntPtr.Zero)
            {
                hookInstall = true;
                ConsoleEx.WriteLine(ConsoleEx.Info, "Low level mouse hook installed");
            }
            else
                ConsoleEx.WriteLine(ConsoleEx.Info, "Can't install low level mouse hook!");
        }

        public static void UnInstallHook()
        {
            if (IsHookInstalled)
            {
                if (!UnhookWindowsHookEx(hHook))
                    ConsoleEx.WriteLine(ConsoleEx.Info, "Can't uninstall low level mouse hook!");
                hHook = IntPtr.Zero;
                hModule = IntPtr.Zero;
                hookInstall = false;
                ConsoleEx.WriteLine(ConsoleEx.Info, "Low level mouse hook uninstalled");
            }
        }

        static IntPtr HookProcFunction(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == 0)
            {
                MSLLHOOKSTRUCT mhs = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                switch ((WM)wParam.ToInt32())
                {
                    case WM.MOUSEMOVE:
                        OnMouseMove?.Invoke(mhs.pt.X, mhs.pt.Y);
                        break;
                    case WM.LBUTTONUP:
                        //ConsoleEx.WriteLine("LBUTTONUP", mhs.pt.X + " " + mhs.pt.Y);
                        OnMouseButtonUp?.Invoke(mhs.pt.X, mhs.pt.Y, MouseButton.Left);
                        break;
                    case WM.RBUTTONUP:
                        //ConsoleEx.WriteLine("RBUTTONUP", mhs.pt.X + " " + mhs.pt.Y);
                        OnMouseButtonUp?.Invoke(mhs.pt.X, mhs.pt.Y, MouseButton.Right);
                        break;
                    case WM.MBUTTONUP:
                        //ConsoleEx.WriteLine("MBUTTONUP", mhs.pt.X + " " + mhs.pt.Y);
                        OnMouseButtonUp?.Invoke(mhs.pt.X, mhs.pt.Y, MouseButton.Center);
                        break;
                }
            }

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }
        #endregion


        public delegate void OnMouseButtonClickEventHandler(int x, int y, MouseButton button);
        public static event OnMouseButtonClickEventHandler OnMouseButtonUp;
        public static event OnMouseButtonClickEventHandler OnMouseButtonDown;

        public delegate void OnMouseMoveEventHandler(int x, int y);
        public static event OnMouseMoveEventHandler OnMouseMove;
    }

    enum MouseButton
    {
        Left = 1,
        Right = 2,
        Center = 4,
        XBUTTON1 = 5,
        XBUTTON2 = 6
    }
}
