using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    public static class MVWAPV4Ext
    {
        static IObservable<double> Repeat(this IObservable<QTY_PX> source) {
            return Observable.Create<double>(obs => {
                return source.Subscribe(val => {
                    for(int itr=0; itr < val.QTY; itr++)
                        obs.OnNext(val.PX);
                }, obs.OnError, obs.OnCompleted);
            });
        }
        public static IObservable<double> MVWAP_V4(this IObservable<QTY_PX> source, uint period, IObservable<double> offset = null)
        {
            return source.Publish(src => {
                // calculate the sum and sample the results at end of each input 
                return src.WithLatestFrom(src.Repeat().SUM(period, offset), (pxVol, total) => total);
            }).Select(total => total / period);
        } 
    }
}
