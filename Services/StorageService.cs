using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using ClickTracker.Models;

namespace ClickTracker.Services
{
    public class StorageService
    {
        private const string CLICK_DATA_FILE = "click_data.json";
        private const string SETTINGS_FILE = "app_settings.json";
        private readonly string _dataDirectory;

        public StorageService()
        {
            _dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClickTracker"
            );

            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public void SaveClickData(ClickData clickData)
        {
            try
            {
                string filePath = Path.Combine(_dataDirectory, CLICK_DATA_FILE);
                string jsonData = JsonConvert.SerializeObject(clickData, Formatting.Indented);
                File.WriteAllText(filePath, jsonData, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving click data: {ex.Message}");
            }
        }

        public ClickData LoadClickData()
        {
            try
            {
                string filePath = Path.Combine(_dataDirectory, CLICK_DATA_FILE);
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath, Encoding.UTF8);
                    return JsonConvert.DeserializeObject<ClickData>(jsonData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading click data: {ex.Message}");
            }

            return new ClickData();
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                string filePath = Path.Combine(_dataDirectory, SETTINGS_FILE);
                string jsonData = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, jsonData, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public AppSettings LoadSettings()
        {
            try
            {
                string filePath = Path.Combine(_dataDirectory, SETTINGS_FILE);
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath, Encoding.UTF8);
                    return JsonConvert.DeserializeObject<AppSettings>(jsonData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new AppSettings();
        }
    }
} 