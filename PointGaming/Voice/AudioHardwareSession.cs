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
using NA = NAudio;

namespace PointGaming.Voice
{
    public delegate void AudioAvailable(AudioHardwareSession source, byte[] data);

    public class AudioHardwareSession : IDisposable
    {
        public event AudioAvailable AudioRecorded;
        public event Action AudioRecordEnded;
        public event Action<int> InputDeviceNumberChanged;

        private WaveIn waveIn;
        private WaveOut waveOut;
        private MixingWaveProvider waveProvider;
        private IVoipCodec codec;
        public System.Windows.Input.Key TriggerKey {get ; set;}

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
            TriggerKey = System.Windows.Input.Key.LeftCtrl;
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
                waveProvider = new MixingWaveProvider(codec.RecordFormat.SampleRate);
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

        private Dictionary<string, BufferedWaveProvider> _myStreams = new Dictionary<string, BufferedWaveProvider>();

        public void AudioReceived(string streamId, byte[] b)
        {
            byte[] decoded;
            try
            {
                decoded = codec.Decode(b, 0, b.Length);
            }
            catch
            {
                return;
            }
            lock (_startStopSynch)
            {
                if (waveOut == null)
                    return;

                BufferedWaveProvider wp;
                if (!_myStreams.TryGetValue(streamId, out wp))
                {
                    wp = new BufferedWaveProvider(codec.RecordFormat);
                    waveProvider.AddStream(wp);
                    _myStreams[streamId] = wp;
                }
                wp.AddSamples(decoded, 0, decoded.Length);
            }
        }
        public void AudioReceiveEnded(string streamId)
        {
            lock (_startStopSynch)
            {
                BufferedWaveProvider wp;
                if (_myStreams.TryGetValue(streamId, out wp))
                {
                    _myStreams.Remove(streamId);
                    waveProvider.RemoveStream(wp);
                }
            }
        }

        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            int callCount = codec.Encode(e.Buffer, 0, e.BytesRecorded);
            for (int i = 0; i < callCount; i++)
                OnAudioRecorded(codec.GetEncoded());
        }

        private DateTime _trickleTimeout;
        private bool _isTrickleStarted = false;
        private bool _lastTrickleSent = true;

        private void OnAudioRecorded(byte[] encoded)
        {
            var isKeyDown = System.Windows.Input.Keyboard.IsKeyDown(TriggerKey);
            if (isKeyDown)
            {
                _lastTrickleSent = false;
                _isTrickleStarted = false;
            }

            if (!isKeyDown && _lastTrickleSent)
                return;

            var call = AudioRecorded;
            if (call != null)
                call(this, encoded);

            if (!isKeyDown)
            {
                if (!_isTrickleStarted)
                {
                    _isTrickleStarted = true;
                    _trickleTimeout = DateTime.UtcNow + TimeSpan.FromMilliseconds(200);
                }
            }

            if (!isKeyDown && DateTime.UtcNow > _trickleTimeout)
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
