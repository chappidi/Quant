using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    public static partial class QuantExt
    {
        /// <summary>
        /// Signal Line: 9-day EMA of (MACD / PPO)
        /// (MACD / PPO) Histogram: (MACD / PPO) - Signal Line
        /// </summary>
        /// <param name="source"></param>
        /// <param name="fast_period"></param>
        /// <param name="slow_period"></param>
        /// <returns></returns>
        public static IObservable<double> Histogram(this IObservable<double> source, uint period)
        {
            return source.Publish(sr => sr.EMA(period).WithLatestFrom(sr, (sig, pnt) => pnt - sig));
        }
    }
}
