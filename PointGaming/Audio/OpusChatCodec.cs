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
        private WaveFormat _recordingFormat;
        private OpusDecoder _decoder;
        private OpusEncoder _encoder;
        private byte[] _encoderInputBuffer;
        private int _bufferCount;
        private string _description;
        private readonly int _segmentLength;

        public OpusChatCodec(int sampleRate, string description)
        {
            this._segmentLength = sampleRate / 25;// 2 bytes per sample, 20ms per segment
            this._decoder = OpusDecoder.Create(sampleRate, 1);
            this._encoder = OpusEncoder.Create(sampleRate, 1, Application.Voip);
            // 16kHz sample rate is 32kB/s raw.
            // Compressed bitrates:
            // 32768 is 4kB/s
            // 24800 is 3kB/s (3100B/s, same as Speex 16kHz)
            // 8192 is 1kB/s
            this._encoder.Bitrate = 24800;
            this._recordingFormat = new WaveFormat(sampleRate, 16, 1);
            this._description = description;
            this._encoderInputBuffer = new byte[this._recordingFormat.AverageBytesPerSecond]; // more than enough
        }

        public string Name
        {
            get { return _description; }
        }

        public int BitsPerSecond
        {
            get { return -1; }
        }

        public WaveFormat RecordFormat
        {
            get { return _recordingFormat; }
        }

        public int Encode(byte[] data, int offset, int length)
        {
            FeedSamplesIntoEncoderInputBuffer(data, offset, length);
            return _bufferCount / _segmentLength;
        }

        public byte[] GetEncoded()
        {
            byte[] encoded;

            if (_bufferCount >= _segmentLength)
            {
                int encodedLength;
                var encodedOut = _encoder.Encode(_encoderInputBuffer, _segmentLength, out encodedLength);
                encoded = new byte[encodedLength];
                Buffer.BlockCopy(encodedOut, 0, encoded, 0, encodedLength);
                ShiftLeftoverSamplesDown(_segmentLength);
            }
            else
                encoded = new byte[0];
            
            return encoded;
        }

        private void FeedSamplesIntoEncoderInputBuffer(byte[] data, int offset, int length)
        {
            Buffer.BlockCopy(data, offset, _encoderInputBuffer, _bufferCount, length);
            _bufferCount += length;
        }

        private void ShiftLeftoverSamplesDown(int shiftCount)
        {
            int leftoverSamples = _bufferCount - shiftCount;
            Buffer.BlockCopy(_encoderInputBuffer, shiftCount, _encoderInputBuffer, 0, leftoverSamples);
            _bufferCount = leftoverSamples;
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
            byte[] decodedOut = _decoder.Decode(data, length, out decodedLength);
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
