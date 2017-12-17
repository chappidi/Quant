using System;
using System.Collections.Generic;
using System.Text;

namespace quant.common
{
    public enum Aggressor { NA, Buy, Sell }
    /// <summary>
    /// 
    /// </summary>
    public class Tick
    {
        public Tick(string sym, uint qty, double price, DateTime time, Aggressor side = Aggressor.NA, bool live = false) {
            Symbol = sym;
            Quantity = qty;
            Price = price;
            Side = side;
            Time = time;
            Live = live;
        }
        public string Symbol { get; }
        public uint Quantity { get; }
        public double Price { get; }
        public DateTime Time { get; }
        public Aggressor Side { get; }
        public bool Live { get; }

        public double PxVol => Price * Quantity;
    }
}
