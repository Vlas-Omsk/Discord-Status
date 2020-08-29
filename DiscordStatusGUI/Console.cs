using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace DiscordStatusGUI
{
    public class c
    {
        public static void init(TextBox element)
        {
            if (File.Exists("logs\\latest.log"))
                if (File.ReadAllText("logs\\latest.log").IndexOf("[CRITICAL ERROR]") != -1)
                    CRITICAL = File.ReadAllText("logs\\latest.log");

            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            else
            {
                if (File.Exists("logs\\latest.log"))
                    File.Move("logs\\latest.log", "logs\\" + File.GetCreationTime("logs\\latest.log").ToString("yyyy-MM-ddTHH-mm-ss.fffzzZ") + ".log");
            }

            logFile = File.OpenWrite("logs\\latest.log");

            Element = element;
        }
        private static TextBox Element;
        private static FileStream logFile;
        public static string CRITICAL;

        public static void i(string content)
        {
            u("INFO", content);
        }

        public static void w(string content)
        {
            u("WARN", content);
        }

        public static void wf(string content)
        {
            u("Warface", content);
        }

        public static void u(string header, string content)
        {
            content = $"\r\n[{header}][{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzZ")}]   {content}";

            if (Element != null)
                Element.Dispatcher.Invoke(() =>
                    Element.Text += content );

            if (logFile != null)
                logFile.Write(Encoding.UTF8.GetBytes(content), 0, Encoding.UTF8.GetBytes(content).Length);
        }

        public static void save()
        {
            if (logFile != null)
                logFile.Close();

            try
            {
                logFile = File.OpenWrite("logs\\latest.log");
            }
            catch { }
        }

        public static void crit(string content)
        {
            if (logFile != null)
                logFile.Close();

            content = $"\r\n[CRITICAL ERROR][{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzZ")}]   {content}";

            File.AppendAllText("logs\\latest.log", content);
        }
    }
}
