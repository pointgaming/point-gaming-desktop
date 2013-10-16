using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;

namespace PointGaming.Voice
{
    //class SineWaveProvider : IWaveProvider
    //{
    //    private double _frequency;
    //    private double _phaseInverse;
    //    private WaveFormat _waveFormat;

    //    public WaveFormat WaveFormat { get { return _waveFormat; } }

    //    public SineWaveProvider(int sampleRate, double frequency)
    //    {
    //        _waveFormat = new WaveFormat(sampleRate, 16, 1);
    //        _frequency = frequency;

    //        var period = sampleRate / frequency;
    //        _phaseInverse = 2.0 * Math.PI / period;
    //    }

    //    private long _time;

    //    public int Read(byte[] buffer, int offset, int count)
    //    {
    //        int end = count >> 1;
    //        for (int i = 0; i < end; i++)
    //        {
    //            var time = (_time + i) * _phaseInverse;
    //            var valueD = 1000 * Math.Cos(time);
    //            var value = (short)valueD;
    //            buffer[offset++] = (byte)(value);
    //            buffer[offset++] = (byte)(value >> 8);
    //        }
    //        _time += end;

    //        return count;
    //    }
    //}


    class MixingWaveProvider : IWaveProvider
    {
        private HashSet<string> _removes = new HashSet<string>();
        private Dictionary<string, IWaveProvider> _inputs = new Dictionary<string, IWaveProvider>();
        private Dictionary<string, IVoipCodec> _decoders = new Dictionary<string, IVoipCodec>();
        private WaveFormat _waveFormat;
        private List<IVoipCodec> _freeDecoders = new List<IVoipCodec>();
        private IVoipCodec _codec;

        public MixingWaveProvider(int sampleRate, IVoipCodec codec)
        {
            _codec = codec;
            _freeDecoders.Add(codec);
            _waveFormat = new WaveFormat(sampleRate, 16, 1);
            //_inputs.Add("test", new SineWaveProvider(sampleRate, 800));
        }

        public WaveFormat WaveFormat { get { return _waveFormat; } }

        public void AddSamples(string id, byte[] data, int offset, int count, bool isEncoded)
        {
            lock (_inputs)
            {
                if (_removes.Contains(id))
                    _removes.Remove(id);

                IWaveProvider wp;
                if (!_inputs.TryGetValue(id, out wp))
                {
                    wp = new BufferedWaveProvider(_waveFormat);
                    _inputs[id] = wp;
                    var codec = GetFreeCodec();
                    _decoders[id] = codec;
                }

                if (isEncoded)
                {
                    var codec = _decoders[id];
                    try
                    {
                        data = codec.Decode(data, offset, count);
                        offset = 0;
                        count = data.Length;
                        isEncoded = false;
                    }
                    catch
                    {
                        return;
                    }
                }

                (wp as BufferedWaveProvider).AddSamples(data, offset, count);
            }
        }

        private IVoipCodec GetFreeCodec()
        {
            var ixLast = _freeDecoders.Count - 1;
            if (ixLast == -1)
                return _codec.Duplicate();
            var last = _freeDecoders[ixLast];
            _freeDecoders.RemoveAt(ixLast);
            return last;
        }

        internal void RemoveStreamOnEmpty(string id)
        {
            lock (_inputs)
            {
                _removes.Add(id);
            }
        }

        internal void RemoveStream(string id)
        {
            lock (_inputs)
            {
                _removes.Remove(id);
                _inputs.Remove(id);
                IVoipCodec codec;
                if (_decoders.TryGetValue(id, out codec))
                {
                    _decoders.Remove(id);
                    _freeDecoders.Add(codec);
                }
            }
        }

        private readonly byte[] _inputBuffer = new byte[32000];
        private readonly int[] _sumBuffer = new int[16000];
        private readonly List<string> _emptyStreamIds = new List<string>();

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
                _emptyStreamIds.Clear();

                foreach (var kvp in _inputs)
                {
                    int readFromThisStream = kvp.Value.Read(_inputBuffer, 0, count);
                    if (readFromThisStream > bytesRead)
                        bytesRead = readFromThisStream;
                    if (readFromThisStream > 0)
                        AddChannel(readFromThisStream);
                    else
                        _emptyStreamIds.Add(kvp.Key);
                }

                foreach (var id in _emptyStreamIds)
                    if (_removes.Contains(id))
                        RemoveStream(id);
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
                //valueF /= _inputs.Count;
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
