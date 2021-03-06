﻿using System;
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
            var binding = KeySelectDialog.Show(HomeWindow.Home, "Select Microphone Key", "Assign the microphone hotkey by pressing/clicking the desired keyboard key/mouse button.  This window must be focused, and for mouse buttons the mouse pointer must be inside this textbox.", PermittedControlBinding.KeyboardKeysAndMouseButtons);
            if (binding == null)
                return;
            labelMicKey.Content = binding.ToString();
            UserDataManager.UserData.Settings.MicTriggerInput = binding;
            UserDataManager.UserData.Voip.TriggerInput = binding;
        }

        private void comboBoxRecordingDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var deviceIndex = comboBoxRecordingDevices.SelectedIndex;
            if (deviceIndex == App.Settings.AudioInputDeviceIndex)
                return;

            App.Settings.AudioInputDeviceIndex = deviceIndex;
            App.Settings.Save();
            UserDataManager.UserData.Voip.SetAudioInputDevice(deviceIndex);
        }

        public void SettingsClosing()
        {
            try
            {
                UserDataManager.UserData.Voip.RecordingDeviceChanged -= AudioSystem_RecordingDeviceChanged;
                _voiceTester.OnVoiceEvent -= vt_OnVoiceEvent;
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

            SetTxSoundOffLabel();

            foreach (var item in Voice.AudioHardware.GetAudioInputDevices())
                comboBoxRecordingDevices.Items.Add(item);
            comboBoxRecordingDevices.SelectedIndex = App.Settings.AudioInputDeviceIndex;
            labelMicKey.Content = UserDataManager.UserData.Settings.MicTriggerInput;
            UserDataManager.UserData.Voip.RecordingDeviceChanged += AudioSystem_RecordingDeviceChanged;

            progressBarPower.Minimum = Voice.SignalHelpers.Power16bInt16000HzMinimumNonZero;
            progressBarPower.Maximum = Voice.SignalHelpers.Power16bInt16000HzMaximum;
            progressBarPower.Value = 0;

            sliderTime.Minimum = 0;
            sliderTime.Maximum = 10;
            sliderTime.Value = 10;

            _voiceTester = new Voice.VoiceTester();
            _voiceTester.OnVoiceEvent += vt_OnVoiceEvent;
            UserDataManager.UserData.Voip.TestVoiceStart(_voiceTester);

            sliderAmplifier.Value = Math.Log10(UserDataManager.UserData.Settings.VoiceAmplifier);
        }

        private bool _isRecording = false;

        void vt_OnVoiceEvent(Voice.VoiceTester.EventType type, TimeSpan time, double signalPower)
        {
            if (type == Voice.VoiceTester.EventType.Recorded)
            {
                SetVolume(signalPower);

                if (!_isRecording)
                {
                    _isRecording = true;
                    sliderTime.Value = 0;
                }
            }
            else if (type == Voice.VoiceTester.EventType.RecordEnded)
            {
                _isRecording = false;
                progressBarPower.Value = 0;

                sliderTime.Value = 0;
                sliderTime.Maximum = time.TotalMilliseconds;
            }
            else if (type == Voice.VoiceTester.EventType.Played)
            {
                SetVolume(signalPower);
                sliderTime.Value = time.TotalMilliseconds;
            }
            else if (type == Voice.VoiceTester.EventType.PlayEnded)
            {
                progressBarPower.Value = 0;
                sliderTime.Value = sliderTime.Maximum;
            }
        }

        private void SetVolume(double signalPower)
        {
            signalPower = AveragePower(signalPower);
            progressBarPower.Value = signalPower;
            // bad is full red, then it adds more green until it becomes yellow
            // after yellow, it looses red to become green

            var minRed = Voice.SignalHelpers.Power16bInt16000HzMaximum - 3;
            var centerGreen = Voice.SignalHelpers.Power16bInt16000HzMaximum - 1.5;
            var maxRed = Voice.SignalHelpers.Power16bInt16000HzMaximum;

            var fractionGreen = 0.0;
            if (signalPower > minRed && signalPower <= centerGreen)
                fractionGreen = (signalPower - minRed) / (centerGreen - minRed);
            else if (signalPower > centerGreen && signalPower < maxRed)
                fractionGreen = (maxRed - signalPower) / (maxRed - centerGreen);
            fractionGreen = Math.Sqrt(fractionGreen);// make changing green more aggressive

            Color color;
            if (fractionGreen <= 0.5)
                color = Color.FromRgb(255, (byte)(255 * fractionGreen * 2), 0);
            else
                color = Color.FromRgb((byte)(255 - 255 * (fractionGreen - 0.5) * 2), 255, 0);

            var brush = new SolidColorBrush(color);
            var rect = (Rectangle)progressBarPower.Template.FindName("PART_Indicator", progressBarPower);

            var hslColor = new HslColor();
            hslColor.Hue = 120.0 * fractionGreen;

            var sats = new double[] { 15, 23, 34, 100, 100, 93 };
            var lums = new double[] { 100, 96, 92, 77, 84, 86 };
            var offs = new double[] { 0, 0.198, 0.516, 0.521, 0.797, 1.0 };

            var oldBrush = (LinearGradientBrush)rect.Fill;

            var lgBrush = new LinearGradientBrush();
            lgBrush.StartPoint = oldBrush.StartPoint;
            lgBrush.EndPoint = oldBrush.EndPoint;

            for (int i = 0; i < sats.Length; i++)
            {
                hslColor.Saturation = sats[i];
                hslColor.Luminosity = lums[i];
                var gsColor = (Color)hslColor;
                var gs = new GradientStop(gsColor, offs[i]);
                lgBrush.GradientStops.Add(gs);
            }

            rect.Fill = lgBrush;
        }

        private double[] _powerBuffer = new double[8];
        private int _powerBufferIndex = 0;

        private double AveragePower(double signalPower)
        {
            _powerBuffer[_powerBufferIndex] = Math.Pow(10, signalPower);
            _powerBufferIndex++;
            _powerBufferIndex %= _powerBuffer.Length;

            signalPower = 0.0;
            for (int i = 0; i < _powerBuffer.Length; i++)
            {
                signalPower += _powerBuffer[i];
            }
            signalPower /= (double)_powerBuffer.Length;
            signalPower = Math.Log10(signalPower);
            return signalPower;
        }

        private void sliderAmplifier_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var amp = Math.Pow(10, sliderAmplifier.Value);
            labelAmplifier.Content = string.Format("{0:0.00}", amp);
            UserDataManager.UserData.Settings.VoiceAmplifier = amp;
        }

        private void buttonToggleTxSoundOff_Click(object sender, RoutedEventArgs e)
        {
            var value = !UserDataManager.UserData.Settings.MicTriggerSoundOffEnabled;
            UserDataManager.UserData.Settings.MicTriggerSoundOffEnabled = value;
            SetTxSoundOffLabel();
        }

        private void SetTxSoundOffLabel()
        {
            var value = UserDataManager.UserData.Settings.MicTriggerSoundOffEnabled;
            labelTxSoundOff.Content = value ? "Enabled" : "Disabled";
        }
    }
}
