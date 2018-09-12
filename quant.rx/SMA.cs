using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    public static partial class QuantExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> SMA(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            double total = 0;
            double count = 0;   // count of elements
            return Observable.Create<double>(obs => {
                var ret = new CompositeDisposable();
                ret.Add(source.RollingWindow(period).Subscribe(val => {
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
                    }, obs.OnError, obs.OnCompleted));

                if(offset != null) {
                    ret.Add(offset.Subscribe(val => {

                    }, obs.OnError, obs.OnCompleted));
                }
                return ret;
            });
        }
    }
}
