using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

/// <summary>
/// Linear Weighted Moving Averages(5) 
/// 15 = 5+4+3+2+1
/// ((90.9*(5/15))+(90.36*(4/15))+(90.28*(3/15))+(90.83*(2/15))+(90.91*(1/15)))
/// </summary>
namespace quant.rx
{
    /// <summary>
    /// Global Extensions
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// Standard Extension
        /// </summary>
        public static IObservable<double> LWMA(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return source.LWMA_V2(period, offset);
        }
        /// <summary>
        /// Tick based WSMA . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> LWMA(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).WSMA(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based WSMA of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> LWMA(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).LWMA(period, sr.Offset());
            });
        }
    }
}
