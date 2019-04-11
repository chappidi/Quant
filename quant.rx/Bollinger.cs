using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;
using quant.core;

namespace quant.rx
{
    public struct BLG_PRM
    {
        public uint Signal { get; set; }
        public double Multiplier { get; set; }
        public uint StdDev { get; set; }
    }
    public static partial class QuantExt
    {
        public static IObservable<Band> Bollinger(this IObservable<OHLC> source, BLG_PRM prm)
        {
            return source.Publish(obs => {
                return obs.SMA(prm.Signal).Zip(obs.StdDev(prm.StdDev), 
                    (mid, dev) => new Band { UPPER = mid + (prm.Multiplier * dev), MIDDLE = mid, LOWER = mid - (prm.Multiplier * dev) });
            });
        }
    }
}
