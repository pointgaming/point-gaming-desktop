using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using NAudio.Wave.Compression;
using System.Diagnostics;
using NAudio;
using System.Windows.Input;

namespace PointGaming.Voice
{
    delegate void AudioAvailable(AudioHardwareSession source, byte[] encoded, double signalPower);

    class AudioHardwareSession : IDisposable
    {
        public event AudioAvailable AudioRecorded;
        public event Action AudioRecordEnded;
        public event Action<int> InputDeviceNumberChanged;
        public event Action AudioSystemTick;

        private WaveIn waveIn;
        private WaveOut waveOut;
        private MixingWaveProvider waveProvider;
        private IVoipCodec codec;
        public Settings.ControlBinding TriggerInput { get; set; }

        private int _InputDeviceNumber;
        public int InputDeviceNumber
        {
            get { return _InputDeviceNumber; }
            set
            {

                if (value == _InputDeviceNumber)
                    return;
                _InputDeviceNumber = value;
                var call = InputDeviceNumberChanged;
                if (call != null)
                    call(value);
            }
        }

        private object _startStopSynch = new object();

        public AudioHardwareSession(IVoipCodec codec)
        {
            this.codec = codec;
            TriggerInput = new Settings.ControlBinding { KeyboardKey = System.Windows.Input.Key.LeftCtrl };
        }

        public void Enable()
        {
            StartSending();
            StartPlaying();
        }
        public void Disable()
        {
            StopPlaying();
            StopSending();
        }

        private void StartSending()
        {
            lock (_startStopSynch)
            {
                if (waveIn != null)
                    return;

                int deviceNumber = InputDeviceNumber;

                while (true)
                {
                    try
                    {
                        CaptureMic(deviceNumber);
                        break;
                    }
                    catch (Exception e)
                    {
                        if (deviceNumber != 0)
                            deviceNumber = 0;
                        else
                        {
                            App.LogLine("Failed to start microphone capture due to " + e.Message);
                            App.LogLine(e.StackTrace);
                            break;
                        }
                    }
                }
                InputDeviceNumber = deviceNumber;
            }
        }

        private void CaptureMic(int deviceNumber)
        {
            waveIn = new WaveIn();
            waveIn.BufferMilliseconds = 20;
            waveIn.DeviceNumber = deviceNumber;
            waveIn.WaveFormat = codec.RecordFormat;

            waveIn.DataAvailable += waveIn_DataAvailable;
            waveIn.StartRecording();
        }

        private void StopSending()
        {
            lock (_startStopSynch)
            {
                if (waveIn == null)
                    return;
                waveIn.DataAvailable -= waveIn_DataAvailable;
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }
        }

        public bool IsEnabled
        {
            get
            {
                lock (_startStopSynch)
                {
                    return waveOut != null;
                }
            }
        }

        private void StartPlaying()
        {
            lock (_startStopSynch)
            {
                if (waveOut != null)
                    return;

                waveOut = new WaveOut();
                waveProvider = new MixingWaveProvider(codec.RecordFormat.SampleRate, codec);
                waveOut.DesiredLatency = 100;
                waveOut.Init(waveProvider);
                waveOut.Play();
            }
        }
        private void StopPlaying()
        {
            lock (_startStopSynch)
            {
                if (waveOut == null)
                    return;

                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
        }

        public void AudioReceived(string streamId, byte[] b, bool isEncoded)
        {
            lock (_startStopSynch)
            {
                if (waveOut == null)
                    return;
                waveProvider.AddSamples(streamId, b, 0, b.Length, isEncoded);
            }
        }

        public void AudioReceiveEnded(string streamId)
        {
            lock (_startStopSynch)
            {
                waveProvider.RemoveStreamOnEmpty(streamId);
            }
        }

        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            double amp;
            try { amp = UserDataManager.UserData.Settings.VoiceAmplifier; }
            catch { amp = 1.0; }
            SignalHelpers.Amplify(e.Buffer, 0, e.BytesRecorded, amp);

            int callCount = codec.Encode(e.Buffer, 0, e.BytesRecorded);
            for (int i = 0; i < callCount; i++)
            {
                byte[] encoded;
                var signalPower = codec.GetEncoded(out encoded);
                OnAudioRecorded(encoded, signalPower);
            }

            var call = AudioSystemTick;
            if (call != null)
                call();
        }

        private DateTime _trickleTimeout;
        private bool _isTrickleStarted = false;
        private bool _lastTrickleSent = true;

        private void OnAudioRecorded(byte[] encoded, double signalPower)
        {
            bool isMicTriggered = TriggerInput.IsDown;

            if (isMicTriggered)
            {
                _lastTrickleSent = false;
                _isTrickleStarted = false;
            }

            if (!isMicTriggered && _lastTrickleSent)
                return;

            var call = AudioRecorded;
            if (call != null)
                call(this, encoded, signalPower);

            if (!isMicTriggered)
            {
                if (!_isTrickleStarted)
                {
                    _isTrickleStarted = true;
                    _trickleTimeout = DateTime.UtcNow + TimeSpan.FromMilliseconds(200);
                }
            }

            if (!isMicTriggered && DateTime.UtcNow > _trickleTimeout)
            {
                OnAudioRecordEnded();
                _lastTrickleSent = true;// gotta trickle a littme more after the key goes up
            }
        }
        
        private void OnAudioRecordEnded()
        {
            var call = AudioRecordEnded;
            if (call != null)
                call();
        }

        public void Dispose()
        {
            StopSending();
            StopPlaying();
        }
    }
}
