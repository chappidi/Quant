using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    public interface IOperator<in T>
    {
        bool Check(T x, T y);
    }

    internal sealed class GreaterThan : IOperator<int>, IOperator<uint>, IOperator<double>
    {
        public bool Check(int x, int y) => (x > y);
        public bool Check(uint x, uint y) => (x > y);
        public bool Check(double x, double y) => (x - y) > 0.0000001;
    }
    internal sealed class LessThan : IOperator<int>, IOperator<uint>, IOperator<double>
    {
        public bool Check(int x, int y) => (x < y);
        public bool Check(uint x, uint y) => (x < y);
        public bool Check(double x, double y) => (x - y) < 0.0000001;
    }
    /// <summary>
    /// Local Extension.
    /// </summary>
    internal static class MaxMinExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        internal static IObservable<double> ABC(this IObservable<double> source, uint period, Func<double, double, bool> func)
        {
            return Observable.Create<double>(obs => {
                var que = new LinkedList<double>();
                double count = 0;   // count of elements
                return source.RollingWindow(period).Subscribe(
                    (val) => {
                        // val < Que.Last.Value
                        while (que.Last != null && func(val.Item1, que.Last.Value))
                            que.RemoveLast();
                        // Que.First.Value == deqVal
                        //Math.Abs(Q.First.Value - val.Item2) < 0.0000001
                        if (que.First != null && EqualityComparer<double>.Default.Equals(que.First.Value, val.Item2))
                            que.RemoveFirst();

                        que.AddLast(val.Item1);

                        if (count >= (period - 1))
                            obs.OnNext(que.First.Value);
                        else
                            count++;
                    }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// basic Implementation
        /// </summary>
        internal static IObservable<double> Max_V1(this IObservable<double> source, int period) {
            return source.Buffer(period, 1).Where(x => x.Count == period).Select(x => x.Max());
        }
        /// <summary>
        /// basic implementation
        /// </summary>
        internal static IObservable<double> Min_V1(this IObservable<double> source, int period) {
            return source.Buffer(period, 1).Where(x => x.Count == period).Select(x => x.Min());
        }

        internal static IObservable<double> Max_V2(this IObservable<double> source, uint period)
        {
            return source.ABC(period, (x, y) => ((x - y) > 0.0000001));
        }
        internal static IObservable<double> Min_V2(this IObservable<double> source, uint period)
        {
            return source.ABC(period, (x, y) => ((x - y) < 0.0000001));
        }
    }
    /// <summary>
    /// Global Extensions
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// Standard Extension of Max
        /// </summary>
        public static IObservable<double> Max(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return source.Max_V2(period);
        }
        /// <summary>
        /// Tick based Max . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> Max(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).Max(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based Max of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> Max(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).Max(period, sr.Offset());
            });
        }

        /// <summary>
        /// Standard Extension of Min
        /// </summary>
        public static IObservable<double> Min(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return source.Min_V2(period);
        }
        /// <summary>
        /// Tick based Min . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> Min(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).Min(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based Min of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> Min(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).Min(period, sr.Offset());
            });
        }
    }
}
