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
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);
        #endregion

        #region SimpleHook
        public static System.Timers.Timer CyclicCheckingEventsThread;
        private static Point Point = new Point(), PointOld = new Point();
        private static bool
            Left, LeftOld,
            Right, RightOld;

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

        public static bool IsKeyPushedDown(int vKey)
        {
            return 0 != GetAsyncKeyState(vKey);
        }

        private static void ProcFunction()
        {
            Point = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            Left = IsKeyPushedDown((int)MouseButton.Left);
            Right = IsKeyPushedDown((int)MouseButton.Right);

            if (Left != LeftOld)
                if (Left == false)
                    Static.InvokeAsync(OnMouseButtonUp, new MouseButtonEventArgsEx(Point.X, Point.Y, MouseButton.Left));
                else
                    Static.InvokeAsync(OnMouseButtonDown, new MouseButtonEventArgsEx(Point.X, Point.Y, MouseButton.Left));
            if (Right != RightOld)
                if (Right == false)
                    Static.InvokeAsync(OnMouseButtonUp, new MouseButtonEventArgsEx(Point.X, Point.Y, MouseButton.Right));
                else
                    Static.InvokeAsync(OnMouseButtonDown, new MouseButtonEventArgsEx(Point.X, Point.Y, MouseButton.Right));
            if (Point.X != PointOld.X || Point.Y != PointOld.Y)
                Static.InvokeAsync(OnMouseMove, new MouseEventArgsEx(Point.X, Point.Y));

            PointOld = Point;
            LeftOld = Left;
            RightOld = Right;
        }
        #endregion

        public static event EventHandler<MouseButtonEventArgsEx> OnMouseButtonUp;
        public static event EventHandler<MouseButtonEventArgsEx> OnMouseButtonDown;

        public static event EventHandler<MouseEventArgsEx> OnMouseMove;
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
