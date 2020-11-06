using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.Extensions
{
    public class ConsoleEx
    {
        public const string Info = "Info";
        public const string WebSocket = "DiscordWebSocket";
        public const string Warning = "Warning";
        public const string Warface = "Warface";
        public const string Message = "Message";
        public const string WarfaceStringParser = "StringParser";

        public static void WriteLine(string prefix, string content)
        {
            ColoredWriteLine(prefix, content);
        }

        public static void ColoredWriteLine(string prefix, string content, ConsoleColor prefixcolor = ConsoleColor.DarkGray, ConsoleColor contentcolor = ConsoleColor.White)
        {
            Console.ForegroundColor = prefixcolor;
            Console.Write($"[{prefix}][{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffzzZ}]   ");
            Console.ForegroundColor = contentcolor;
            Console.WriteLine($"{content}");
        }

        public static void InitLogger()
        {
            var stringWriter = new StreamWriter("latest.log") { AutoFlush = true };
            var consoleWriter = Console.Out;

            Console.SetOut(new MultiWriter(stringWriter, consoleWriter));
            WriteLine("", $"{Static.Titile} v{Assembly.GetExecutingAssembly().GetName().Version}");

            var DiscordStatus =
            @" ____                                        __      ____    __             __                      " + Environment.NewLine +
            @"/\  _`\   __                                /\ \    /\  _`\ /\ \__         /\ \__                   " + Environment.NewLine +
            @"\ \ \/\ \/\_\    ____    ___    ___   _ __  \_\ \   \ \,\L\_\ \ ,_\    __  \ \ ,_\  __  __    ____  " + Environment.NewLine +
            @" \ \ \ \ \/\ \  /',__\  /'___\ / __`\/\`'__\/'_` \   \/_\__ \\ \ \/  /'__`\ \ \ \/ /\ \/\ \  /',__\ " + Environment.NewLine +
            @"  \ \ \_\ \ \ \/\__, `\/\ \__//\ \L\ \ \ \//\ \L\ \    /\ \L\ \ \ \_/\ \L\.\_\ \ \_\ \ \_\ \/\__, `\" + Environment.NewLine +
            @"   \ \____/\ \_\/\____/\ \____\ \____/\ \_\\ \___,_\   \ `\____\ \__\ \__/.\_\\ \__\\ \____/\/\____/" + Environment.NewLine +
            @"    \/___/  \/_/\/___/  \/____/\/___/  \/_/ \/__,_ /    \/_____/\/__/\/__/\/_/ \/__/ \/___/  \/___/ ";
            Console.WriteLine($"\r\n{DiscordStatus}\r\n");
        }
    }

    public class MultiWriter : TextWriter
    {
        private readonly List<TextWriter> _writers;

        public MultiWriter(params TextWriter[] writers)
        {
            _writers = new List<TextWriter>(writers);
        }

        public override void Write(char value)
        {
            _writers.ForEach(_ => _.Write(value));
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
