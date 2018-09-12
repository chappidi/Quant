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
    /// <summary>
    /// Exponential Moving Average
    /// Initial SMA: 10-period sum / 10 
    /// Multiplier: (2 / (Time periods + 1) ) = (2 / (10 + 1) ) = 0.1818 (18.18%)
    /// EMA: {Close - EMA(previous day)} x multiplier + EMA(previous day)
    /// EMA = Close * multiplier + EMA(previous day) x { 1 - multiplier}
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> EMA_X(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            double factor = 2.0 / (period + 1);
            uint count = 0;
            double ema = 0;
            return Observable.Create<double>(obs => {
                var ret = new CompositeDisposable();
                ret.Add(source.Subscribe(val => {
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
        public static IObservable<double> EMA(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs => {
                var ema = new EMA(period);
                return source.Subscribe((val) => {
                    var retVal = ema.Calc(val);
                    if (!double.IsNaN(retVal))
                        obs.OnNext(retVal);
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> EMA(this IObservable<Tuple<OHLC, OHLC>> source, uint period)
        {
            Debug.Assert(period > 1);

            return Observable.Create<double>(obs => {
                double factor = (2.0 / (period + 1));
                int count = 0;
                double ema = 0;
                return source.Subscribe((val) => {
                    var input = val.Item1.Close.Price;
                    var offset = val.Item1.Close.Price - val.Item2.Close.Price;
                    // buffer not full
                    if (count < period) {
                        if(offset != 0)
                            ema += count * offset;    // roll offset
                        count++;
                        ema += input;
                        if (count == period)
                            obs.OnNext(ema / period);
                    }
                    else {
                        if (offset != 0)
                            ema += offset;
                        ema = (input * factor) + (ema * (1.0 - factor));
                        obs.OnNext(ema);
                    }
                }, obs.OnError, obs.OnCompleted);
            });
        }
    }
}
