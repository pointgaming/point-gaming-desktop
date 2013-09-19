using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PointGaming.HomeTab
{
    public partial class DebugTab : UserControl
    {
        public DebugTab()
        {
            InitializeComponent();

            foreach (var item in UserDataManager.UserData.AudioSystem.GetAudioInputDevices())
                comboBoxRecordingDevices.Items.Add(item);
            comboBoxRecordingDevices.SelectedIndex = App.Settings.AudioInputDeviceIndex;
            labelMicKey.Content = (Key)UserDataManager.UserData.Settings.MicTriggerKey;
            UserDataManager.UserData.AudioSystem.RecordingDeviceChanged += AudioSystem_RecordingDeviceChanged;
        }

        void AudioSystem_RecordingDeviceChanged(int obj)
        {
            if (obj == App.Settings.AudioInputDeviceIndex)
                return;
            App.Settings.AudioInputDeviceIndex = obj;
            App.Settings.Save();
            comboBoxRecordingDevices.SelectedIndex = obj;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            App.DebugBox = textBoxConsole;
        }

        public string ProgramVersion { get { return "Version " + App.Version; } }

        private void buttonChooseChatFont_Click(object sender, RoutedEventArgs e)
        {
            PgFonts.FontChooser fontChooser = new PgFonts.FontChooser();
            fontChooser.Owner = HomeWindow.Home;

            if (UserDataManager.UserData.Settings.ChatFontFamily != "")
                textBoxFontChoice.FontFamily = new System.Windows.Media.FontFamily(UserDataManager.UserData.Settings.ChatFontFamily + ", " + textBoxFontChoice.FontFamily);
            if (UserDataManager.UserData.Settings.ChatFontSize != 0)
                textBoxFontChoice.FontSize = UserDataManager.UserData.Settings.ChatFontSize;
            fontChooser.SetPropertiesFromObject(textBoxFontChoice);
            fontChooser.PreviewSampleText = textBoxFontChoice.Text;

            if (fontChooser.ShowDialog().Value)
            {
                fontChooser.ApplyPropertiesToObject(textBoxFontChoice);
                var family = textBoxFontChoice.FontFamily.ToString();
                var size = textBoxFontChoice.FontSize;
                UserDataManager.UserData.Settings.ChatFontFamily = family;
                UserDataManager.UserData.Settings.ChatFontSize = size;
                UserDataManager.UserData.Settings.Save();
            }
        }

        private void buttonSetMicKey_Click(object sender, RoutedEventArgs e)
        {
            SetAudioInputDeviceTriggerKey();
        }


        private void SetAudioInputDeviceTriggerKey()
        {
            var key = KeySelectDialog.Show(HomeWindow.Home, "Select Microphone Key", "Press new microphone hotkey.");
            if (!key.HasValue)
                return;
            labelMicKey.Content = key.Value;
            UserDataManager.UserData.Settings.MicTriggerKey = (int)key.Value;
            UserDataManager.UserData.Settings.Save();
            UserDataManager.UserData.AudioSystem.TriggerKey = key.Value;
        }
        
        private void comboBoxRecordingDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var deviceIndex = comboBoxRecordingDevices.SelectedIndex;
            App.Settings.AudioInputDeviceIndex = deviceIndex;
            App.Settings.Save();
            UserDataManager.UserData.AudioSystem.SetAudioInputDevice(deviceIndex);
        }
    }
}
