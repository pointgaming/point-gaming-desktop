using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace PointGaming.Voice
{
    [Export(typeof(IVoipCodec))]
    class Opus8kCodec : VoipCodecOpus
    {
        public Opus8kCodec() :
            base(8000, 12400, "Opus 8kHz")
        {

        }
    }

    [Export(typeof(IVoipCodec))]
    class Opus12kCodec : VoipCodecOpus
    {
        public Opus12kCodec() :
            base(12000, 18600, "Opus 12kHz")
        {

        }
    }

    [Export(typeof(IVoipCodec))]
    class Opus16kCodec : VoipCodecOpus
    {
        public Opus16kCodec() :
            base(16000, 24800, "Opus 16kHz")
        {

        }
    }

    [Export(typeof(IVoipCodec))]
    class Opus24kCodec : VoipCodecOpus
    {
        public Opus24kCodec() :
            base(24000, 37200, "Opus 24kHz")
        {

        }
    }

    [Export(typeof(IVoipCodec))]
    class Opus48kCodec : VoipCodecOpus
    {
        public Opus48kCodec() :
            base(48000, 74400, "Opus 48kHz")
        {

        }
    }

    class VoipCodecOpus : IVoipCodec
    {
        private WaveFormat _recordingFormat;
        private OpusDecoder _decoder;
        private OpusEncoder _encoder;
        private byte[] _encoderInputBuffer;
        private int _bufferCount;
        private string _description;
        private readonly int _segmentLength;
        private readonly int _bitrate;
        
        public VoipCodecOpus(int sampleRate, int bitrate, string description, VoipCodecMode mode = VoipCodecMode.Both)
        {
            _bitrate = bitrate;
            _segmentLength = sampleRate / 25;// 2 bytes per sample, 20ms per segment
            _recordingFormat = new WaveFormat(sampleRate, 16, 1);
            _description = description;

            if (mode.HasFlag(VoipCodecMode.Decode))
                _decoder = OpusDecoder.Create(sampleRate, 1);
            
            if (mode.HasFlag(VoipCodecMode.Encode))
            {
                _encoder = OpusEncoder.Create(sampleRate, 1, OpusAPI.Application.Voip);
                // 16kHz sample rate is 32kB/s raw.
                // Compressed bitrates:
                // 32768 is 4kB/s
                // 24800 is 3kB/s (3100B/s, same as Speex 16kHz)
                // 8192 is 1kB/s
                _encoder.Bitrate = bitrate;
                _encoderInputBuffer = new byte[_recordingFormat.AverageBytesPerSecond]; // more than enough
            }
        }

        public IVoipCodec Duplicate(VoipCodecMode mode)
        {
            return new VoipCodecOpus(_recordingFormat.SampleRate, _bitrate, _description, mode);
        }

        public string Name
        {
            get { return _description; }
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

        public double GetEncoded(out byte[] encoded)
        {
            double signalPower = 0;

            if (_bufferCount >= _segmentLength)
            {
                int encodedLength;
                signalPower = SignalHelpers.CalculatePowerInDb(_encoderInputBuffer, 0, _segmentLength, (double)_recordingFormat.SampleRate);
                var encodedOut = _encoder.Encode(_encoderInputBuffer, _segmentLength, out encodedLength);
                encoded = new byte[encodedLength];
                Buffer.BlockCopy(encodedOut, 0, encoded, 0, encodedLength);
                ShiftLeftoverSamplesDown(_segmentLength);
            }
            else
            {
                encoded = new byte[0];
                signalPower = 0;
            }
            
            return signalPower;
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
            if (_decoder != null)
                _decoder.Dispose();
            if (_encoder != null)
                _encoder.Dispose();
            _decoder = null;
            _encoder = null;
        }
    }
}
