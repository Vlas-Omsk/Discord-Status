using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using PinkJson;

namespace DiscordStatusGUI.Extensions
{
    public class ConsoleEx
    {
        public static Color BlueColor = new Color(63, 61, 200);

        public static readonly (string, Color) 
            Info = ("Info", BlueColor),
            DiscordWebSocket = ("DiscordWebSocket", BlueColor),
            WebSocketServer = ("WebSocketServer", System.Drawing.Color.Green),
            WarfaceApi = ("WarfaceApi", BlueColor),
            WarfaceStringParser = ("WarfaceStringParser", BlueColor),
            UpdateManager = ("UpdateManager", BlueColor),
            Warning = ("Warning", System.Drawing.Color.Red),
            Empty = ("", System.Drawing.Color.DarkGray);

        public static StreamWriter LogFileWriter;

        private static bool isConsole = false;
        private static string
            whiteColor = ((Color)System.Drawing.Color.White).ToAnsiForegroundEscapeCode(),
            darkGrayColor = ((Color)System.Drawing.Color.DarkGray).ToAnsiForegroundEscapeCode();
        private static string
            lastcontent = null,
            lastdatetime = null;
        private static int lastline = 0;
        private static Thread ConsoleReaderThread = new Thread(ConsoleReader) { IsBackground = true };

        public static void InitLogger()
        {
            SyntaxHighlighting.EnableVirtualTerminalProcessing();
            
            try { Console.SetCursorPosition(0, 0); isConsole = true; } catch { }
            LogFileWriter = new StreamWriter("latest.log");
            
            if (isConsole)
                ConsoleReaderThread.Start();

            WriteLogo();
        }

        public static void WriteLine((string, Color) prefix, string content)
        {
            var datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzZ");
            LogFileWriter.Write($"[{prefix.Item1}][{datetime}]   {content}\r\n");
            LogFileWriter.Flush();

            if (isConsole)
            {
                var strPrefix = $"{prefix.Item2.ToAnsiForegroundEscapeCode()}[{prefix.Item1}]{darkGrayColor}";

                if (lastcontent == content)
                {
                    Console.CursorTop = lastline;
                    Console.Write($"{strPrefix}[{lastdatetime} - {datetime}]   {whiteColor}{content}\r\n");
                }
                else
                    Console.Write($"{strPrefix}[{datetime}]   {whiteColor}{content}\r\n");

                lastline = Console.CursorTop - 1;
                if (lastcontent != content)
                    lastdatetime = datetime;
                lastcontent = content;
            }
        }

        public static void WriteLogo()
        {
            var DiscordStatus =
            @" ____            Don't Worry Be Happy        __      ____    __             __                      " + Environment.NewLine +
            @"/\  _`\   __                 =)             /\ \    /\  _`\ /\ \__         /\ \__                   " + Environment.NewLine +
            @"\ \ \/\ \/\_\    ____    ___    ___   _ __  \_\ \   \ \,\L\_\ \ ,_\    __  \ \ ,_\  __  __    ____  " + Environment.NewLine +
            @" \ \ \ \ \/\ \  /',__\  /'___\ / __`\/\`'__\/'_` \   \/_\__ \\ \ \/  /'__`\ \ \ \/ /\ \/\ \  /',__\ " + Environment.NewLine +
            @"  \ \ \_\ \ \ \/\__, `\/\ \__//\ \L\ \ \ \//\ \L\ \    /\ \L\ \ \ \_/\ \L\.\_\ \ \_\ \ \_\ \/\__, `\" + Environment.NewLine +
            @"   \ \____/\ \_\/\____/\ \____\ \____/\ \_\\ \___,_\   \ `\____\ \__\ \__/.\_\\ \__\\ \____/\/\____/" + Environment.NewLine +
            @"    \/___/  \/_/\/___/  \/____/\/___/  \/_/ \/__,_ /    \/_____/\/__/\/__/\/_/ \/__/ \/___/  \/___/ ";
            WriteLine(Empty, $"{Static.Title} v{Static.Version.ToString().Replace(',', '.')}\r\n" +
                $"\r\n{DiscordStatus}\r\n");
        }

        public static void ConsoleReader()
        {
            while (true)
            {
                Console.Write("\x1b[K");
                switch (Console.ReadLine())
                {
                    case "test1":
                        WriteLine(Warning, "Test1");
                        break;
                    case "test2":
                        WriteLine(Info, "Test2");
                        break;
                }
            }
        }
    }
}