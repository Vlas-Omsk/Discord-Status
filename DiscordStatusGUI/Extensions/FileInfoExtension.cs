using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace DiscordStatusGUI.Extensions
{
    public class FileInfoEx
    {
        public FileInfoEx(string path)
        {
            Info = new FileInfo(path);
        }

        public FileInfo Info;

        private System.Timers.Timer _EventTimer;

        public event EventHandler<TextChangedEventArgsEx> TextChanged;

        public string SafeReadText() => SafeReadText(Info.FullName);
        public string[] SafeReadLines() => SafeReadLines(Info.FullName);

        public static string SafeReadText(string path)
        {
            File.Copy(path, path + "temp", true);
            var text = File.ReadAllText(path + "temp");
            File.Delete(path + "temp");
            return text;
        }

        public static string[] SafeReadLines(string path)
        {
            File.Copy(path, path + "temp", true);
            var text = File.ReadAllLines(path + "temp");
            File.Delete(path + "temp");
            return text;
        }

        public void StartTimer()
        {
            _EventTimer = new System.Timers.Timer(200);
            var prew = Info.LastWriteTimeUtc;
            _EventTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                Info.Refresh();
                if (prew != Info.LastWriteTimeUtc)
                {
                    Static.InvokeAsync(TextChanged, new TextChangedEventArgsEx(Info), this);
                }
                prew = Info.LastWriteTimeUtc;
            };
            _EventTimer.AutoReset = true;
            _EventTimer.Enabled = true;
            _EventTimer.Start();
        }

        public void StopTimer()
        {
            _EventTimer.Stop();
        }
    }
    public class TextChangedEventArgsEx
    {
        public TextChangedEventArgsEx(System.IO.FileInfo info)
        {
            this.Info = info;
        }

        public System.IO.FileInfo Info;
    }
}
