using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace quant.core
{
    public static class BucketExt
    {
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
