using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace Razer_App_Engine_High_CPU_Usage_Fix
{
    class Program
    {
        public static Timer AppEngineTimer = new Timer()
        {
            Interval = 1000
        };

        static void Main(string[] args)
        {
            WindowHelper.WindowVisibility(false);
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            // If program runs with "--remove" parameter then no longer will run when startup
            // Otherwise program will open startup
            if (args.Contains("--remove"))
                registryKey.DeleteValue(Application.ProductName, false);
            else if (!args.Contains("--remove"))
                registryKey.SetValue(Application.ProductName, Application.ExecutablePath);

            // Wait while RazerApp engine starts
            AppEngineTimer.Elapsed += AppEngineTimer_Tick;
            AppEngineTimer.Start();
            Console.ReadKey();
        }

        private static void AppEngineTimer_Tick(object sender, EventArgs e)
        {
            // Checks if THX Spatial Audio is started
            if (Process.GetProcessesByName("RZTHXService").Length != 0)
            {
                AppEngineTimer.Stop();
                //Restart audio service
                RestartAudioService();
            }
        }

        private static void RestartAudioService()
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    // Run as administrator
                    Verb = "runas",
                    Arguments = "/C net stop audiosrv && net start audiosrv",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            proc.Start();
            Environment.Exit(0);
        }
    }
}
