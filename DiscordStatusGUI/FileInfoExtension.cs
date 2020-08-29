using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace WarfaceStatus
{
    public class FileInfo
    {
        public FileInfo(string path)
        {
            Info = new System.IO.FileInfo(path);
        }

        public System.IO.FileInfo Info;

        public delegate void ChangeEventHandler(ChangeEventArgs e);
        public event ChangeEventHandler OnChange;

        public string ReadText()
        {
            var path = Info.FullName;
            File.Copy(path, path + "temp", true);
            var text = File.ReadAllText(path + "temp");
            File.Delete(path + "temp");
            return text;
        }

        public string[] ReadLines()
        {
            var path = Info.FullName;
            File.Copy(path, path + "temp", true);
            var text = File.ReadAllLines(path + "temp");
            File.Delete(path + "temp");
            return text;
        }

        private System.Timers.Timer _EventTimer;
        public void StartTimer()
        {
            _EventTimer = new System.Timers.Timer(200);
            var prew = Info.LastWriteTimeUtc;
            _EventTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                Info.Refresh();
                if (prew != Info.LastWriteTimeUtc)
                {
                    OnChange?.Invoke(new ChangeEventArgs(Info, this));
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
    public class ChangeEventArgs
    {
        public ChangeEventArgs(System.IO.FileInfo Info, FileInfo Sender)
        {
            this.Info = Info;
            this.Sender = Sender;
        }

        public FileInfo Sender;
        public System.IO.FileInfo Info;
    }
}
