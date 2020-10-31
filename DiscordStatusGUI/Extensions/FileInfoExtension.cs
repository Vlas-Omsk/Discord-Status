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

        public delegate void OnTextChangedEventHandler(OnTextChangedEventArgs e);
        public event OnTextChangedEventHandler OnTextChanged;
        
        public string SafeReadText()
        {
            var path = Info.FullName;
            File.Copy(path, path + "temp", true);
            var text = File.ReadAllText(path + "temp");
            File.Delete(path + "temp");
            return text;
        }

        public string[] SafeReadLines()
        {
            var path = Info.FullName;
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
                    OnTextChanged?.Invoke(new OnTextChangedEventArgs(Info, this));
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
    public class OnTextChangedEventArgs
    {
        public OnTextChangedEventArgs(System.IO.FileInfo info, FileInfoEx sender)
        {
            this.Info = info;
            this.Sender = sender;
        }

        public FileInfoEx Sender;
        public System.IO.FileInfo Info;
    }
}
