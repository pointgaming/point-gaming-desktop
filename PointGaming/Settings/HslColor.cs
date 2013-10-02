using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace PointGaming.Settings
{
    public class HslColor
    {
        private const double oneOver100 = 1.0 / 100.0;
        private const double oneOver60 = 1.0 / 60.0;

        private double _hue = 0;
        private double _saturation = 0;
        private double _luminosity = 0;

        public double Hue
        {
            get { return _hue; }
            set { _hue = CheckRange(value, 360.0); }
        }
        public double Saturation
        {
            get { return _saturation; }
            set { _saturation = CheckRange(value, 100.0); }
        }
        public double Luminosity
        {
            get { return _luminosity; }
            set { _luminosity = CheckRange(value, 100.0); }
        }

        private double CheckRange(double value, double max)
        {
            if (double.IsNaN(value))
                throw new ArgumentException();
            if (value < 0.0)
                value = 0.0;
            else if (value > max)
                value = max;
            return value;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        public string ToRGBString()
        {
            Color color = (Color)this;
            return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
        }

        public static implicit operator Color(HslColor hslColor)
        {
            double r = 0, g = 0, b = 0;
            if (hslColor._luminosity != 0)
            {
                if (hslColor._saturation == 0)
                {
                    r = g = b = hslColor._luminosity * oneOver100;
                }
                else
                {
                    HueToRGB(hslColor._hue, out r, out g, out b);

                    var sat = hslColor._saturation * oneOver100;
                    var sun = 1.0 - sat;
                    r = r * sat + sun;
                    g = g * sat + sun;
                    b = b * sat + sun;

                    var lum = hslColor._luminosity * oneOver100;
                    r *= lum;
                    g *= lum;
                    b *= lum;
                }
            }

            byte rr = (byte)(255 * r);
            byte gg = (byte)(255 * g);
            byte bb = (byte)(255 * b);

            var color = Color.FromRgb(rr, gg, bb);
            return color;
        }
        
        private static void HueToRGB(double hue, out double r, out double g, out double b)
        {
            //   000 060 120 180 240 300 360
            // r   1   1   0   0   0   1   1
            // g   0   1   1   1   0   0   0
            // b   0   0   0   1   1   1   0

            if (hue <= 60.0)
            {
                r = 1;
                g = hue * oneOver60;
                b = 0;
            }
            else if (hue <= 120.0)
            {
                var dh = 120 - hue;
                r = dh * oneOver60;
                g = 1;
                b = 0;
            }
            else if (hue <= 180)
            {
                var dh = hue - 120;
                r = 0;
                g = 1;
                b = dh * oneOver60;
            }
            else if (hue <= 240)
            {
                var dh = 240 - hue;
                r = 0;
                g = dh * oneOver60;
                b = 1;
            }
            else if (hue <= 300)
            {
                var dh = hue - 240;
                r = dh * oneOver60;
                g = 0;
                b = 1;
            }
            else
            {
                var dh = 360 - hue;
                r = 1;
                g = 0;
                b = dh * oneOver60;
            }
        }

        public HslColor() { }
        public HslColor(double hue, double saturation, double luminosity)
        {
            this.Hue = hue;
            this.Saturation = saturation;
            this.Luminosity = luminosity;
        }

    }
}
