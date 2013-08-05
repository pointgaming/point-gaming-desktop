using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace PointGaming.Audio
{
    [Export(typeof(INetworkChatCodec))]
    class Opus8kCodec : OpusChatCodec
    {
        public Opus8kCodec() :
            base(8000, "Opus 8kHz")
        {

        }
    }

    [Export(typeof(INetworkChatCodec))]
    class Opus12kCodec : OpusChatCodec
    {
        public Opus12kCodec() :
            base(12000, "Opus 12kHz")
        {

        }
    }

    [Export(typeof(INetworkChatCodec))]
    class Opus16kCodec : OpusChatCodec
    {
        public Opus16kCodec() :
            base(16000, "Opus 16kHz")
        {

        }
    }

    [Export(typeof(INetworkChatCodec))]
    class Opus24kCodec : OpusChatCodec
    {
        public Opus24kCodec() :
            base(24000, "Opus 24kHz")
        {

        }
    }

    [Export(typeof(INetworkChatCodec))]
    class Opus48kCodec : OpusChatCodec
    {
        public Opus48kCodec() :
            base(48000, "Opus 48kHz")
        {

        }
    }

    class OpusChatCodec : INetworkChatCodec
    {
        private WaveFormat recordingFormat;
        private OpusDecoder decoder;
        private OpusEncoder encoder;
        private WaveBuffer encoderInputBuffer;
        private string description;

        public OpusChatCodec(int sampleRate, string description)
        {
            this.decoder = OpusDecoder.Create(sampleRate, 1);
            this.encoder = OpusEncoder.Create(sampleRate, 1, Application.Restricted_LowLatency);
            this.encoder.Bitrate = 8192;
            this.recordingFormat = new WaveFormat(sampleRate, 16, 1);
            this.description = description;
            this.encoderInputBuffer = new WaveBuffer(this.recordingFormat.AverageBytesPerSecond); // more than enough
        }

        public string Name
        {
            get { return description; }
        }

        public int BitsPerSecond
        {
            get { return -1; }
        }

        public WaveFormat RecordFormat
        {
            get { return recordingFormat; }
        }

        public byte[] Encode(byte[] data, int offset, int length)
        {
            FeedSamplesIntoEncoderInputBuffer(data, offset, length);
            
            var segmentLen = 1920;
            byte[] encoded = new byte[0];
            while (encoderInputBuffer.ByteBufferCount > segmentLen)
            {
                int encodedLength;
                var encodedOut = encoder.Encode(encoderInputBuffer.ByteBuffer, segmentLen, out encodedLength);
                byte[] encoded2 = new byte[encoded.Length + encodedLength];
                Buffer.BlockCopy(encoded, 0, encoded2, 0, encoded.Length);
                Buffer.BlockCopy(encodedOut, 0, encoded2, encoded.Length, encodedLength);
                ShiftLeftoverSamplesDown(segmentLen);
            }

            return encoded;
        }

        private void ShiftLeftoverSamplesDown(int length)
        {
            int leftoverSamples = encoderInputBuffer.ByteBufferCount - length;
            Buffer.BlockCopy(encoderInputBuffer.ByteBuffer, length, encoderInputBuffer.ByteBuffer, 0, leftoverSamples);
            encoderInputBuffer.ByteBufferCount = leftoverSamples;
        }

        private void FeedSamplesIntoEncoderInputBuffer(byte[] data, int offset, int length)
        {
            Buffer.BlockCopy(data, offset, encoderInputBuffer.ByteBuffer, encoderInputBuffer.ByteBufferCount, length);
            encoderInputBuffer.ByteBufferCount += length;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            if (offset != 0)
            {
                var data2 = new byte[length];
                Buffer.BlockCopy(data, offset, data2, 0, length);
                data = data2;
                offset = 0;
            }

            int decodedLength;
            var decodedOut = decoder.Decode(data, length, out decodedLength);
            byte[] decoded = new byte[decodedLength];
            Buffer.BlockCopy(decodedOut, 0, decoded, 0, decodedLength);
            return decoded;
        }

        public void Dispose()
        {
            // nothing to do
        }

        public bool IsAvailable { get { return true; } }
    }
}
