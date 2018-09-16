using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
   internal static class WSMAV1Ext
    {
        public static IObservable<double> WSMA_V1(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            uint count = 0;
            double total = 0;
            return Observable.Create<double>(obs => {
                var ret = new CompositeDisposable();
                if(offset != null) {
                    ret.Add(offset.Subscribe(val => {
                        //empty not implemented yet
                    }, obs.OnError, obs.OnCompleted));
                }
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
    }
 }
