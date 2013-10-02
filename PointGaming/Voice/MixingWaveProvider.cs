using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;

namespace PointGaming.Voice
{
    class MixingWaveProvider : IWaveProvider
    {
        private List<IWaveProvider> _inputs = new List<IWaveProvider>();
        private WaveFormat _waveFormat;

        public MixingWaveProvider(int sampleRate)
        {
            _waveFormat = new WaveFormat(sampleRate, 16, 1);
        }

        public WaveFormat WaveFormat { get { return _waveFormat; } }

        public void AddStream(IWaveProvider waveProvider)
        {
            if (waveProvider.WaveFormat.BitsPerSample != 16)
                throw new ArgumentException("Only 16 bit audio currently supported", "waveProvider.WaveFormat");
            if (!waveProvider.WaveFormat.Equals(_waveFormat))
                throw new ArgumentException("All incoming channels must have the same format", "waveProvider.WaveFormat");

            lock (_inputs)
            {
                _inputs.Add(waveProvider);
            }
        }
        public void RemoveStream(IWaveProvider waveProvider)
        {
            lock (_inputs)
            {
                this._inputs.Remove(waveProvider);
            }
        }

        private readonly byte[] _inputBuffer = new byte[32000];
        private readonly float[] _sumBuffer = new float[16000];

        public int Read(byte[] buffer, int offset, int count)
        {
            if (count % 2 != 0)
                throw new ArgumentException("Must read an whole number of samples", "count");
            if (count > _inputBuffer.Length)
                throw new ArgumentException("Exceeded buffer capacity", "count");

            Array.Clear(buffer, offset, count);
            int bytesRead = 0;

            Array.Clear(_sumBuffer, 0, count >> 1);

            lock (_inputs)
            {
                foreach (var input in _inputs)
                {
                    int readFromThisStream = input.Read(_inputBuffer, 0, count);
                    if (readFromThisStream > bytesRead)
                        bytesRead = readFromThisStream;
                    if (readFromThisStream > 0)
                        AddChannel(readFromThisStream);
                }
            }

            FinishMix(buffer, offset, bytesRead);

            return count;
        }

        private void AddChannel(int bytesRead)
        {
            var i = 0;
            var end = bytesRead;
            while (i < end)
            {
                var sumI = i >> 1;
                var lo = (ushort)_inputBuffer[i++];
                var hi = (ushort)_inputBuffer[i++];
                var valueU = lo | (hi << 8);
                var value = (short)valueU;
                _sumBuffer[sumI] += value;
            }
        }

        private void FinishMix(byte[] buffer, int offset, int bytesRead)
        {
            var sumBufferIndex = 0;
            var bufferIndex = offset;
            var sumBUfferEnd = bytesRead >> 1;
            while (sumBufferIndex < sumBUfferEnd)
            {
                var valueF = _sumBuffer[sumBufferIndex++];
                // clip
                if (valueF > short.MaxValue)
                    valueF = short.MaxValue;
                else if (valueF < short.MinValue)
                    valueF = short.MinValue;
                // convert to bytes
                var valueS = (short)valueF;
                var valueU = (ushort)valueS;
                var lo = (byte)valueU;
                var hi = (byte)(valueU >> 8);
                buffer[bufferIndex++] = lo;
                buffer[bufferIndex++] = hi;
            }
        }


        public static short GetMaxValue(byte[] buffer, int length)
        {
            short result = 0;
            var i = 0;
            var end = length;
            while (i < end)
            {
                var lo = (ushort)buffer[i++];
                var hi = (ushort)buffer[i++];
                var valueU = lo | (hi << 8);
                var value = (short)valueU;
                if (value < 0)
                    value *= -1;
                if (value > result)
                    result = value;
            }

            return result;
        }
    }
}
