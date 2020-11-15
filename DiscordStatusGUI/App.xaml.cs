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

            InitializeComponent();

            Locales.Lang.Init();
            Static.Init();

            Preferences.OpenLocalServer();
        }

        private void CheckCopy()
        {
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            while (processes.Length > 1)
                Thread.Sleep(100);

            ProcessEx.OnProcessOpened += ProcessEx_OnProcessOpened;
        }

        private static void ProcessEx_OnProcessOpened(Processes processes)
        {
            Process current = Process.GetCurrentProcess(), ProcessCopy = processes.GetProcessesByNames(current.ProcessName).FirstOrDefault();
            if (ProcessCopy == null || ProcessCopy.HasExited)
                return;

            var cmdline = ProcessEx.GetProcessCommandLine(ProcessCopy.Id, out _);
            ConsoleEx.WriteLine(ConsoleEx.Info, $"\r\n    [{ProcessCopy.Id}] {ProcessCopy.ProcessName}\r\n    {cmdline}");
            ProcessCopy.Kill();
            var splittedcmdline = ProcessEx.SplitParams(cmdline);
            Preferences.SetPropertiesByCmdLine(splittedcmdline);
        }
    }
}
