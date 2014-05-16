using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;


namespace PointGaming.Voice
{
    class DateTimePrecise
    {
        private static long _startTick;
        private static Stopwatch _stopWatch;

        static DateTimePrecise()
        {
            _startTick = DateTime.UtcNow.Ticks;
            _stopWatch = Stopwatch.StartNew();
        }

        public static long UtcNowTicks
        {
            get
            {
                return _startTick + _stopWatch.ElapsedTicks;
            }
        }

        public static DateTime UtcNow
        {
            get
            {
                return new DateTime(_startTick + _stopWatch.ElapsedTicks, DateTimeKind.Utc);
            }
        }
    }
}
