﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using NSpeex;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace PointGaming.Audio
{
    [Export(typeof(INetworkChatCodec))]
    class NarrowBandSpeexCodec : SpeexChatCodec
    {
        public NarrowBandSpeexCodec() :
            base(BandMode.Narrow, 8000, "Speex Narrow Band")
        {

        }
    }

    [Export(typeof(INetworkChatCodec))]
    class WideBandSpeexCodec : SpeexChatCodec
    {
        public WideBandSpeexCodec() :
            base(BandMode.Wide, 16000, "Speex Wide Band (16kHz)")
        {

        }
    }

    [Export(typeof(INetworkChatCodec))]
    class UltraWideBandSpeexCodec : SpeexChatCodec
    {
        public UltraWideBandSpeexCodec() :
            base(BandMode.UltraWide, 32000, "Speex Ultra Wide Band (32kHz)")
        {

        }
    }

    class SpeexChatCodec : INetworkChatCodec
    {
        private WaveFormat recordingFormat;
        private SpeexDecoder decoder;
        private SpeexEncoder encoder;
        private WaveBuffer encoderInputBuffer;
        private string description;

        public SpeexChatCodec(BandMode bandMode, int sampleRate, string description)
        {
            this.decoder = new SpeexDecoder(bandMode);
            this.encoder = new SpeexEncoder(bandMode);
            this.recordingFormat = new WaveFormat(sampleRate, 16, 1);
            this.description = description;
            this.encoderInputBuffer = new WaveBuffer(this.recordingFormat.AverageBytesPerSecond); // more than enough
            outputBufferTemp = new byte[this.recordingFormat.AverageBytesPerSecond];
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

        public int Encode(byte[] data, int offset, int length)
        {
            FeedSamplesIntoEncoderInputBuffer(data, offset, length);
            return encoderInputBuffer.ShortBufferCount / encoder.FrameSize;
        }

        byte[] outputBufferTemp;
        public byte[] GetEncoded()
        {
            int samplesToEncode = encoder.FrameSize;
            if (encoderInputBuffer.ShortBufferCount < encoder.FrameSize)
                return new byte[0];

            int bytesWritten = encoder.Encode(encoderInputBuffer.ShortBuffer, 0, samplesToEncode, outputBufferTemp, 0, outputBufferTemp.Length);
            byte[] encoded = new byte[bytesWritten];
            Array.Copy(outputBufferTemp, 0, encoded, 0, bytesWritten);
            ShiftLeftoverSamplesDown(samplesToEncode);
            return encoded;
        }

        private void ShiftLeftoverSamplesDown(int samplesEncoded)
        {
            int leftoverSamples = encoderInputBuffer.ShortBufferCount - samplesEncoded;
            Array.Copy(encoderInputBuffer.ByteBuffer, samplesEncoded * 2, encoderInputBuffer.ByteBuffer, 0, leftoverSamples * 2);
            encoderInputBuffer.ShortBufferCount = leftoverSamples;
        }

        private void FeedSamplesIntoEncoderInputBuffer(byte[] data, int offset, int length)
        {
            Array.Copy(data, offset, encoderInputBuffer.ByteBuffer, encoderInputBuffer.ByteBufferCount, length);
            encoderInputBuffer.ByteBufferCount += length;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            byte[] outputBufferTemp = new byte[length * 320];
            WaveBuffer wb = new WaveBuffer(outputBufferTemp);
            int samplesDecoded = decoder.Decode(data, offset, length, wb.ShortBuffer, 0, false);
            int bytesDecoded = samplesDecoded * 2;
            byte[] decoded = new byte[bytesDecoded];
            Array.Copy(outputBufferTemp, 0, decoded, 0, bytesDecoded);
            return decoded;
        }

        public void Dispose()
        {
            // nothing to do
        }

        public bool IsAvailable { get { return true; } }
    }
}
