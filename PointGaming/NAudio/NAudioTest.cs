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

namespace PointGaming.NAudio
{
    public delegate void AudioAvailable(NAudioTest source, byte[] data);

    public partial class NAudioTest : IDisposable
    {
        public event AudioAvailable AudioRecorded;
        public event Action AudioRecordEnded;

        private WaveIn waveIn;
        private IWavePlayer waveOut;
        private BufferedWaveProvider waveProvider;
        private INetworkChatCodec codec;
        public System.Windows.Input.Key TriggerKey {get ; set;}

        public int InputDeviceNumber { get; set; }

        private object _startStopSynch = new object();

        public NAudioTest(INetworkChatCodec codec)
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

                waveIn = new WaveIn();
                waveIn.BufferMilliseconds = 50;
                waveIn.DeviceNumber = InputDeviceNumber;
                waveIn.WaveFormat = codec.RecordFormat;

                waveIn.DataAvailable += waveIn_DataAvailable;
                waveIn.StartRecording();
            }
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
                waveProvider = new BufferedWaveProvider(codec.RecordFormat);
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

        public void AudioReceived(byte[] b)
        {
            byte[] decoded = codec.Decode(b, 0, b.Length);
            lock (_startStopSynch)
            {
                if (waveOut == null)
                    return;
                waveProvider.AddSamples(decoded, 0, decoded.Length);
            }
        }

        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] encoded = codec.Encode(e.Buffer, 0, e.BytesRecorded);
            if (encoded.Length > 0)
            {
                OnAudioRecorded(encoded);
            }
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
