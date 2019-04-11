using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace quant.core
{
    public static class OHLCExt
    {
        /// <summary>
        /// This is for Stitch
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        static IObservable<IList<OHLC>> OHLC(this IObservable<IObservable<Tick>> source)
        {
            return source.SelectMany(p => p.Aggregate((OHLC)null,
                (ohlc, td) => {
                    if (ohlc == null)
                        ohlc = new OHLC(td);
                    else
                        ohlc.Add(td);
                    return ohlc;
                })).ToList();
        }
        internal static IObservable<IObservable<TSource>> Slice<TKey, TSource>(this IObservable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.Publish(xs => xs.GroupByUntil(k => keySelector(k), g => {
                return xs.Where(x => !EqualityComparer<TKey>.Default.Equals(keySelector(x), g.Key));
//                return xs.Where(x => keySelector(x) != g.Key);
            }));
        }
        /// <summary>
        /// replacement to System.Reactive.Window
        /// </summary>
        /// <typeparam name="TWindowBoundary"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="bound"></param>
        /// <returns></returns>
        internal static IObservable<IObservable<TSource>> Slice<TWindowBoundary, TSource>(this IObservable<TSource> source, IObservable<TWindowBoundary> bound)
        {
            object lck = new object();
            Subject<TSource> ot = null;
            return Observable.Create<IObservable<TSource>>(obs => {
                var ret = new CompositeDisposable();
                // boundaries to slice
                ret.Add(bound.Subscribe(bnd => {
                    lock (lck) {
                        ot?.OnCompleted();
                        ot = null;
                    }
                }));
                // source to be sliced
                ret.Add(source.Subscribe(tck => {
                    lock (lck) {
                        if (ot == null) {
                            ot = new Subject<TSource>();
                            obs.OnNext(ot);
                        }
                        ot.OnNext(tck);
                    }
                }, err => { ot?.OnError(err); obs.OnError(err); }, () => { ot?.OnCompleted(); obs.OnCompleted(); }));
                return ret;
            });
//            return source.Window(bound);
        }
        public static IObservable<IList<OHLC>> Bucket_1(this IObservable<Tick> source, TimeSpan period)
        {
            return source.Slice(x => x.TradedAt.Ticks / period.Ticks)
                .SelectMany(grp => grp.GroupBy(tk => tk.Security.Symbol).OHLC());
        }
        public static IObservable<IList<OHLC>> Bucket_2(this IObservable<Tick> source, TimeSpan period)
        {
            return source.Publish(xs => {
                var bound = xs.Select(x => x.TradedAt.Ticks / period.Ticks).DistinctUntilChanged();
                return xs.Slice(bound);
            }).SelectMany(grp => grp.GroupBy(tk => tk.Security.Symbol).OHLC());
        }
        public static IObservable<IList<OHLC>> Bucket_2(this IObservable<IObservable<Tick>> source, TimeSpan period)
        {
            return source.SelectMany(x => x).Bucket_2(period);
        }
    }
}
