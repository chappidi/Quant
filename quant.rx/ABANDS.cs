using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    /// <summary>
    /// https://www.tradingtechnologies.com/xtrader-help/x-study/technical-indicator-definitions/acceleration-bands-abands/
    /// Upper Band = Simple Moving Average (High * ( 1 + 4 * (High - Low) / (High + Low)))
    /// Middle Band = Simple Moving Average
    /// Lower Band = Simple Moving Average(Low* (1 - 4 * (High - Low)/ (High + Low)))
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<Band> ABAND(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(obs => {
                var factor = obs.Select(oh => 4 * ((double)(oh.High.Price - oh.Low.Price)) / ((double)(oh.High.Price + oh.Low.Price)));
                factor.Publish(xs => {
                    var lbx = obs.Zip(xs, (oh, f) => oh.Low.Price * (1.0 - f));
                    var mbx = obs.Zip(xs, (oh, f) => oh.High.Price * (1.0 + f));
                    return obs.Zip(xs, (oh, f) => oh.High.Price * (1.0 + f));
                });
                var mb = obs.Select(x => (double)x.Close.Price);
                var ub = obs.Select(oh => oh.High.Price * (1 + 4 * ((double)(oh.High.Price - oh.Low.Price)) / (oh.High.Price + oh.Low.Price)));
                var lb = obs.Select(oh => oh.Low.Price * (1 - 4 * ((double)(oh.High.Price - oh.Low.Price)) / (oh.High.Price + oh.Low.Price)));

                var xyz = ub.SMA(period, obs.Offset()).And(mb.SMA(period, obs.Offset())).And(lb.SMA(period, obs.Offset()));
                return Observable.When(xyz.Then((u, m, l) => new Band { UPPER = u, MIDDLE = m, LOWER = l }));
            });
        }
    }
}
