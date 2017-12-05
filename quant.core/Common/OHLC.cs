using System;
using System.Collections.Generic;
using System.Text;

namespace quant.common
{
    /// <summary>
    /// 
    /// </summary>
    public class OHLC
    {
        /// <summary>
        /// 
        /// </summary>
        public struct Stat  {
            public DateTime Time { get; internal set; }
            public double Price { get; internal set; }
        }
        Stat _open, _close, _high, _low;
        public Stat Open => _open;
        public Stat Close => _close;
        public Stat High => _high;
        public Stat Low => _low;
        public uint Volume { get; private set; }
        public uint Count { get; private set; }
        public double VWAP { get; private set; }
        /// <summary>
        /// High to Low
        /// </summary>
        public double Range {
            get {
                return _high.Price - _low.Price;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prev"></param>
        /// <returns></returns>
        public double TR(OHLC prev) {
            double high_prevclose = Math.Abs(this._high.Price - prev._close.Price);
            double low_prevclose = Math.Abs(this._low.Price - prev._close.Price);
            return Math.Max(this.Range, Math.Max(low_prevclose, high_prevclose));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tck"></param>
        public void Add(Tick tck)   {
            // open
            if (Count == 0) {
                _open.Time = tck.Time;
                _open.Price = tck.Price;
                // set high and low
                _high = _low = _open;
            }
            // high check
            if (_high.Price < tck.Price) {
                _high.Time = tck.Time;
                _high.Price = tck.Price;
            }
            // low check
            else if (_low.Price > tck.Price) {
                _low.Time = tck.Time;
                _low.Price = tck.Price;
            }
            // close
            _close.Time = tck.Time;
            _close.Price = tck.Price;

            VWAP = (VWAP * Volume + tck.Price * tck.Quantity) / (Volume + tck.Quantity);
            Count++;
            Volume += tck.Quantity;
        }

        //public override string ToString() {
        //    return JsonConvert.SerializeObject(this);
        //}
    }
}
