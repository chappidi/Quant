using System;
using System.Collections.Generic;
using System.Text;

namespace quant.common
{
    /// <summary>
    /// represents tick data of a given Security
    /// </summary>
    public class Tick
    {
        public enum Aggressor { NA, Buy, Sell }
        #region ctor
        public Tick(Security sec, uint qty, uint price, DateTime time) {
            Security = sec;
            Quantity = qty;
            Price = price;
            Time = time;
        }
        #endregion

        #region properties
        public Security Security { get; }
        public uint Quantity { get; }
        public uint Price { get; }
        public DateTime Time { get; }
        public Aggressor Side { get; set; } = Aggressor.NA;
        public bool Live { get; set; } = false;
        public double PxVol => Price * Quantity;
        #endregion
        public override string ToString() {
            var tm = Time.ToString("MM/dd/yyyy HH:mm:ss.fff");
            return $"[{Security}\t{tm}\t{Quantity}\t{Price}]";
        }
    }
}
