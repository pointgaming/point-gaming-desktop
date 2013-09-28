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
    public class SettingsApplication : ViewModelBase
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

        private SettingsSaver<SettingsApplication> _saver;

        public static SettingsApplication Load()
        {
            var saver = new SettingsSaver<SettingsApplication>(null);
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
