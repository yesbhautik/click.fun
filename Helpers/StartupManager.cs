using Microsoft.Win32;
using System.Windows.Forms;

namespace ClickTracker.Helpers
{
    public static class StartupManager
    {
        public static void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            string appPath = Application.ExecutablePath;
            rk.SetValue("ClickTracker", appPath);
        }

        public static void RemoveStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            rk.DeleteValue("ClickTracker", false);
        }
    }
} 