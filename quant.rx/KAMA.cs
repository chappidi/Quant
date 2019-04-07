using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// Kaufman Adaptive Moving Average
    /// https://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:kaufman_s_adaptive_moving_average
    /// Excel Data
    /// https://stockcharts.com/school/lib/exe/fetch.php?media=chart_school:technical_indicators:kaufman_s_adaptive_moving_average:cs-kama.xls
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// Smoothing Constant
        /// </summary>
        /// <param name="source"></param>
        /// <param name="periodER"></param>
        /// <param name="periodFast"></param>
        /// <param name="periodSlow"></param>
        /// <returns></returns>
        internal static IObservable<double> SC(this IObservable<double> source, uint periodER, uint periodFast, uint periodSlow)
        {
            Debug.Assert(periodFast < periodSlow);
            double ssc = 2.0 / (periodSlow + 1);
            double fsc = 2.0 / (periodFast + 1);
            return source.Publish(sc => {
                var pdObs = sc.Buffer((int)periodER + 1, 1).Where(x => x.Count == periodER + 1).Select(x => Math.Abs(x[(int)periodER] - x[0]));
                var pvObs = sc.Buffer(2, 1).Where(x => x.Count == 2).Select(x => Math.Abs(x[1] - x[0])).SUM(periodER);
                return pdObs.Zip(pvObs, (x, y) => Math.Pow((x / y) * (fsc - ssc) + ssc, 2));
            });
        }
        public static IObservable<double> KAMA(this IObservable<double> source, uint periodER, uint periodFast, uint periodSlow)
        {
            double kama = 0;
            double? SC = null;
            return source.Publish(sc => {
                return Observable.Create<double>(obs => {
                    var ret = new CompositeDisposable();
                    ret.Add(sc.SC(periodER, periodFast, periodSlow).Subscribe(val => SC = val));
                    ret.Add(sc.Skip((int)periodER-1).Subscribe(px => {
                        kama = (SC != null) ? kama + SC.Value * (px - kama) : px;
                        obs.OnNext(kama);
                    }));
                    return ret;
                });
            });
        }
    }
}
