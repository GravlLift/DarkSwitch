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

            #region Register hotkey
            keyBoardManager = new KeyboardHookManager();
            keyBoardManager.Start();
            keyBoardManager.RegisterHotkey(ModifierKeys.WindowsKey, (int)Keys.OemQuestion, ToggleTheme);
            #endregion

            // Save icons
            lightIcon = Resources.LightIcon;
            darkIcon = lightIcon.Invert();

            var systemUsesLightThemeValue = (int)Registry
                .GetValue(registryKey, systemUsesLightTheme, 1) != 0;

            // Initialize Tray Icon
            var contextMenu = new ContextMenuStrip();
            var exit = contextMenu.Items.Add("Exit");
            exit.Click += Exit;
            trayIcon = new NotifyIcon()
            {
                Icon = systemUsesLightThemeValue ? lightIcon : darkIcon,
                ContextMenuStrip = contextMenu,
                Text = "DarkSwitch\n (Win + ?)",
                Visible = true,
            };
            trayIcon.MouseClick += new MouseEventHandler(this.Icon_MouseClick);
        }

        private static bool AlreadyRunning()
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

        private void Icon_MouseClick(object _, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                ToggleTheme();
            }
        }

        private void ToggleTheme()
        {
            var systemUsesLightThemeValue = (int)Registry
                .GetValue(registryKey, systemUsesLightTheme, 1) != 0;

            Registry.SetValue(registryKey, appsUseLightThemeValue, Convert.ToInt32(!systemUsesLightThemeValue));
            Registry.SetValue(registryKey, systemUsesLightTheme, Convert.ToInt32(!systemUsesLightThemeValue));

            trayIcon.Icon = !systemUsesLightThemeValue ? lightIcon : darkIcon;
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
