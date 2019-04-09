using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

/// <summary>
/// Exponential Moving Average
/// Initial SMA: 10-period sum / 10 
/// Multiplier: (2 / (Time periods + 1) ) = (2 / (10 + 1) ) = 0.1818 (18.18%)
/// EMA: {Close - EMA(previous day)} x multiplier + EMA(previous day)
/// EMA = Close * multiplier + EMA(previous day) x { 1 - multiplier}
/// </summary>
namespace quant.rx
{
    /// <summary>
    /// Global Extension.
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// Standard Extension
        /// </summary>
        public static IObservable<double> EMA(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return source.EMA_V2(period, offset);
        }
        /// <summary>
        /// Tick based EMA . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> EMA(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).EMA(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based EMA of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> EMA(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).EMA(period, sr.Offset());
            });
        }
    }
}
