using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    public static partial class QuantExt
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        // PPO : Percentage Price Oscillator
        // http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:price_oscillators_ppo
        // PPO Line: {(12-day EMA - 26-day EMA)/26-day EMA} x 100 = ((12-day EMA / 26-day EMA) - 1) * 100
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static IObservable<double> PPO(this IObservable<double> source, uint fast_period, uint slow_period)
        {
            Debug.Assert(fast_period < slow_period);
            return source.Publish(sr => sr.EMA(fast_period).WithLatestFrom(sr.EMA(slow_period), (fst, slw) => ((fst / slw) - 1) * 100));
        }

        public static IObservable<double> PPO(this IObservable<OHLC> source, uint fast_period, uint slow_period)
        {
            Debug.Assert(fast_period < slow_period);
            return null;
//            return source.Publish(sr => sr.EMA(fast_period).Zip(sr.EMA(slow_period), (fst, slw) => ((fst / slw) - 1) * 100));
        }
    }
}
