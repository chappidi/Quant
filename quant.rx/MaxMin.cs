using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.rx
{
    /// <summary>
    /// Value and its position in the Moving List
    /// </summary>
    public class DATA_POS<TSource>
    {
        public TSource Data { get; set; }
        public uint Pos { get; set; } = 0;
        public DATA_POS(TSource val) { Data = val; }
    }
    /// <summary>
    /// https://stackoverflow.com/questions/14823713/efficient-rolling-max-and-min-window
    /// </summary>
    class MaxMin : IObservable<DATA_POS<double>>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly Func<double, double, bool> _func;
        readonly uint _period;
        // variables
        readonly LinkedList<DATA_POS<double>> lnkQue = new LinkedList<DATA_POS<double>>();
        readonly RingWnd<double> _ring = null;          // buffer of elements
        uint _count = 0;                                // count of elements

        #region ctor
        public MaxMin(IObservable<double> source, uint period, Func<double, double, bool> func, IObservable<double> offset = null) {
            _source = source;
            _offset = offset;
            _period = period;
            _func = func;

            _ring = new RingWnd<double>(period);
        }
        #endregion
        void OnNext(double newVal, double oldVal)
        {
            //Step 1: remove the unfit ones from priority que
            while (lnkQue.Last != null)
            {
                // GreaterThan for Max;  LessThan for Min
                if (_func(newVal, lnkQue.Last.Value.Data))
                    lnkQue.RemoveLast();
                else
                    break;
            }
            // Que.First.Value == deqVal
            //Math.Abs(Q.First.Value - val.Item2) < 0.0000001

            //if (lnkQue.First != null && _count >= _period) 
            if (lnkQue.First != null)
            {
                if (EqualityComparer<double>.Default.Equals(lnkQue.First.Value.Data, oldVal))
                    lnkQue.RemoveFirst();
            }
            // Step 4: New Item is going to be added, adjust position index to reflect it
            foreach (var itm in lnkQue) {
                itm.Pos++;
            }
            // Step 5: cache item and its position
            lnkQue.AddLast(new DATA_POS<double>(newVal));
        }
        void OnNext(double newVal, IObserver<DATA_POS<double>> obsvr)
        {
            OnNext(newVal, _ring.Enqueue(newVal));
            //send outgoing
            if (_count >= (_period - 1)) {
                obsvr.OnNext(lnkQue.First.Value);
            } else {
                _count++;
            }
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<DATA_POS<double>> obsvr)
        {
            var ret = new CompositeDisposable();
            if (_offset != null) {
                ret.Add(_offset.Subscribe(ofst => {
                    // adjust add the values in buffer
                    for (int itr = 0; itr < _count; ++itr) {
                        long idx = (_ring.head + itr) % _period;
                        _ring.buffer[idx] += ofst;
                    }
                    // adjust values in lnkQue
                    foreach (var itm in lnkQue) {
                        itm.Data += ofst;
                    }
                }));
            }
            ret.Add(_source.Subscribe(val => OnNext(val, obsvr), obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion
    }
    /// <summary>
    /// Local Extension.
    /// </summary>
    internal static class MaxMinExt
    {
        internal static IObservable<double> ABC(this IObservable<double> source, uint period, Func<double, double, bool> func)
        {
            return Observable.Create<double>(obs => {
                var que = new LinkedList<double>();
                double count = 0;   // count of elements
                return source.RollingWindow(period).Subscribe(val => {
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
        internal static IObservable<double> Max_V1(this IObservable<double> source, int period)
        {
            return source.Buffer(period, 1).Where(x => x.Count == period).Select(x => x.Max());
        }
        /// <summary>
        /// basic implementation
        /// </summary>
        internal static IObservable<double> Min_V1(this IObservable<double> source, int period)
        {
            return source.Buffer(period, 1).Where(x => x.Count == period).Select(x => x.Min());
        }
        /// <summary>
        /// No adjustments for Roll
        /// </summary>
        internal static IObservable<double> Max_V2(this IObservable<double> source, uint period)
        {
            return source.ABC(period, (x, y) => ((x - y) > 0.0000001));
        }
        internal static IObservable<double> Min_V2(this IObservable<double> source, uint period)
        {
            return source.ABC(period, (x, y) => ((x - y) < 0.0000001));
        }
        /// <summary>
        /// Implementation with support to adjust for future rolls
        /// </summary>
        internal static IObservable<DATA_POS<double>> Max_V3(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new MaxMin(source, period, (x, y) => ((x - y) > 0.0000001), offset);
        }
        internal static IObservable<DATA_POS<double>> Min_V3(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new MaxMin(source, period, (x, y) => ((x - y) < 0.0000001), offset);
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
        public static IObservable<double> Max(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return source.Max_V2(period);
        }
        /// <summary>
        /// Tick based Max . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> Max(this IObservable<Tick> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).Max(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based Max of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> Max(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).Max(period, sr.Offset());
            });
        }

        /// <summary>
        /// Standard Extension of Min
        /// </summary>
        public static IObservable<double> Min(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return source.Min_V2(period);
        }
        /// <summary>
        /// Tick based Min . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> Min(this IObservable<Tick> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).Min(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based Min of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> Min(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).Min(period, sr.Offset());
            });
        }
    }
}
