using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using quant.common;
using quant.core;

namespace quant.rx
{
    public static partial class QuantExt
    {
        public static IObservable<double> WSMA(this IObservable<Tuple<OHLC, OHLC>> source, uint period)
        {
            Debug.Assert(period > 1);
            return Observable.Create<double>(obs => {
                int count = 0;
                double wsma = 0;
                return source.Subscribe(val => {
                    var offset = val.Item1.Close.Price - val.Item2.Close.Price;
                    var input = val.Item1.Close.Price;
                    if (offset != 0)
                        wsma += count * offset;    // roll offset
                    // buffer not full
                    if (count < period) {
                        count++;
                        wsma += input;
                        if (count == period)
                            obs.OnNext(wsma / period);
                    } else {
                        Debug.Assert(count == period);
                        double wsma_one = wsma / period;
                        wsma = (wsma - wsma_one + input);
                        obs.OnNext(wsma / period);
                    }
                }, obs.OnError, obs.OnCompleted);
            });
        }
        public static IObservable<double> WSMA_X(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs => {
                uint count = 0;
                double total = 0;

                return source.Subscribe((val) => {
                    if (period == 1)
                    {
                        obs.OnNext(val);
                        return;
                    }
                    // buffer not full
                    if (count < period)
                    {
                        ++count;
                        total += val;
                        if (count == period)
                            obs.OnNext(total / period);
                    }
                    else
                    {
                        double wsma_one = total / period;
                        total = (total - wsma_one + val);
                        obs.OnNext(total / period);
                    }
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> WSMA(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs => {
                var ema = new WSMA(period);
                return source.Subscribe( (val) => {
                        var retVal = ema.Calc(val);
                        if (!double.IsNaN(retVal))
                            obs.OnNext(retVal);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
    }
}
