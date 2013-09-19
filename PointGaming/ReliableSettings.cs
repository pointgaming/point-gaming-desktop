using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json;

namespace PointGaming
{
    class SettingsSaver<T> where T : INotifyPropertyChanged
    {
        private const string _settingsFileName = "settings.js";
        private const string _okChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-1234567890";
        private string _saveFilePath;

        private static string FilenameFilter(string s)
        {
            var ok = new List<char>(_okChars.ToCharArray());
            var filtered = new List<char>();
            foreach (var c in s)
            {
                var cc = c;
                if (!ok.Contains(cc))
                {
                    int ix = cc;
                    ix = ix % ok.Count;
                    cc = ok[ix];
                }
                filtered.Add(cc);
            }

            return new string(filtered.ToArray());
        }
        
        public SettingsSaver(string extraPath)
        {
            var directory = App.ApplicationSettingsPath;
            if (extraPath != null && extraPath.Length > 0)
                _saveFilePath = Path.Combine(directory, FilenameFilter(extraPath), _settingsFileName);
            else
                _saveFilePath = Path.Combine(directory, _settingsFileName);
        }

        public T Load()
        {
            T settings = default(T);

            try
            {
                if (File.Exists(_saveFilePath))
                {
                    string fileData = File.ReadAllText(_saveFilePath);
                    settings = JsonConvert.DeserializeObject<T>(fileData, new JsonSerializerSettings { });
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
                if (!di.Exists)
                    di.Create();

                string fileData;
                lock (settings)
                {
                    var jsonSettings  = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
                    };
                    fileData = JsonConvert.SerializeObject(settings, Formatting.Indented, jsonSettings);
                }

                using (StreamWriter writer = File.CreateText(_saveFilePath))
                {
                    writer.Write(fileData);
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save settings due to Exception: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }

    public class UserSettings : ViewModelBase
    {
        // { get { return _; } set { SetProperty(this, ref _, value, () => ); } }
        private bool _ShouldFlashChatWindow = true;
        public bool ShouldFlashChatWindow { get { return _ShouldFlashChatWindow; } set { SetProperty(this, ref _ShouldFlashChatWindow, value, () => ShouldFlashChatWindow); } }
        private string _ChatFontFamily = "Segoe UI";
        public string ChatFontFamily { get { return _ChatFontFamily; } set { SetProperty(this, ref _ChatFontFamily, value, () => ChatFontFamily); } }
        private double _ChatFontSize = 12;
        public double ChatFontSize { get { return _ChatFontSize; } set { SetProperty(this, ref _ChatFontSize, value, () => ChatFontSize); } }
        private List<string> _LaunchList = new List<string>();
        public List<string> LaunchList { get { return _LaunchList; } set { SetProperty(this, ref _LaunchList, value, () => LaunchList); } }
        private double _UserIdleMinutes = 10;
        public double UserIdleMinutes { get { return _UserIdleMinutes; } set { SetProperty(this, ref _UserIdleMinutes, value, () => UserIdleMinutes); } }
        private bool _BitcoinMinerOnlyWheUserIdle = true;
        public bool BitcoinMinerOnlyWheUserIdle { get { return _BitcoinMinerOnlyWheUserIdle; } set { SetProperty(this, ref _BitcoinMinerOnlyWheUserIdle, value, () => BitcoinMinerOnlyWheUserIdle); } }
        private bool _BitcoinMinerEnabled = false;
        public bool BitcoinMinerEnabled { get { return _BitcoinMinerEnabled; } set { SetProperty(this, ref _BitcoinMinerEnabled, value, () => BitcoinMinerEnabled); } }
        private string _WindowBounds = "";
        public string WindowBounds { get { return _WindowBounds; } set { SetProperty(this, ref _WindowBounds, value, () => WindowBounds); } }
        private int _MicTriggerKey = 118;
        public int MicTriggerKey { get { return _MicTriggerKey; } set { SetProperty(this, ref _MicTriggerKey, value, () => MicTriggerKey); } }
        
        private SettingsSaver<UserSettings> _saver;

        public static UserSettings Load(string username)
        {
            var saver = new SettingsSaver<UserSettings>(username);
            var result = saver.Load();
            result._saver = saver;
            return result;
        }

        public void Save()
        {
            _saver.Save(this);
        }
    }

    public class ApplicationSettings : ViewModelBase
    {
        // { get { return _; } set { SetProperty(this, ref _, value, () => ); } }
        private string _Username = "";
        public string Username { get { return _Username; } set { SetProperty(this, ref _Username, value, () => Username); } }
        private string _Password = "";
        public string Password { get { return _Password; } set { SetProperty(this, ref _Password, value, () => Password); } }
        private string _SocketIoUrl = "https://socket.pointgaming.com/";
        public string SocketIoUrl { get { return _SocketIoUrl; } set { SetProperty(this, ref _SocketIoUrl, value, () => SocketIoUrl); } }
        private TimeSpan _LogInTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan LogInTimeout { get { return _LogInTimeout; } set { SetProperty(this, ref _LogInTimeout, value, () => LogInTimeout); } }
        private bool _UpdateAutomatic = true;
        public bool UpdateAutomatic { get { return _UpdateAutomatic; } set { SetProperty(this, ref _UpdateAutomatic, value, () => UpdateAutomatic); } }
        private string _StratumIp = "50.116.28.30";
        public string StratumIp { get { return _StratumIp; } set { SetProperty(this, ref _StratumIp, value, () => StratumIp); } }
        private int _StratumPort = 3334;
        public int StratumPort { get { return _StratumPort; } set { SetProperty(this, ref _StratumPort, value, () => StratumPort); } }
        private string _WebServerUrl = "https://dev.pointgaming.com";
        public string WebServerUrl { get { return _WebServerUrl; } set { SetProperty(this, ref _WebServerUrl, value, () => WebServerUrl); } }
        private string _AudioChatIp = "66.228.50.130";
        public string AudioChatIp { get { return _AudioChatIp; } set { SetProperty(this, ref _AudioChatIp, value, () => AudioChatIp); } }
        private int _AudioChatPort = 31337;
        public int AudioChatPort { get { return _AudioChatPort; } set { SetProperty(this, ref _AudioChatPort, value, () => AudioChatPort); } }
        private int _AudioInputDeviceIndex = 0;
        public int AudioInputDeviceIndex { get { return _AudioInputDeviceIndex; } set { SetProperty(this, ref _AudioInputDeviceIndex, value, () => AudioInputDeviceIndex); } }

        private SettingsSaver<ApplicationSettings> _saver;

        public static ApplicationSettings Load()
        {
            var saver = new SettingsSaver<ApplicationSettings>(null);
            var result = saver.Load();
            result._saver = saver;
            return result;
        }

        public void Save()
        {
            _saver.Save(this);
        }
    }
}
