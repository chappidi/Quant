using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// Hull Moving Average
    /// WMA ==> means LWMA
    /// HMA(n) = WMA(2*WMA(n/2) – WMA(n)),sqrt(n))	
    /// http://finance4traders.blogspot.com/2009/06/how-to-calculate-hull-moving-average.html
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> HMA(this IObservable<double> source, uint period)
        {
            var kp = (uint)Math.Sqrt(period);
            return source.Publish(sc=> {
                return sc.LWMA(period / 2).WithLatestFrom(sc.LWMA(period), (hlf, ful) => 2 * hlf - ful).LWMA(kp);
            });
        }
    }
}
