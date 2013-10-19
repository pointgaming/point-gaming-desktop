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
        private List<HomeTab.LauncherInfo> _LauncherList = new List<HomeTab.LauncherInfo>();
        public List<HomeTab.LauncherInfo> LauncherList { get { return _LauncherList; } set { SetProperty(this, ref _LauncherList, value, () => LauncherList); } }
        private double _UserIdleMinutes = 10;
        public double UserIdleMinutes { get { return _UserIdleMinutes; } set { SetProperty(this, ref _UserIdleMinutes, value, () => UserIdleMinutes); } }
        private bool _BitcoinMinerOnlyWheUserIdle = true;
        public bool BitcoinMinerOnlyWheUserIdle { get { return _BitcoinMinerOnlyWheUserIdle; } set { SetProperty(this, ref _BitcoinMinerOnlyWheUserIdle, value, () => BitcoinMinerOnlyWheUserIdle); } }
        private bool _BitcoinMinerEnabled = false;
        public bool BitcoinMinerEnabled { get { return _BitcoinMinerEnabled; } set { SetProperty(this, ref _BitcoinMinerEnabled, value, () => BitcoinMinerEnabled); } }
        private List<WindowPersistInfo> _WindowBounds = new List<WindowPersistInfo>();
        public List<WindowPersistInfo> WindowBounds { get { return _WindowBounds; } set { SetProperty(this, ref _WindowBounds, value, () => WindowBounds); } }
        private ControlBinding _MicTriggerInput = new ControlBinding { KeyboardKey = System.Windows.Input.Key.LeftCtrl, };
        public ControlBinding MicTriggerInput { get { return _MicTriggerInput; } set { SetProperty(this, ref _MicTriggerInput, value, () => MicTriggerInput); } }
        private bool _MicTriggerSoundOffEnabled = true;
        public bool MicTriggerSoundOffEnabled { get { return _MicTriggerSoundOffEnabled; } set { SetProperty(this, ref _MicTriggerSoundOffEnabled, value, () => MicTriggerSoundOffEnabled); } }

        private double _VoiceAmplifier = 1.0;
        public double VoiceAmplifier { get { return _VoiceAmplifier; } set { SetProperty(this, ref _VoiceAmplifier, value, () => VoiceAmplifier); } }

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
