using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.rx
{
    public static partial class QuantExt
    {
        /// <summary>
        /// ZLEMA = EMA of (close + (close-close[lag]))
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> ZLEMA(this IObservable<double> source, uint period)
        {
            int lagP = (int)(period - 1) / 2;
            lagP = lagP + 1;
            return source.Buffer(lagP, 1).Where(x => x.Count == lagP).Select(x => 2 * x[lagP - 1] - x[0]).EMA(period);
        }
        public static IObservable<double> ZLEMA(this IObservable<OHLC> source, uint period)
        {
            int lagP = (int)(period - 1) / 2;
            lagP = lagP + 1;
            return source.Buffer(lagP, 1).Where(x => x.Count == lagP).Select(x => {
                // Take into consideration of Future Roll and offset
                return (double)(2 * x[lagP - 1].Close.Price - x[0].Close.Price);
            }).EMA(period);
        }
    }
}
