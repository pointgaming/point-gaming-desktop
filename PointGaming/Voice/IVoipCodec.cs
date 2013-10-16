using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace PointGaming.Voice
{
    interface IVoipCodec : IDisposable
    {
        /// <summary>
        /// Creates a deep copy of the codec instance
        /// </summary>
        IVoipCodec Duplicate(VoipCodecMode mode);
        /// <summary>
        /// Friendly Name for this codec
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Preferred PCM format for recording in (usually 8kHz mono 16 bit)
        /// </summary>
        WaveFormat RecordFormat { get; }
        /// <summary>
        /// Returns how many times to call Encode given the input length
        /// </summary>
        int Encode(byte[] data, int offset, int length);
        /// <summary>
        /// Returns the signal power
        /// </summary>
        double GetEncoded(out byte[] encoded);
        /// <summary>
        /// Decodes a block of audio
        /// </summary>
        byte[] Decode(byte[] data, int offset, int length);
    }

    [Flags]
    enum VoipCodecMode
    {
        Encode = 1,
        Decode = 2,
        Both = 3,
    }
}
