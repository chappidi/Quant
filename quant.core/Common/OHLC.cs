using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace quant.common
{
    /// <summary>
    /// 
    /// </summary>
    public class OHLC
    {
        static string codes = "_FGHJKMNQUVXZ";
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
        public string Symbol { get; }
        public int MonthCode { get; }
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
        /// <param name="offset"></param>
        /// <returns></returns>
        public double TR(OHLC prev, double offset = 0)
        {
            Debug.Assert((offset != 0) ? (prev.Symbol != this.Symbol) : true);
            // adjusted for Roll.
            double adj_prevClose = prev._close.Price + offset;
            double high_prevclose = Math.Abs(this._high.Price - adj_prevClose);
            double low_prevclose = Math.Abs(this._low.Price - adj_prevClose);
            return Math.Max(this.Range, Math.Max(low_prevclose, high_prevclose));
        }
        /// <summary>
        /// To Create from CSV
        /// </summary>
        /// <param name="data"></param>
        public OHLC(string[] data)
        {
            Symbol = data[1];
            MonthCode = (10 + Symbol[3] - '0') * 100 + codes.IndexOf(Symbol[2]);
            _open.Time = DateTime.Parse(data[2]);
            _open.Price = double.Parse(data[3]);
            _high.Time = DateTime.Parse(data[4]);
            _high.Price = double.Parse(data[5]);
            _low.Time = DateTime.Parse(data[6]);
            _low.Price = double.Parse(data[7]);
            _close.Time = DateTime.Parse(data[8]);
            _close.Price = double.Parse(data[9]);
            Volume = uint.Parse(data[10]);
            VWAP = double.Parse(data[11]);
        }
        public OHLC(string sym) {
            Symbol = sym;
            MonthCode = (10 + Symbol[3] - '0') * 100 + codes.IndexOf(Symbol[2]);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tck"></param>
        public void Add(Tick tck)   {
            Debug.Assert(Symbol == tck.Symbol);
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
