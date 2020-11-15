using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using DiscordStatusGUI.Extensions;
using PinkJson.Parser;

namespace DiscordStatusGUI
{
    class DiscordStiller
    {
        [DllImport(@"DiscordStatusGUI.CPP.dll")]
        static extern IntPtr GetDiscordToken(int pid, int skip);

        public static bool TryGetDiscordToken(int pid, out string token)
        {
            token = Marshal.PtrToStringAnsi(GetDiscordToken(pid, 4));

            return !string.IsNullOrEmpty(token);
        }


        static Thread StillDiscordToken_Thread;

        public static void Init()
        {
            ProcessEx.OnProcessOpened += ProcessEx_OnProcessOpened;
        }

        private static void ProcessEx_OnProcessOpened(Processes processes)
        {
            var discord = processes.GetProcessesByNames("Discord");

            if (discord != null && discord.Count == 0)
                return;

            StillDiscordToken(discord);
        }

        public static void StillDiscordToken(Processes processes)
        {
            if (StillDiscordToken_Thread?.IsAlive != null && StillDiscordToken_Thread.IsAlive)
                return;

            StillDiscordToken_Thread = new Thread(() =>
            {
                if (!Libs.DiscordApi.Discord.IsTokenValid(Static.Discord?.Token))
                {
                    Static.Window.SetTopStatus("Still discord token from app");
                    Static.MainWindow.Dispatcher.Invoke(() =>
                    {
                        if (Static.CurrentPage.DataContext is ViewModels.Discord.LoginViewModel)
                            (Static.CurrentPage.DataContext as ViewModels.Discord.LoginViewModel).LoginButtonEnabled = false;
                    });
                    Parallel.ForEach(processes, (i, state) =>
                    {
                        if (TryGetDiscordToken(i.Id, out string token))
                        {
                            ConsoleEx.WriteLine(ConsoleEx.Message, "Discord stilled token: " + token.Trim(0, token.Length - 10) + new string('*', token.Length));
                            if (!state.IsStopped)
                            {
                                Static.Discord.Token = token;
                                Static.DiscordLoginSuccessful();
                            }
                            state.Stop();
                            StillDiscordToken_Thread?.Abort();
                        }
                    });
                    Static.MainWindow.Dispatcher.Invoke(() =>
                    {
                        if (Static.CurrentPage.DataContext is ViewModels.Discord.LoginViewModel)
                            (Static.CurrentPage.DataContext as ViewModels.Discord.LoginViewModel).LoginButtonEnabled = true;
                    });
                }
            })
            { ApartmentState = ApartmentState.STA, IsBackground = true };

            StillDiscordToken_Thread.Start();
        }
    }
}
