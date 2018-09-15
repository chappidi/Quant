using System;
using System.Collections.Generic;
using System.Text;

namespace quant.common
{
    public enum MovingAvg { EMA, SMA, WSMA };

    public struct Band
    {
        public double UPPER;
        public double MIDDLE;
        public double LOWER;
        #region object
        public override string ToString() { return $"[{UPPER.ToString("0.00")}\t{MIDDLE.ToString("0.00")}\t{LOWER.ToString("0.00")}]"; }
        #endregion
        public static Band operator -(Band x, Band y) {
            return new Band {
                UPPER = x.UPPER - y.UPPER,
                MIDDLE = x.MIDDLE - y.MIDDLE,
                LOWER = x.LOWER - y.LOWER
            };
        }
    }
}
