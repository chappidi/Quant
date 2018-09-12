using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    /// <summary>
    /// Exponential Moving Average
    /// Initial SMA: 10-period sum / 10 
    /// Multiplier: (2 / (Time periods + 1) ) = (2 / (10 + 1) ) = 0.1818 (18.18%)
    /// EMA: {Close - EMA(previous day)} x multiplier + EMA(previous day)
    /// EMA = Close * multiplier + EMA(previous day) x { 1 - multiplier}
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> EMA(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            double factor = 2.0 / (period + 1);
            uint count = 0;
            double ema = 0;
            return Observable.Create<double>(obs => {
                var ret = new CompositeDisposable();
                ret.Add(source.Subscribe(val => {
                    // edge condition
                    if (period == 1) { obs.OnNext(val); return; }
                    // buffer not full
                    if (count < period) {
                        // Initial SMA mode
                        ema += val;
                        ++count;
                        if (count != period)
                            return;
                        // count matches window size
                        ema = ema / count;
                    } else {
                        // normal calculation
                        ema = ema + (val - ema) * factor;
                    }
                    obs.OnNext(ema);
                }, obs.OnError, obs.OnCompleted));

                if(offset != null) {
                    ret.Add(offset.Subscribe(val => {

                    }, obs.OnError, obs.OnCompleted));
                }
                return ret;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> EMA(this IObservable<OHLC> source, uint period)
        {
            Debug.Assert(period > 1);
            return null;
        }
    }
}
