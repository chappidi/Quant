using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace quant.common
{
    /// <summary>
    /// OHLC = Open High Low Close Ticks
    /// bar can be on Interval, VWAP, Volume, Price 
    /// Open.Security != Close.Security is possible because of Roll
    /// Offset represents the Offset between the roll of the securities
    /// Offset == 0 if Open.Security == Close.Security
    /// </summary>
    public class OHLC
    {
        #region ctor
        /// <summary>
        /// To Create from CSV
        /// </summary>
        /// <param name="data"></param>
        public OHLC(string[] data) {
            var symbol = Security.Lookup(data[1]);
            Open = new Tick(symbol,1, uint.Parse(data[3]), DateTime.Parse(data[2]));
            High = new Tick(symbol, 1, uint.Parse(data[5]), DateTime.Parse(data[4]));
            Low = new Tick(symbol, 1, uint.Parse(data[7]), DateTime.Parse(data[6]));
            Close = new Tick(symbol, 1, uint.Parse(data[9]), DateTime.Parse(data[8]));
            Volume = uint.Parse(data[10]);
            PxVol = double.Parse(data[11]) * Volume;
        }
        /// <summary>
        /// need to have atleast one tick
        /// </summary>
        public OHLC(Tick tck) {
            High = Low = Open = Close = tck;
            updateStats(tck);
        }
        #endregion

        #region properties
        public Tick Open    { get; private set; } = null;
        public Tick Close   { get; private set; } = null;
        public Tick High    { get; private set; } = null;
        public Tick Low     { get; private set; } = null;
        public uint Volume  { get; private set; } = 0;
        public uint Count   { get; private set; } = 0;
        public double PxVol { get; private set; } = 0;
        public int Offset   { get; private set; } = 0;

        public double VWAP    => PxVol / Volume;
        public uint Range => High.Price - Low.Price;
        #endregion

        void updateStats(Tick tck) {
            Count++;
            Volume += tck.Quantity;
            PxVol += tck.PxVol;
        }
        /// <summary>
        /// TODO: Remove
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public long TR(OHLC prev)
        {
            // To do check the logic
            // adjusted for Roll.
            var adj_prevClose = prev.Close.Price + Offset;
            var high_prevclose = Math.Abs(this.High.Price - adj_prevClose);
            var low_prevclose = Math.Abs(this.Low.Price - adj_prevClose);
            return Math.Max(this.Range, Math.Max(low_prevclose, high_prevclose));
        }
        /// <summary>
        /// Add a tick to the OHLC bar
        /// update high, low, close ticks
        /// update stats.
        /// </summary>
        /// <param name="tck"></param>
        public void Add(Tick tck)
        {
            if (High.Price < tck.Price) 
                High = tck;
            else if (Low.Price > tck.Price) 
                Low = tck;

            // Calculate Offset.
            if(tck.Security != Close.Security) {
                // find the offset
                var diff = (int)(tck.Price - Close.Price);
                // increment offset if there are multiple rolls
                Offset += diff;
                // adjust pxVol to reflect continuous pricing
                PxVol += Volume * diff;
            }

            Close = tck;
            updateStats(tck);
        }

        #region Object
        public override string ToString() {
            var opn = Open.Time.ToString("MM/dd/yyyy HH:mm");
            var cls = Close.Time.ToString("MM/dd/yyyy HH:mm");
            return ($"OHLC:\t{Close.Security}\t[{opn} : {cls}]\t[O:{Open.Price} H:{High.Price} L:{Low.Price} C:{Close.Price} V:{Volume}]");
        }
        #endregion
    }
}
