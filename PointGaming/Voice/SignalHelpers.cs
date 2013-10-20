using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.Voice
{
    static class SignalHelpers
    {
        public const double Power16bInt16000HzMinimumNonZero = 4.2041199826559247808549555788979721070727595258484341;
        public const double Power16bInt16000HzMaximum = 13.235019852575360637267122420632762910118455969711690;

        public static double CalculatePowerInDb(double[] x, int offset, int count, double sampleFrequency)
        {
            var dTime = 1.0 / sampleFrequency;
            double energySum = 0;
            int end = offset + count;
            for (int i = offset; i < end; i++)
            {
                double cur = x[i];
                energySum += cur * cur;
            }
            var power = CalcPowerInDb(count, dTime, energySum);
            return power;
        }

        public static double CalculatePowerInDb(short[] x, int offset, int count, double sampleFrequency)
        {
            var dTime = 1.0 / sampleFrequency;
            long energySum = 0;
            int end = offset + count;
            for (int i = offset; i < end; i++)
            {
                long cur = x[i];// convert to long to prevent overflow
                energySum += cur * cur;
            }
            var power = CalcPowerInDb(count, dTime, energySum);
            return power;
        }

        /// <summary>
        /// assumes 16 bit samples
        /// </summary>
        public static double CalculatePowerInDb(byte[] x, int offset, int count, double sampleFrequency)
        {
            var dTime = 1.0 / sampleFrequency;
            long energySum = 0;
            int end = offset + count;
            int i = offset;
            while (i < end)
            {
                var lo = (ushort)x[i++];
                var hi = (ushort)x[i++];
                var valueU = lo | (hi << 8);
                var value = (short)valueU;

                long cur = (long)value;// convert to long to prevent overflow
                energySum += cur * cur;
            }
            var power = CalcPowerInDb(count >> 1, dTime, energySum);
            return power;
        }

        private static double CalcPowerInDb(int count, double dTime, double energySum)
        {
            var power = energySum / (dTime * count);
            if (count == 0)
                power = 0;

            if (power <= 0)
                return 0;
            var log = Math.Log10(power);
            return log;
        }

        internal static void Amplify(byte[] x, int offset, int count, double multiplier)
        {
            int end = offset + count;
            int i = offset;
            while (i < end)
            {
                var lo = (ushort)x[i];
                var hi = (ushort)x[i + 1];
                var valueU = lo | (hi << 8);
                var value = (short)valueU;

                double ampValue = multiplier * value;
                ampValue = Math.Round(ampValue);

                if (ampValue <= short.MinValue)
                    value = short.MinValue;
                else if (ampValue >= short.MaxValue)
                    value = short.MaxValue;
                else
                    value = (short)ampValue;

                x[i] = (byte)value;
                x[i + 1] = (byte)(value >> 8);
                i += 2;
            }
        }

        internal static byte[] ShortsToBytes(this short[] data)
        {
            var x = new byte[data.Length << 1];
            Buffer.BlockCopy(data, 0, x, 0, x.Length);
            return x;
        }
        internal static short[] BytesToShorts(this byte[] data)
        {
            var x = new short[data.Length >> 1];
            Buffer.BlockCopy(data, 0, x, 0, data.Length);
            return x;
        }


        /// <summary>
        /// Makes the signal quickly come from silent to full volume at time 0%-3%, then from time 3%-100% it drops down to 0.22% of the signal
        /// </summary>
        internal static void LogNormal(short[] values)
        {
            var sMul = 1.0 / (Math.Sqrt(2 * Math.PI));

            var timeMul = 20.0 / (double)values.Length;

            values[0] = 0;
            for (int i = 1; i < values.Length; i++)
            {
                double x = i * timeMul;
                var top = (Math.Log(x) - 0.5);
                var e9 = -0.5 * top * top;
                var px = 2.5 * sMul * Math.Exp(e9) / x;
                values[i] = (short)(values[i] * px);
            }
        }

        public static SerialPacketStream ReadAudioResource(string fileName, NAudio.Wave.WaveFormat format) 
        {
            if (fileName.ToLower().EndsWith(".wav"))
            {
                var stream2 = App.GetResourceFileStream("MicActivate.wav");
                NAudio.Wave.WaveFileReader r = new NAudio.Wave.WaveFileReader(stream2);
                NAudio.Wave.ResamplerDmoStream s = new NAudio.Wave.ResamplerDmoStream(r, format);
                var length = (int)s.Length;
                byte[] resampled = new byte[length];
                s.Read(resampled, 0, length);
                var sps = new SerialPacketStream(resampled, format.SampleRate, fileName, 0, "self", false);
                return sps;
            }
            else if (fileName.ToLower().EndsWith(".pga"))
            {
                var stream = App.GetResourceFileStream("micTrigger.pga");
                var sps = SerialPacketStream.Read(stream);
                return sps;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
