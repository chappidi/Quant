using System;
using System.Collections.Generic;
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
        public static IObservable<double> SUM(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new SUM_V2(source, period, offset);
        }
        /// <summary>
        /// Tick based SUM . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> SUM(this IObservable<Tick> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).SUM(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based SUM of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> SUM(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).SUM(period, sr.Offset());
            });
        }
    }
}
