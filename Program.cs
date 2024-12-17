using System;
using System.Windows.Forms;
using ClickTracker.Forms;
using ClickTracker.Services;

namespace ClickTracker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create storage service to check login state
            var storageService = new StorageService();
            var settings = storageService.LoadSettings();

            // Create the main form
            var mainForm = new MainForm();

            // Run the application
            Application.Run(mainForm);
        }
    }
}
