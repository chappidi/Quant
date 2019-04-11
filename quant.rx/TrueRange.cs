using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.rx
{
    public static partial class QuantExt
    {
        /// <summary>
        /// True Range between bars
        /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:average_true_range_atr
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<double> TR(this IObservable<OHLC> source)
        {
            return Observable.Create<double>(obs => {
                OHLC prev = null;
                return source.Subscribe(val => {
                    if (prev != null)
                        obs.OnNext(val.TR(prev));
                    prev = val;
                }, obs.OnError, obs.OnCompleted);
            });
        }
    }
}
