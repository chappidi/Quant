using System;
using System.Collections.Generic;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// Hull Moving Average
    /// HMA(n) = WMA(2*WMA(n/2) – WMA(n)),sqrt(n))	
    /// http://finance4traders.blogspot.com/2009/06/how-to-calculate-hull-moving-average.html
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> HMA(this IObservable<double> source)
        {
            return null;
        }
    }
}
