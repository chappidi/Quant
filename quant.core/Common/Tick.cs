using System;
using System.Collections.Generic;
using System.Text;

namespace quant.common
{
    public enum Aggressor { NA, Buy, Sell }
    /// <summary>
    /// 
    /// </summary>
    public struct Tick
    {
        public Tick(uint qty, double price, DateTime time, Aggressor side = Aggressor.NA, bool live = false) {
            Quantity = qty;
            Price = price;
            Side = side;
            Time = time;
            Live = live;
        }
        public uint Quantity { get; }
        public double Price { get; }
        public DateTime Time { get; }
        public Aggressor Side { get; }
        public bool Live { get; }
    }
}
