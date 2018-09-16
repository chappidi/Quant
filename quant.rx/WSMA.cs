using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

/// <summary>
/// Wilder’s Smoothing AKA SMoothed Moving Average (WSMA)
/// SUM1=SUM (CLOSE, N)
/// WSMA1 = SUM1/ N
/// WSMA (i) = (SUM1 – WSMA1 + CLOSE(i) )/ N =  ( (WSMA1 * (N-1)) + CLOSE(i) )/N
///
/// The WSMA is almost identical to an EMA of twice the look back period. 
/// In other words, 20-period WSMA is almost identical to a 40-period EMA
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
        public static IObservable<double> WSMA(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return source.WSMA_V2(period, offset);
        }
        /// <summary>
        /// Tick based WSMA . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> WSMA(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).WSMA(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based WSMA of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> WSMA(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).WSMA(period, sr.Offset());
            });
        }

    }
}
