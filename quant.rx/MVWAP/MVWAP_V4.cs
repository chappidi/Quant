using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    public static class MVWAPV4Ext
    {
        public static IObservable<double> MVAP_V4(this IObservable<QTY_PX> source, uint period, IObservable<double> offset)
        {
            return source.Publish(src => {
                // calculate the sum
                var sumObs = src.SelectMany(pxVl => Observable.Range(0, (int)pxVl.QTY).Select(y => pxVl.PX)).SUM(period, offset);
                // sample the results and end of each input
                return src.WithLatestFrom(sumObs, (pxVol, total) => total);
            }).Select(total => total / period);
        }
    }
}
