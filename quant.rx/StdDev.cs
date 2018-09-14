using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using quant.common;

/// <summary>
/// http://jonisalonen.com/2014/efficient-and-accurate-rolling-standard-deviation/
/// https://stackoverflow.com/questions/5147378/rolling-variance-algorithm
/// https://www.johndcook.com/blog/standard_deviation/
/// https://www.johndcook.com/blog/skewness_kurtosis/
/// </summary>
namespace quant.rx
{
    /// <summary>
    /// local extensions
    /// </summary>
    public static class StdDevExt
    {
        internal static double Variance(this IList<double> values)
        {
            // edge condition optimization
            if (values.Count <= 1)
                return 0;
            //Compute the Average
            double avg = values.Average();
            //Perform the Sum of (value-avg)^2
            double sum = values.Sum(d => (d - avg) * (d - avg));
            return sum / values.Count;
        }
        internal static double StdDev(this IList<double> values)
        {
            return Math.Sqrt(values.Variance());
        }

        /// <summary>
        /// double sum = variance * N + ((newV - oldV) * (newV + oldV - oldAvg - newAvg));
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> Variance(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return Observable.Create<double>(obs =>
            {
                double[] buffer = new double[period];
                double Avg = 0;
                double total = 0;
                uint count = 0;   // count of elements
                return source.RollingWindow(period).Subscribe(
                    (val) =>
                    {
                        if (count < period)
                        {
                            buffer[count] = val.Item1;
                            count++;
                            // buffer full. calculate first Variance
                            if (count == period)
                            {
                                Avg = buffer.Average();
                                total = buffer.Sum(d => Math.Pow(d - Avg, 2));
                            }
                        }
                        else
                        {
                            var oldAvg = Avg;
                            var delta = (val.Item1 - val.Item2) / period;
                            var newAvg = Avg = oldAvg + delta;
                            total += ((val.Item1 - val.Item2) * (val.Item1 + val.Item2 - oldAvg - newAvg));
                        }
                        // count matches window size
                        if (count == period)
                            obs.OnNext(total / period);
                    }, obs.OnError, obs.OnCompleted);
            });
        }

        internal static IObservable<double> StdDev_V1(this IObservable<double> source, int period)
        {
            return source.Buffer(period, 1).Where(x => x.Count == period).Select(x => x.StdDev());
        }
        internal static IObservable<double> StdDev_V2(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return source.Variance(period, offset).Select(v => Math.Sqrt(v));
        }
    }
    /// <summary>
    /// Global Extensions
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> StdDev(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return source.StdDev_V2(period);
        }
        /// <summary>
        /// Tick based StdDev . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> StdDev(this IObservable<Tick> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).StdDev(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based StdDev of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> StdDev(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).StdDev(period, sr.Offset());
            });
        }
    }
}
