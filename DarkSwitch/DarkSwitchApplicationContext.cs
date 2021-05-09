using Microsoft.Win32;
using NonInvasiveKeyboardHookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DarkSwitch
{
    class DarkSwitchApplicationContext : ApplicationContext
    {
        private const string registryKey =
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string appsUseLightThemeValue =
            "AppsUseLightTheme";
        private const string systemUsesLightTheme =
            "SystemUsesLightTheme";
        private readonly NotifyIcon trayIcon;
        private readonly Icon lightIcon;
        private readonly Icon darkIcon;
        private readonly KeyboardHookManager keyBoardManager;

        public DarkSwitchApplicationContext()
        {
            if(AlreadyRunning())
            {
                Application.Exit();
                return;
            }

            keyBoardManager = new KeyboardHookManager();
            keyBoardManager.Start();
            keyBoardManager.RegisterHotkey(ModifierKeys.WindowsKey, (int)Keys.OemQuestion, ToggleTheme);

            lightIcon = Resources.LightIcon;
            darkIcon = InvertIcon(lightIcon);
            var systemUsesLightThemeValue = (int)Registry
                .GetValue(registryKey, systemUsesLightTheme, 1) != 0;

            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = systemUsesLightThemeValue ? lightIcon : darkIcon,
                Text = "DarkSwitch",
                Visible = true,
            };
            trayIcon.MouseClick += new MouseEventHandler(this.Icon_MouseClick);
        }

        public static bool AlreadyRunning()
        {
            try
            {
                // Getting collection of process  
                Process currentProcess = Process.GetCurrentProcess();

                // Check with other process already running   
                foreach (var p in Process.GetProcesses())
                {
                    if (p.Id != currentProcess.Id) // Check running process   
                    {
                        if (p.ProcessName.Equals(currentProcess.ProcessName) == true)
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static Icon InvertIcon(Icon ico)
        {
            var pic = new Bitmap(128,128);
            using (Graphics g = Graphics.FromImage(pic))
            {
                g.DrawImage(ico.ToBitmap(), 0, 0, 128, 128);
            }

            for (int y = 0; (y <= (pic.Height - 1)); y++)
            {
                for (int x = 0; (x <= (pic.Width - 1)); x++)
                {
                    Color inv = pic.GetPixel(x, y);
                    inv = Color.FromArgb(inv.A, (255 - inv.R), (255 - inv.G), (255 - inv.B));
                    pic.SetPixel(x, y, inv);
                }
            }
            return Icon.FromHandle(pic.GetHicon());
        }

        private void Icon_MouseClick(object _, MouseEventArgs __)
            => ToggleTheme();

        private void ToggleTheme()
        {
            var systemUsesLightThemeValue = (int)Registry
                .GetValue(registryKey, systemUsesLightTheme, 1) != 0;

            Registry.SetValue(registryKey, appsUseLightThemeValue, Convert.ToInt32(!systemUsesLightThemeValue));
            Registry.SetValue(registryKey, systemUsesLightTheme, Convert.ToInt32(!systemUsesLightThemeValue));
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            keyBoardManager.Stop();

            Application.Exit();
        }
    }
}
