using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using NSpeex;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace PointGaming.Voice
{
    [Export(typeof(IVoipCodec))]
    class NarrowBandSpeexCodec : VoipCodecSpeex
    {
        public NarrowBandSpeexCodec() :
            base(BandMode.Narrow, 8000, "Speex Narrow Band")
        {

        }
    }

    [Export(typeof(IVoipCodec))]
    class WideBandSpeexCodec : VoipCodecSpeex
    {
        public WideBandSpeexCodec() :
            base(BandMode.Wide, 16000, "Speex Wide Band (16kHz)")
        {

        }
    }

    [Export(typeof(IVoipCodec))]
    class UltraWideBandSpeexCodec : VoipCodecSpeex
    {
        public UltraWideBandSpeexCodec() :
            base(BandMode.UltraWide, 32000, "Speex Ultra Wide Band (32kHz)")
        {

        }
    }

    class VoipCodecSpeex : IVoipCodec
    {
        private WaveFormat _recordingFormat;
        private SpeexDecoder _decoder;
        private SpeexEncoder _encoder;
        private WaveBuffer _encoderInputBuffer;
        private string _description;
        private BandMode _bandMode;
        byte[] _outputBufferTemp;

        public VoipCodecSpeex(BandMode bandMode, int sampleRate, string description, VoipCodecMode mode = VoipCodecMode.Both)
        {
            _bandMode = bandMode;
            _recordingFormat = new WaveFormat(sampleRate, 16, 1);
            _description = description;

            if (mode.HasFlag(VoipCodecMode.Decode))
                _decoder = new SpeexDecoder(bandMode);
            if (mode.HasFlag(VoipCodecMode.Encode))
            {
                _encoder = new SpeexEncoder(bandMode);
                _outputBufferTemp = new byte[_recordingFormat.AverageBytesPerSecond];
                _encoderInputBuffer = new WaveBuffer(_recordingFormat.AverageBytesPerSecond); // more than enough
            }
        }

        public IVoipCodec Duplicate(VoipCodecMode mode)
        {
            return new VoipCodecSpeex(_bandMode, _recordingFormat.SampleRate, _description, mode);
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
            return _encoderInputBuffer.ShortBufferCount / _encoder.FrameSize;
        }

        public double GetEncoded(out byte[] encoded)
        {
            double signalPower = 0;
            int samplesToEncode = _encoder.FrameSize;
            if (_encoderInputBuffer.ShortBufferCount < _encoder.FrameSize)
            {
                encoded = new byte[0];
                return signalPower;
            }

            signalPower = SignalHelpers.CalculatePowerInDb(_encoderInputBuffer.ByteBuffer, 0, samplesToEncode * 2, _recordingFormat.SampleRate);
            int bytesWritten = _encoder.Encode(_encoderInputBuffer.ShortBuffer, 0, samplesToEncode, _outputBufferTemp, 0, _outputBufferTemp.Length);
            encoded = new byte[bytesWritten];
            Array.Copy(_outputBufferTemp, 0, encoded, 0, bytesWritten);
            ShiftLeftoverSamplesDown(samplesToEncode);
            return signalPower;
        }

        private void ShiftLeftoverSamplesDown(int samplesEncoded)
        {
            int leftoverSamples = _encoderInputBuffer.ShortBufferCount - samplesEncoded;
            Array.Copy(_encoderInputBuffer.ByteBuffer, samplesEncoded * 2, _encoderInputBuffer.ByteBuffer, 0, leftoverSamples * 2);
            _encoderInputBuffer.ShortBufferCount = leftoverSamples;
        }

        private void FeedSamplesIntoEncoderInputBuffer(byte[] data, int offset, int length)
        {
            Array.Copy(data, offset, _encoderInputBuffer.ByteBuffer, _encoderInputBuffer.ByteBufferCount, length);
            _encoderInputBuffer.ByteBufferCount += length;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            byte[] outputBufferTemp = new byte[length * 320];
            WaveBuffer wb = new WaveBuffer(outputBufferTemp);
            int samplesDecoded = _decoder.Decode(data, offset, length, wb.ShortBuffer, 0, false);
            int bytesDecoded = samplesDecoded * 2;
            byte[] decoded = new byte[bytesDecoded];
            Array.Copy(outputBufferTemp, 0, decoded, 0, bytesDecoded);
            return decoded;
        }

        public void Dispose()
        {
        }
    }
}
