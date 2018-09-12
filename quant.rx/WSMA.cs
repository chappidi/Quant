using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using quant.common;
using quant.core;

namespace quant.rx
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Wilder’s Smoothing AKA SMoothed Moving Average (WSMA)
    // SUM1=SUM (CLOSE, N)
    // WSMA1 = SUM1/ N
    // WSMA (i) = (SUM1 – WSMA1 + CLOSE(i) )/ N =  ( (WSMA1 * (N-1)) + CLOSE(i) )/N
    //
    // The WSMA is almost identical to an EMA of twice the look back period. 
    // In other words, 20-period WSMA is almost identical to a 40-period EMA
    //////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// Wilder’s Smoothing AKA SMoothed Moving Average (WSMA)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> WSMA(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            uint count = 0;
            double total = 0;
            return Observable.Create<double>(obs => {
                var ret = new CompositeDisposable();
                ret.Add(source.Subscribe(val => {
                    // edge condition
                    if (period == 1) { obs.OnNext(val); return; }
                    // buffer not full
                    if (count < period) {
                        // Initial SMA mode
                        total += val;
                        ++count;
                        if (count == period)
                            obs.OnNext(total / period);
                    } else {
                        // normal calculation
                        double wsma_one = total / period;
                        total = (total - wsma_one + val);
                        obs.OnNext(total / period);
                    }
                }, obs.OnError, obs.OnCompleted));
                return ret;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> WSMA(this IObservable<OHLC> source, uint period)
        {
            return null;
        }
    }
}
