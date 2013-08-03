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
    public delegate void AudioAvailable(NAudioTest source, byte[] data, bool isLast);

    public partial class NAudioTest : IDisposable
    {
        public event AudioAvailable AudioRecorded;

        private WaveIn waveIn;
        private IWavePlayer waveOut;
        private BufferedWaveProvider waveProvider;
        private INetworkChatCodec codec;
        public System.Windows.Forms.Keys TriggerKey {get ; set;}

        public int InputDeviceNumber { get; set; }

        private object _startStopSynch = new object();

        public NAudioTest(INetworkChatCodec codec)
        {
            this.codec = codec;
            TriggerKey = System.Windows.Forms.Keys.LControlKey;

            App.KeyDown += App_KeyDown;
            App.KeyUp += App_KeyUp;
        }

        public void Enable()
        {
            StartPlaying();
        }
        public void Disable()
        {
            StopPlaying();
            StopSending();
        }

        private void App_KeyUp(System.Windows.Forms.Keys obj)
        {
            if (waveOut == null)
                return;

            if (obj != TriggerKey)
                return;

            StopSending();
        }

        private void App_KeyDown(System.Windows.Forms.Keys obj)
        {
            if (waveOut == null)
                return;

            if (obj != TriggerKey)
                return;

            StartSending();
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
                OnAudioRecorded(new byte[0]);
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
            OnAudioRecorded(encoded);
        }

        private void OnAudioRecorded(byte[] encoded)
        {
            var call = AudioRecorded;
            if (call != null)
                call(this, encoded, encoded.Length == 0);
        }

        public void Dispose()
        {
            StopSending();
            StopPlaying();
        }
    }

}
