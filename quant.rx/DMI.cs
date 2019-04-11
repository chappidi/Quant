using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.rx
{
    /// <summary>
    /// Directional Movement Indicator (DMI)
    ///  1. +DI
    ///  2. -DI
    ///  3. ADX
    /// https://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:average_directional_index_adx
    /// Excel Data
    /// https://stockcharts.com/school/lib/exe/fetch.php?media=chart_school:technical_indicators_and_overlays:average_directional_index_adx:cs-adx.xls
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// Directional Momentum
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static IObservable<(int hr, int tr, int lr)> DM(this IObservable<OHLC> source)
        {
            return Observable.Create<(int, int, int)>(obs => {
                OHLC prev = null;
                return source.Subscribe(val => {
                    if (prev != null)
                        obs.OnNext(val.DM(prev));
                    prev = val;
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// Directional Momentum Indicator (+DI, -DI)
        /// </summary>
        public static IObservable<(double plsDI, double mnsDI)> DMI(this IObservable<OHLC> source, uint period)
        {
            return source.DM().Publish(sc => {
                var trObs = sc.Select(x => (double)x.tr).WSMA(period).Select(x => x * period);
                var plsObs = sc.Select(x => (x.hr > x.lr) ? Math.Max((double)x.hr, 0) : 0).WSMA(period).Select(x => x * period);
                var mnsObs = sc.Select(x => (x.lr > x.hr) ? Math.Max((double)x.lr, 0) : 0).WSMA(period).Select(x => x * period);
                return Observable.When(trObs.And(plsObs).And(mnsObs).Then((tr, plsDM, mnsDM) => ((100 * plsDM) / tr, (100 * mnsDM) / tr)));
            });
        }
    }
}
