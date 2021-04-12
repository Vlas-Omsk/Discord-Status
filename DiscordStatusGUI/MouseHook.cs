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
        #region WinAPI
        [StructLayout(LayoutKind.Sequential)]
        struct POINT
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

        enum HookType : int
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

        enum WM
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
        static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, [In] IntPtr lParam);
        #endregion

        #region LowLevel
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, [In] IntPtr lParam);

        static IntPtr hHook = IntPtr.Zero;
        static IntPtr hModule = IntPtr.Zero;
        static bool hookInstall = false;
        static HookProc hookDel;
        static Thread hookThread;

        public static bool IsHookInstalled => hookInstall && hHook != IntPtr.Zero;

        public static void InstallHook()
        {
            hModule = Marshal.GetHINSTANCE(AppDomain.CurrentDomain.GetAssemblies()[0].GetModules()[0]);
            hookDel = new HookProc(HookProcFunction);

            hHook = SetWindowsHookEx(HookType.WH_MOUSE_LL,
                hookDel, IntPtr.Zero, 0);

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
            var result = CallNextHookEx(hHook, nCode, wParam, lParam);

            if (nCode == 0)
            {
                MSLLHOOKSTRUCT mhs = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                switch ((WM)wParam.ToInt32())
                {
                    case WM.MOUSEMOVE:
                        //OnMouseMove?.BeginInvoke(null, new MouseEventArgsEx(mhs.pt.X, mhs.pt.Y), null, null);
                        InvokeAsync(OnMouseMove, new MouseEventArgsEx(mhs.pt.X, mhs.pt.Y));
                        break;
                    case WM.LBUTTONUP:
                        //ConsoleEx.WriteLine("LBUTTONUP", mhs.pt.X + " " + mhs.pt.Y);
                        //OnMouseButtonUp?.BeginInvoke(null, new MouseButtonEventArgsEx(mhs.pt.X, mhs.pt.Y, MouseButton.Left), null, null);
                        InvokeAsync(OnMouseButtonUp, new MouseButtonEventArgsEx(mhs.pt.X, mhs.pt.Y, MouseButton.Left));
                        break;
                    case WM.RBUTTONUP:
                        //ConsoleEx.WriteLine("RBUTTONUP", mhs.pt.X + " " + mhs.pt.Y);
                        //OnMouseButtonUp?.BeginInvoke(null, new MouseButtonEventArgsEx(mhs.pt.X, mhs.pt.Y, MouseButton.Right), null, null);
                        InvokeAsync(OnMouseButtonUp, new MouseButtonEventArgsEx(mhs.pt.X, mhs.pt.Y, MouseButton.Right));
                        break;
                    case WM.MBUTTONUP:
                        //ConsoleEx.WriteLine("MBUTTONUP", mhs.pt.X + " " + mhs.pt.Y);
                        //OnMouseButtonUp?.BeginInvoke(null, new MouseButtonEventArgsEx(mhs.pt.X, mhs.pt.Y, MouseButton.Center), null, null);
                        InvokeAsync(OnMouseButtonUp, new MouseButtonEventArgsEx(mhs.pt.X, mhs.pt.Y, MouseButton.Center));
                        break;
                }
            }

            return result;
        }
        #endregion

        public static event EventHandler<MouseButtonEventArgsEx> OnMouseButtonUp;
        public static event EventHandler<MouseButtonEventArgsEx> OnMouseButtonDown;

        public static event EventHandler<MouseEventArgsEx> OnMouseMove;

        private static void InvokeAsync<T>(EventHandler<T> handler, T args)
        {
            if (handler != null)
            {
                var eventListeners = handler.GetInvocationList();

                for (int index = 0; index < eventListeners.Length; index++)
                {
                    var methodToInvoke = (EventHandler<T>)eventListeners[index];
                    methodToInvoke.BeginInvoke(null, args, null, null);
                }
            }
        }
    }

    class MouseButtonEventArgsEx : MouseEventArgsEx
    {
        public MouseButtonEventArgsEx(int x, int y, MouseButton mouseButton) : base(x, y)
        {
            MouseButton = mouseButton;
        }

        public MouseButton MouseButton;
    }

    class MouseEventArgsEx : EventArgs
    {
        public MouseEventArgsEx(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;
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
