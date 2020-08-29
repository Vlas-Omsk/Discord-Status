using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += (a, b) => { c.save(); };

            var proc = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).OfType<Process>().ToList();
            if (proc.Count > 1)
            {
                foreach (var p in proc)
                {
                    ShowWindow(p.MainWindowHandle, 1);
                    SetForegroundWindow(p.MainWindowHandle);
                }
                Shutdown();
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            crit(e.ExceptionObject as Exception);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            crit(e.Exception);
        }

        void crit (Exception ex)
        {
            c.crit($"\r\n-----------------------BEGIN-------------------------" +
                   $"\r\n[MESSAGE]\r\n{ex.Message}" +
                   $"\r\n[STACK TRACE]\r\n{ex.StackTrace}" +
                   $"\r\n[SOURCE]\r\n{ex.Source}" +
                   $"\r\n[TARGET SITE]\r\n{ex.TargetSite}" +
                   $"\r\n[HRESULT]\r\n{ex.HResult}" +
                   $"\r\n[DATA]\r\n{string.Join("\r\n", ex.Data)}" +
                   $"\r\n[HELP LINK]\r\n{ex.HelpLink}" +
                   $"\r\n-------------------------END-------------------------");
        }
    }
}
