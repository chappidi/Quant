using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    internal static partial class EMAV1Ext
    {
        internal static IObservable<double> EMA_V1(this IObservable<double> source, uint period, IObservable<double> offset = null)
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
                        // empty not implemented
                    }, obs.OnError, obs.OnCompleted));
                }
                return ret;
            });
        }
    }
}
