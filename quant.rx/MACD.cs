using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.rx
{
    public static partial class QuantExt
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        // MACD : Moving Average Convergence and Divergence
        // http://cns.bu.edu/~gsc/CN710/fincast/Technical%20_indicators/Moving%20Average%20Convergence-Divergence%20(MACD).htm
        // http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_average_convergence_divergence_macd
        // MACD Line: (12-day EMA - 26-day EMA)
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static IObservable<double> MACD(this IObservable<double> source, uint fast_period, uint slow_period)
        {
            Debug.Assert(fast_period < slow_period);
            return source.Publish(sr => {
                return sr.EMA(fast_period).WithLatestFrom(sr.EMA(slow_period), (fst, slw) => fst - slw);
            });
        }
        public static IObservable<double> MACD(this IObservable<OHLC> source, uint fast_period, uint slow_period)
        {
            Debug.Assert(fast_period < slow_period);
            return source.Publish(sr => {
                return sr.EMA(fast_period).WithLatestFrom(sr.EMA(slow_period), (fst, slw) => fst - slw);
            });
        }
    }
}
