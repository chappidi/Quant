using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
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
    class Variance : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly uint _period;
        // variables
        readonly RingWnd<double> _ring = null;          // buffer of elements
        uint _count = 0;
        double _total = 0;
        double _Avg = 0;

        #region ctor
        public Variance(IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            _source = source;
            _offset = offset;
            _period = period;

            _ring = new RingWnd<double>(period);
        }
        #endregion
        void OnVal(double newVal, double oldVal, IObserver<double> obsvr)
        {
            if (_count < _period) {
                _count++;
                // buffer full. calculate first Variance
                if (_count == _period) {
                    _Avg = _ring.buffer.Average();
                    _total = _ring.buffer.Sum(d => Math.Pow(d - _Avg, 2));
                }
            }
            else {
                var oldAvg = _Avg;
                var delta = (newVal - oldVal) / _period;
                var newAvg = _Avg = oldAvg + delta;
                _total += (newVal - oldVal) * (newVal + oldVal - oldAvg - newAvg);
            }
            if (_count == _period) 
                obsvr.OnNext(_total / _period);
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr) {
            var ret = new CompositeDisposable();
            if (_offset != null) {
                ret.Add(_offset.Subscribe(ofst => {
                    Debug.Assert(false, "Needs to Implement");
                    //empty for now
                }));
            }
            ret.Add(_source.Subscribe(val => OnVal(val, _ring.Enqueue(val), obsvr), obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// local extensions
    /// </summary>
    public static class StdDevExt
    {
        internal static double Variance(this IList<double> values)
        {
            // edge condition optimization
            if (values.Count <= 1)  return 0;
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
                return source.RollingWindow(period).Subscribe(val => {
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
        internal static IObservable<double> StdDev_V3(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new Variance(source, period, offset).Select(v => Math.Sqrt(v));
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
