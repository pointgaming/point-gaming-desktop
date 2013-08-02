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

        public NAudioTest(INetworkChatCodec codec)
        {
            this.codec = codec;

            waveOut = new WaveOut();
            waveProvider = new BufferedWaveProvider(codec.RecordFormat);
            waveOut.Init(waveProvider);
            waveOut.Play();

            _maxSamplesAfterPickup = codec.RecordFormat.SampleRate / 3;
            _samplesSincePickup = _maxSamplesAfterPickup;
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

        private float _pickupAmplitude = 32767 / 4;
        private bool _foundPickup = false;
        private int _samplesSincePickup;
        private int _maxSamplesAfterPickup;
        
        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            var isPickupLost = false;

            var bytesRecorded = e.BytesRecorded;
            for (int i = 0; i < bytesRecorded; i+= 2)
            {
                var b1 = (ushort)e.Buffer[i];
                var b2 = (ushort)e.Buffer[i + 1];
                var sample = (short)((b2 << 8) | b1);
                var absSample = sample >= 0 ? sample : -sample;

                if (absSample >= _pickupAmplitude)
                    _samplesSincePickup = 0;
                else if (_samplesSincePickup < _maxSamplesAfterPickup)
                {
                    _samplesSincePickup++;
                    if (_samplesSincePickup == _maxSamplesAfterPickup)
                        isPickupLost = true;
                }
            }

            var isPickup = _samplesSincePickup < _maxSamplesAfterPickup;

            if (isPickupLost || isPickup)
            {
                byte[] encoded = codec.Encode(e.Buffer, 0, bytesRecorded);

                var call = AudioRecorded;
                if (call != null)
                    call(this, encoded, !isPickup);
            }
        }
    }

}
