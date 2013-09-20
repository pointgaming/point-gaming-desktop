using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.Voice
{
    public class AudioHardware
    {
        public static List<string> GetAudioInputDevices()
        {
            var devices = new List<string>();
            for (int n = 0; n < NAudio.Wave.WaveIn.DeviceCount; n++)
            {
                var capabilities = NAudio.Wave.WaveIn.GetCapabilities(n);
                devices.Add(capabilities.ProductName);
            }
            return devices;
        }
    }
}
