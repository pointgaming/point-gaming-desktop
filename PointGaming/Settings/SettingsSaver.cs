using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json;

namespace PointGaming.Settings
{
    abstract class SettingsSaver
    {
        protected object FileSystemSynch = new object();
    }

    class SettingsSaver<T> : SettingsSaver where T : INotifyPropertyChanged
    {
        private const string _settingsFileName = "settings.js";
        private string _saveFilePath;
                
        public SettingsSaver(string extraPath)
        {
            var directory = App.ApplicationSettingsPath;
            if (extraPath != null && extraPath.Length > 0)
                _saveFilePath = Path.Combine(directory, extraPath.FilterFilename(), _settingsFileName);
            else
                _saveFilePath = Path.Combine(directory, _settingsFileName);
        }

        public T Load()
        {
            T settings = default(T);

            try
            {
                lock (FileSystemSynch)
                {
                    if (File.Exists(_saveFilePath))
                    {
                        string fileData = File.ReadAllText(_saveFilePath);
                        settings = JsonConvert.DeserializeObject<T>(fileData, new JsonSerializerSettings { });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load settings due to Exception: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }

            if (settings == null)
            {
                settings = System.Activator.CreateInstance<T>();
                Save(settings);
            }
            return settings;
        }

        public void Save(T settings)
        {
            try
            {
                FileInfo fi = new FileInfo(_saveFilePath);
                DirectoryInfo di = fi.Directory;

                lock (FileSystemSynch)
                {
                    if (!di.Exists)
                        di.Create();
                }

                string fileData;
                lock (settings)
                {
                    fileData = JsonConvert.SerializeObject(settings, Formatting.Indented);
                }

                lock (FileSystemSynch)
                {
                    using (StreamWriter writer = File.CreateText(_saveFilePath))
                    {
                        writer.Write(fileData);
                        writer.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save settings due to Exception: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
