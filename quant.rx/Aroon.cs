using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using quant.common;

/// <summary>
/// https://chartpatterns.files.wordpress.com/2011/11/excel-indicators.xls    
/// </summary>
namespace quant.rx
{
    /// <summary>
    /// Local extensions
    /// </summary>
    internal static class ArronExt
    {
        internal static IObservable<double> AroonDown_V1(this IObservable<double> source, int period)
        {
            return source.Buffer(period, 1).Where(x => x.Count == period)
                .Select(lt => {
                    var y = lt.Min();
                    var idx = period - (lt.IndexOf(y) + 1);
                    return (period - idx) * (100.0 / period);
                });
        }
        internal static IObservable<double> AroonUp_V1(this IObservable<double> source, int period)
        {
            return source.Buffer(period, 1).Where(x => x.Count == period)
                .Select(lt => {
                    var y = lt.Max();
                    var idx = period - (lt.IndexOf(y) + 1);
                    return (period - idx) * (100.0 / period);
                });
        }
        // New Implemenation: Performance Improvement
        internal static IObservable<double> AroonDown_V2(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return source.Min_V3(period, offset).Select(x => (period - x.Pos) * (100.0 / period));
        }

        internal static IObservable<double> AroonUp_V2(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return source.Max_V3(period, offset).Select(x => (period - x.Pos) * (100.0 / period));
        }
    }
    /// <summary>
    /// Global Extensions
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> AroonDown(this IObservable<double> source, uint period)
        {
            return source.AroonDown_V2(period);
        }

        public static IObservable<double> AroonUp(this IObservable<double> source, uint period)
        {
            return source.AroonUp_V2(period);
        }
        // Tick based
        public static IObservable<double> AroonDown(this IObservable<Tick> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).AroonDown_V2(period, sr.Offset());
            });
        }
        public static IObservable<double> AroonUp(this IObservable<Tick> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).AroonUp_V2(period, sr.Offset());
            });
        }
        // OHLC based
        public static IObservable<double> AroonDown(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Low.Price).AroonDown_V2(period, sr.Offset());
            });
        }
        public static IObservable<double> AroonUp(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.High.Price).AroonUp_V2(period, sr.Offset());
            });
        }
    }
}
