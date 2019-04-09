using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using quant.core;

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
        public static IObservable<double> SMA(this IObservable<double> source, uint period, IObservable<double> offset = null) {
//            return source.SMA_V2(period);
            return source.SMA_V4(period, offset);
        }
        /// <summary>
        /// Tick based SMA . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> SMA(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).SMA(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based SMA of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> SMA(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).SMA(period, sr.Offset());
            });
        }
    }
}
