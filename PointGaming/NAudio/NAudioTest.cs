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

        private WaveIn waveIn;
        private IWavePlayer waveOut;
        private BufferedWaveProvider waveProvider;
        private INetworkChatCodec codec;

        public NAudioTest(INetworkChatCodec codec)
        {
            this.codec = codec;

            waveOut = new WaveOut();
            waveProvider = new BufferedWaveProvider(codec.RecordFormat);
            waveOut.Init(waveProvider);
            waveOut.Play();
        }

        public void StartSending()
        {
            if (waveIn != null)
                return;

            //for (int n = 0; n < WaveIn.DeviceCount; n++)
            //{
            //    var capabilities = WaveIn.GetCapabilities(n);
            //    this.comboBoxInputDevices.Items.Add(capabilities.ProductName);
            //}
            int inputDeviceNumber = 0;

            waveIn = new WaveIn();
            waveIn.BufferMilliseconds = 50;
            waveIn.DeviceNumber = inputDeviceNumber;
            waveIn.WaveFormat = codec.RecordFormat;
            
            waveIn.DataAvailable += waveIn_DataAvailable;
            waveIn.StartRecording();
        }
        public void StopSending()
        {
            if (waveIn == null)
                return;
            waveIn.DataAvailable -= waveIn_DataAvailable;
            waveIn.StopRecording();
            waveIn.Dispose();
            waveIn = null;
        }

        public void Dispose()
        {
            StopSending();

            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
                codec.Dispose();// a bit naughty but we have designed the codecs to support multiple calls to Dispose, recreating their resources if Encode/Decode called again
            }
        }
        
        public void AudioReceived(byte[] b)
        {
            byte[] decoded = codec.Decode(b, 0, b.Length);
            waveProvider.AddSamples(decoded, 0, decoded.Length);
        }
        
        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] encoded = codec.Encode(e.Buffer, 0, e.BytesRecorded);

            var call = AudioRecorded;
            if (call != null)
                call(this, encoded);
        }
    }

}
