using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PointGaming.Settings
{
    public partial class VoiceTab : UserControl, ISettingsTab
    {
        private Voice.VoiceTester _voiceTester;

        public VoiceTab()
        {
            InitializeComponent();
        }

        void AudioSystem_RecordingDeviceChanged(int obj)
        {
            if (obj == App.Settings.AudioInputDeviceIndex)
                return;
            App.Settings.AudioInputDeviceIndex = obj;
            App.Settings.Save();
            comboBoxRecordingDevices.SelectedIndex = obj;
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
            UserDataManager.UserData.Voip.TriggerKey = key.Value;
        }

        private void comboBoxRecordingDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var deviceIndex = comboBoxRecordingDevices.SelectedIndex;
            App.Settings.AudioInputDeviceIndex = deviceIndex;
            App.Settings.Save();
            UserDataManager.UserData.Voip.SetAudioInputDevice(deviceIndex);
        }

        public void SettingsClosing()
        {
            try
            {
                UserDataManager.UserData.Voip.RecordingDeviceChanged -= AudioSystem_RecordingDeviceChanged;
                _voiceTester.OnVoiceEvent -= new Voice.VoiceTester.Event(vt_OnVoiceEvent);
                UserDataManager.UserData.Voip.TestVoiceStop();
            }
            catch { }
        }

        private bool _loadedOnce = false;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (_loadedOnce)
                return;
            _loadedOnce = true;

            foreach (var item in Voice.AudioHardware.GetAudioInputDevices())
                comboBoxRecordingDevices.Items.Add(item);
            comboBoxRecordingDevices.SelectedIndex = App.Settings.AudioInputDeviceIndex;
            labelMicKey.Content = (Key)UserDataManager.UserData.Settings.MicTriggerKey;
            UserDataManager.UserData.Voip.RecordingDeviceChanged += AudioSystem_RecordingDeviceChanged;

            progressBarVolume.Minimum = 0;
            progressBarVolume.Maximum = short.MaxValue;
            progressBarVolume.Value = 0;

            sliderTime.Minimum = 0;
            sliderTime.Maximum = 10;
            sliderTime.Value = 10;

            _voiceTester = new Voice.VoiceTester();
            _voiceTester.OnVoiceEvent += new Voice.VoiceTester.Event(vt_OnVoiceEvent);
            UserDataManager.UserData.Voip.TestVoiceStart(_voiceTester);
        }

        private bool _isRecording = false;

        void vt_OnVoiceEvent(Voice.VoiceTester.EventType type, TimeSpan time, short maxValue)
        {
            if (type == Voice.VoiceTester.EventType.Recorded)
            {
                progressBarVolume.Value = maxValue;

                if (!_isRecording)
                {
                    _isRecording = true;
                    sliderTime.Value = 0;
                }
            }
            else if (type == Voice.VoiceTester.EventType.RecordEnded)
            {
                _isRecording = false;
                progressBarVolume.Value = 0;

                sliderTime.Value = 0;
                sliderTime.Maximum = time.TotalMilliseconds;
            }
            else if (type == Voice.VoiceTester.EventType.Played)
            {
                progressBarVolume.Value = maxValue;
                sliderTime.Value = time.TotalMilliseconds;
            }
            else if (type == Voice.VoiceTester.EventType.PlayEnded)
            {
                progressBarVolume.Value = 0;
                sliderTime.Value = sliderTime.Maximum;
            }
        }
    }
}
