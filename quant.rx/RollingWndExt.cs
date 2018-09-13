using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace quant.rx
{
   /// <summary>
    /// base class to implement sliding window
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RingWnd<T> {
        readonly uint period;
        internal readonly T[] buffer = null;
        public uint head { get; private set; } = 0;

        public RingWnd(uint period) {
            this.period = period;
            buffer = new T[period];
        }
        public T Enqueue(T item) {
            //oldVal = default(T) if the buffer is not full
            T oldVal = buffer[head];
            buffer[head] = item;
            head = (head + 1) % period;
            return oldVal;
        }
    }
    public static class RollingWndExt
    {
        internal static IObservable<Tuple<TSource, TSource>> RollingWindowX<TSource>(this IObservable<TSource> source, uint period)
        {
            RingWnd<TSource> ring = new RingWnd<TSource>(period);
            return Observable.Create<Tuple<TSource, TSource>>(obs => {
                return source.Subscribe(newVal => {
                    obs.OnNext(new Tuple<TSource, TSource>(newVal, ring.Enqueue(newVal)));
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<Tuple<TSource, TSource>> RollingWindow<TSource>(this IObservable<TSource> source, uint period)
        {
            return Observable.Create<Tuple<TSource, TSource>>(obs => {
                TSource[] buffer = new TSource[period];
                uint count = 0;          // items in buffer
                uint head  = 0;          // enque here
                //Create Subscription
                return source.Subscribe((val) => {
                    //TSource oldVal = Interlocked.Exchange(ref buffer[head], val);
                    TSource oldVal = buffer[head];
                    buffer[head] = val;
                    head = (head + 1) % period;
                    // increment items in buffer till the period
                    if (count < period)  count++;
                    obs.OnNext(new Tuple<TSource, TSource>(val, oldVal));
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// DEPRICATED. Already Supported by Buffer(period,1). 
        /// Keeping it for study purpose.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        internal static IObservable<IList<TSource>> RollingBuffer<TSource>(this IObservable<TSource> source, uint period)
        {
            // return source.Buffer(period,1);

            return Observable.Create<IList<TSource>>(obs => {
                LinkedList<TSource> buffer = new LinkedList<TSource>();
                uint count = 0;          // items in buffer
                //Create Subscription
                return source.Subscribe((val) => {
                    if (count >= period)
                        buffer.RemoveFirst();
                    else
                        count++;
                    buffer.AddLast(val);
                    obs.OnNext(buffer.ToList());
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        static IObservable<double> ABC(this IObservable<double> source, uint period, Func<double, double, bool> func)
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
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> Max(this IObservable<double> source, uint period) {
            return source.ABC(period, (x, y) => ((x - y) > 0.0000001));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> Min(this IObservable<double> source, uint period) {
            return source.ABC(period, (x, y) => ((x - y) < 0.0000001));
        }

    }
}
