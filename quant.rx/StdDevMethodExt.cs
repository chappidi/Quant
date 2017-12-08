using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    public static class StdDevMethodExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Variance(this IList<double> values)
        {
            // edge condition optimization
            int count = values.Count();
            if (count <= 1)
                return 0;
            //Compute the Average
            double avg = values.Average();
            //Perform the Sum of (value-avg)^2
            double sum = values.Sum(d => (d - avg) * (d - avg));
            return sum / count;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double StdDev(this IList<double> values) {
            return Math.Sqrt(values.Variance());
        }
        /// <summary>
        ///             double sum = variance * N + ((newV - oldV) * (newV + oldV - oldAvg - newAvg));
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> Variance(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs => {
                double[] buffer = new double[period];
                double Avg = 0;
                double total = 0;
                uint count = 0;   // count of elements
                return source.RollingWindow(period).Subscribe(
                    (val) => {
                        if(count < period) { 
                            buffer[count] = val.Item1;
                            count++;
                            // buffer full. calculate first Variance
                            if (count == period) {
                                Avg = buffer.Average();
                                total = buffer.Sum(d => Math.Pow(d - Avg, 2));
                            }
                        }
                        else {
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
        public static IObservable<double> StdDev(this IObservable<double> source, uint period) {
            return source.Variance(period).Select(v => Math.Sqrt(v));
        }

    }
}
