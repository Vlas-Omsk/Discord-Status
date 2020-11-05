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

        public App()
        {
            string CurrentExe = Environment.GetCommandLineArgs()[0];
            string CurrentDir = System.IO.Path.GetDirectoryName(CurrentExe);

            //var dllDirectory = CurrentDir + "\\libs";
            //SetDllDirectory(dllDirectory);
            //Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);

            Directory.SetCurrentDirectory(CurrentDir);

            ConsoleEx.WriteLine("Info", "Working directory: " + Directory.GetCurrentDirectory());

            //Environment.SetEnvironmentVariable("Path", Environment.GetEnvironmentVariable("Path") + ";" + CurrentDir + @"\libs");
            //ConsoleEx.WriteLine("Info", "Libs directory: " + CurrentDir + @"\libs");

            Locales.Lang.Init();
        }
    }
}
