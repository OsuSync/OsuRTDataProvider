using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuRTDataProvider.Listen
{
    public struct ErrorStatisticsResult : IEquatable<ErrorStatisticsResult>
    {
        public static ErrorStatisticsResult Empty => new ErrorStatisticsResult();
        public double ErrorMin, ErrorMax;
        public double UnstableRate;

        public bool Equals(ErrorStatisticsResult other)
        {
            return Math.Abs(other.ErrorMin - ErrorMin) < 1e-6 &&
                   Math.Abs(other.ErrorMax - ErrorMax) < 1e-6 &&
                   Math.Abs(other.UnstableRate - UnstableRate) < 1e-6;
        }
    }
}
