using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    internal static class SMA_V1Ext {
        /// <summary>
        /// VERSION 1: basic raw
        /// </summary>
        internal static IObservable<double> SMA_V1(this IObservable<double> source, int period) {
//            return source.Buffer(period, 1).Where(x => x.Count == period).Select(x => x.Sum() / period);
            return source.SUM_V1(period).Select(x => x / period);
        }
    }
}
