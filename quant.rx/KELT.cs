using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    public struct KLT_PRM
    {
        public uint Signal { get; set; }
        public double Multiplier { get; set; }
        public uint ATR { get; set; }
    }
    public static partial class QuantExt
    {
        public static IObservable<Band> KELT(this IObservable<OHLC> source, KLT_PRM prm)
        {
            return source.Publish(obs => {
                return obs.SMA(prm.Signal).Skip(1).Zip( obs.TR().SMA(prm.ATR), 
                    (mid, atr) => new Band { UPPER = mid + (prm.Multiplier * atr), MIDDLE = mid, LOWER = mid - (prm.Multiplier * atr) });
            });
        }
    }
}
