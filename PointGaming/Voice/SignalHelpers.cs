﻿using System;
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

        /// <summary>
        /// assumes 16 bit samples
        /// </summary>
        public static double CalculatePowerInDb(byte[] x, int offset, int count, double sampleFrequency)
        {
            var dTime = 1.0 / sampleFrequency;
            double energySum = 0;
            int end = offset + count;
            int i = offset;
            while (i < end)
            {
                var lo = (ushort)x[i++];
                var hi = (ushort)x[i++];
                var valueU = lo | (hi << 8);
                var value = (short)valueU;

                double cur = (double)value;// convert to double to prevent overflow
                energySum += cur * cur;
            }
            var power = CalcPowerInDb(count, dTime, energySum);
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
                
                double cur = (double)value;// convert to double to prevent overflow
                cur *= multiplier;
                value = (short)cur;
                if (cur <= short.MinValue)
                    value = short.MinValue;
                else if (cur >= short.MaxValue)
                    value = short.MaxValue;

                x[i] = (byte)value;
                x[i + 1] = (byte)(value >> 8);
                i += 2;
            }
        }
    }
}
