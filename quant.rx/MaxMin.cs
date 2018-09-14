using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

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
        public static IObservable<double> Max(this IObservable<double> source, uint period) {
            return source.Max_V2(period);
        }
        public static IObservable<double> Min(this IObservable<double> source, uint period) {
            return source.Min_V2(period);
        }
    }
}
