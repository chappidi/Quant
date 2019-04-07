using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    /// <summary>
    /// https://www.tradingtechnologies.com/xtrader-help/x-study/technical-indicator-definitions/acceleration-bands-abands/
    /// Upper Band = SMA(High * ( 1 + 4 * (High - Low) / (High + Low)))
    /// Middle Band = SMA
    /// Lower Band = SMA(Low * (1 - 4 * (High - Low)/ (High + Low)))    
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<Band> ABANDS(this IObservable<OHLC> source, uint period, uint width)
        {
            return source.Publish(sc => {
                var ftrObs = sc.Select(oh => width * ((double)(oh.High.Price - oh.Low.Price)) / (oh.High.Price + oh.Low.Price));
                return ftrObs.Publish(xs => {
                    var upr = sc.Zip(xs, (oh, f) => oh.High.Price * (1.0 + f)).SMA(period/*, sc.Offset()*/);
                    var lwr = sc.Zip(xs, (oh, f) => oh.Low.Price * (1.0 - f)).SMA(period/*, sc.Offset()*/);
                    var mid = sc.SMA(20);
                    return Observable.When(upr.And(mid).And(lwr).Then((u, m, l) => new Band() { UPPER = u, MIDDLE = m, LOWER = l }));
                });
            });
        }
    }
}
