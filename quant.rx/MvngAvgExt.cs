using quant.core;
using System;
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
    }
}
