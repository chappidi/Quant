using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.rx
{
    /// <summary>
    /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:stochastic_oscillator_fast_slow_and_full
    /// http://investexcel.net/how-to-calculate-the-stochastic-oscillator/
    /// %K = (Current Close - Lowest Low)/(Highest High - Lowest Low) * 100
    /// %D = 3-day SMA of %K
    ///
    /// Lowest Low = lowest low for the look-back period
    /// Highest High = highest high for the look-back period
    /// %K is multiplied by 100 to move the decimal point two places
    /// 
    /// Fast Stochastics (14,1,3)
    /// Slow Stochastics (14,1,3)
    /// Full Stochastics (14,3,3)
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> STOCH(this IObservable<OHLC> source, uint period)
        {
            var seq = source.Publish(oh => {
                var cls = oh.Select(x => x.Close.Price).Skip((int)period - 1);
                var max = oh.Select(x => (double)x.High.Price).Max(period, oh.Offset());
                var min = oh.Select(x => (double)x.Low.Price).Min(period, oh.Offset());
                return Observable.When(cls.And(max).And(min).Then((cl, hh, ll) => new { Close = cl, HH = hh, LL = ll }));
            });
            return seq.Select(y => (100.0 * (y.Close - y.LL)) / (y.HH - y.LL));
        }
    }
}
