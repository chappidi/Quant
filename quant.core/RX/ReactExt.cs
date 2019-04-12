using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace quant.core
{
    /// <summary>
    /// Common Reactive Extensions
    /// </summary>
    public static class ReactExt
    {
        /// <summary>
        /// Filters the input stream based on stream of filter inputs
        /// </summary>
        /// <param name="obsTick"></param>
        /// <param name="fltr"></param>
        /// <returns></returns>
        public static IObservable<Tick> Where(this IObservable<Tick> source, IObservable<Security> fltr) {
            object lck = new object();
            Security latest = null;
            return Observable.Create<Tick>(obs => {
                var ret = new CompositeDisposable();
                // capture the latest filter value
                ret.Add(fltr.Subscribe(val => { latest = val; }));
                // pass through only which matches the latest filter value
                ret.Add(source.Where(tk=> tk.Security == latest).Subscribe(obs));
                return ret;
            });
        }
        /// <summary>
        /// replacement to System.Reactive.Window
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IObservable<IObservable<TSource>> Slice<TKey, TSource>(this IObservable<TSource> source, Func<TSource, TKey> keySelector)
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
        public static IObservable<IObservable<TSource>> Slice<TWindowBoundary, TSource>(this IObservable<TSource> source, IObservable<TWindowBoundary> bound)
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
   }
}
