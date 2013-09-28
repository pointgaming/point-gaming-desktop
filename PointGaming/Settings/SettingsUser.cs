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
    public class SettingsUser : ViewModelBase
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
        
        private SettingsSaver<SettingsUser> _saver;

        public static SettingsUser Load(string username)
        {
            var saver = new SettingsSaver<SettingsUser>(username);
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
