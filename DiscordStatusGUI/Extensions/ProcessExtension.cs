using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;

namespace DiscordStatusGUI.Extensions
{
    class ProcessEx
    {
        [DllImport(@"DiscordStatusGUI.CPP.dll")]
        static extern IntPtr GetProcessCommandLineByPid(int pid, out long ntstatus);

        public static string GetProcessCommandLine(int pid, out long ntstatus)
        {
            return Marshal.PtrToStringAnsi(GetProcessCommandLineByPid(pid, out ntstatus));
        }

        public static string GetOutput(string path, string args, bool hidewindow = true)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = path;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            if (hidewindow)
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();

            using (StreamReader sr = proc.StandardOutput)
            {
                return sr.ReadToEnd();
            }
        }

        public static string[] SplitParams(string str)
        {
            var result = new List<string>();
            var locked = false;
            var p = "";
            for (var i = 0; i < str.Length; i++)
            {
                p += str[i];
                if (str[i] == '\"') locked = !locked;
                if (str[i] == ' ' && !locked)
                {
                    result.Add(p.Trim());
                    p = "";
                }
            }
            if (!string.IsNullOrEmpty(p.Trim()))
                result.Add(p.Trim());
            return result.ToArray();
        }


        public static void Init()
        {
            _ProcessTrackingThread = new Thread(() =>
            {
                while (true)
                {
                    _ProcessTracking_Update();
                    Thread.Sleep(3000);
                }
            })
            { IsBackground=true };
            _ProcessTrackingThread.Start();

            _ForegroundWindowTrackingThread = new Thread(() =>
            {
                while (true)
                {
                    _ForegroundWindowTracking_Update();
                    Thread.Sleep(3000);
                }
            })
            { IsBackground = true };
            _ForegroundWindowTrackingThread.Start();

            Static.InitializationSteps.IsProcessExInitialized = true;
        }

        #region ProcessTracking
        private static Thread _ProcessTrackingThread;
        private static Process[] _LatestProcessList;

        private static void _ProcessTracking_Update()
        {
            var processes = Process.GetProcesses();
            var openedprocess = new Processes();
            var closedprocess = new Processes();

            for (var i = 0; i < processes?.Length; i++)
            {
                try
                {
                    if (processes[i].StartTime > DateTime.Now - TimeSpan.FromMilliseconds(1100) || _LatestProcessList != null)
                        openedprocess.Add(processes[i]);
                }
                catch { }
            }
            for (var i = 0; i < _LatestProcessList?.Length; i++)
            {
                try
                {
                    if (_LatestProcessList[i].HasExited)
                        closedprocess.Add(_LatestProcessList[i]);
                }
                catch { }
            }

            if (closedprocess.Count != 0)
                OnProcessClosed?.Invoke(closedprocess);
            if (openedprocess.Count != 0)
                OnProcessOpened?.Invoke(openedprocess);

            _LatestProcessList = processes;
        }


        public delegate void OnProcessStartedEventHandler(Processes processes);
        public static event OnProcessStartedEventHandler OnProcessOpened;

        public delegate void OnProcessClosedEventHandler(Processes processes);
        public static event OnProcessClosedEventHandler OnProcessClosed;
        #endregion

        #region ForegroundWindowTracking
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);


        private static Thread _ForegroundWindowTrackingThread;
        private static IntPtr _LatestForegroundWindow;

        public static Process ForegroundWindowProcess;

        public static void _ForegroundWindowTracking_Update()
        {
            var window = GetForegroundWindow();

            int pid;
            GetWindowThreadProcessId(window, out pid);
            if (_LatestForegroundWindow != window || ForegroundWindowProcess?.MainWindowTitle != Process.GetProcessById(pid)?.MainWindowTitle)
                try
                {
                    OnForegroundWindowChanged?.Invoke(ForegroundWindowProcess = Process.GetProcessById(pid));
                } catch
                {
                    OnForegroundWindowChanged?.Invoke(ForegroundWindowProcess = new Process());
                }


            _LatestForegroundWindow = window;
        }


        public delegate void OnForegroundWindowChangedEventHandler(Process process);
        public static event OnForegroundWindowChangedEventHandler OnForegroundWindowChanged;
        #endregion
    }

    class Processes : List<Process>
    {
        public Processes GetProcessesByNames(params string[] name)
        {
            var current = Process.GetCurrentProcess();
            var processes = new Processes();
            foreach (var process in this)
                try
                {
                    if (name.Contains(process.ProcessName) && process.Id != current.Id)
                        processes.Add(process);
                }
                catch { }
            return processes;
        }
    }
}
