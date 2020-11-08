using DiscordStatusGUI.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace DiscordStatusGUI
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        //[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool SetDllDirectory(string lpPathName);
        //var dllDirectory = CurrentDir + "\\libs";
        //SetDllDirectory(dllDirectory);
        //Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);
        //Environment.SetEnvironmentVariable("Path", Environment.GetEnvironmentVariable("Path") + ";" + CurrentDir + @"\libs");
        //ConsoleEx.WriteLine("Info", "Libs directory: " + CurrentDir + @"\libs");

        public App()
        {
            Console.Write($"[{ConsoleEx.Info}][{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffzzZ}]   STARTED\r\n");

            CheckCopy();

            string CurrentExe = Environment.GetCommandLineArgs()[0];
            string CurrentDir = Path.GetDirectoryName(CurrentExe);

            ConsoleEx.InitLogger();

            Directory.SetCurrentDirectory(CurrentDir);
            ConsoleEx.WriteLine(ConsoleEx.Info, "Working directory: " + Directory.GetCurrentDirectory());

            Locales.Lang.Init();
            Static.Init();
        }

        private void CheckCopy()
        {
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            while (processes.Length > 1)
                Thread.Sleep(100);

            ProcessEx.OnProcessOpened += ProcessEx_OnProcessOpened;
        }

        private static void ProcessEx_OnProcessOpened(List<Process> processes)
        {
            Process ProcessCopy = null, current = Process.GetCurrentProcess();
            foreach (var process in processes)
                if (process.ProcessName == current.ProcessName && process.Id != current.Id)
                {
                    ProcessCopy = process;
                    break;
                }
            if (ProcessCopy is null)
                return;

            var cmdline = ProcessEx.GetProcessCommandLine(ProcessCopy.Id, out _);
            ConsoleEx.WriteLine(ConsoleEx.Info, $"\r\n    [{ProcessCopy.Id}] {ProcessCopy.ProcessName}\r\n    {cmdline}");
            ProcessCopy.Kill();
            var splittedcmdline = ProcessEx.SplitParams(cmdline);
            Preferences.SetPropertiesByCmdLine(splittedcmdline);
        }
    }
}
