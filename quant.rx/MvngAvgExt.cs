using quant.common;
using quant.core;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace quant.rx
{
    public static class MvngAvgExt
    {
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
                return source.Subscribe( (val) => {
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> SMA(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs => {
                double total = 0;
                double count = 0;   // count of elements
                return source.RollingWindow(period).Subscribe(
                    (val) => {
                        // add to the total sum
                        total += val.Item1;
                        // buffer not full
                        if (count < period)
                            count++;
                        else
                            total -= val.Item2;

                        // count matches window size
                        if (count == period)
                            obs.OnNext(total / period);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> MVWAP(this IObservable<Tick> source, uint period) {

            return Observable.Create<double>(obs => {
                var que = new LinkedList<Tick>();
                double pxVol = 0;  // price * Vol
                uint Vol = 0;   // volume
                return source.Subscribe( (newTck) => {

                    que.AddLast(newTck);    // add to the end                    
                    pxVol += newTck.PxVol;  // add to the total sum and volume
                    Vol += newTck.Quantity;

                    // if volume exceeded the limit
                    while(Vol > period) {
                        // remove  old value
                        var oldTck = que.First.Value;
                        que.RemoveFirst();
                        if (oldTck.Quantity + period > Vol) {
                            // find amount to reduce
                            uint diff = Vol - period;
                            // add back the difference
                            que.AddFirst(new Tick("", oldTck.Quantity - diff, oldTck.Price, oldTck.Time, oldTck.Side, oldTck.Live));
                            // reduce the aggregate amounts
                            pxVol -= oldTck.Price * diff;
                            Vol -= diff;
                        }
                        else {
                            // reduce the aggregate amounts
                            pxVol -= oldTck.PxVol;
                            Vol -= oldTck.Quantity;
                        }
                    }
                    // count matches window size
                    if (Vol >= period)
                        obs.OnNext(pxVol / period);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
    }
}
