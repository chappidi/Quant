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
            var symbol = new Security(data[1]);
            Open = new Tick(symbol,1, uint.Parse(data[3]), DateTime.Parse(data[2]));
            High = new Tick(symbol, 1, uint.Parse(data[5]), DateTime.Parse(data[4]));
            Low = new Tick(symbol, 1, uint.Parse(data[7]), DateTime.Parse(data[6]));
            Close = new Tick(symbol, 1, uint.Parse(data[9]), DateTime.Parse(data[8]));
            Volume = uint.Parse(data[10]);
            VWAP = double.Parse(data[11]);
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
        public Tick Open    { get; private set; }
        public Tick Close   { get; private set; }
        public Tick High    { get; private set; }
        public Tick Low     { get; private set; }
        public uint Volume  { get; private set; }
        public uint Count   { get; private set; }
        public double VWAP  { get; private set; }
        /// <summary>
        /// High to Low
        /// </summary>
        public uint Range => High.Price - Low.Price;
        /// <summary>
        /// Roll Offset
        /// </summary>
        public int Offset { get; }
        #endregion

        /// <summary>
        /// TODO: better formula to calculate VWAP. to prevent overflow
        /// </summary>
        /// <param name="tck"></param>
        void updateStats(Tick tck) {
            VWAP = (VWAP * Volume + tck.PxVol) / (Volume + tck.Quantity);
            Count++;
            Volume += tck.Quantity;
        }
        /// <summary>
        /// 
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
        /// </summary>
        /// <param name="tck"></param>
        public void Add(Tick tck)
        {
            if (High.Price < tck.Price) 
                High = tck;
            else if (Low.Price > tck.Price) 
                Low = tck;

            // Calculate Offset
            if(tck.Security != Close.Security) {

            }

            Close = tck;
            updateStats(tck);
        }

        //public override string ToString() {
        //    return JsonConvert.SerializeObject(this);
        //}
    }
}
