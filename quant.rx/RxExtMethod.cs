using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.rx
{
    public static class RxExtMethod
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> RSI(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs => {
                var ema = new RSI(period);
                return source.Subscribe(
                    (val) => {
                        var retVal = ema.Calc(val);
                        if (!double.IsNaN(retVal))
                            obs.OnNext(retVal);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
    }
}
