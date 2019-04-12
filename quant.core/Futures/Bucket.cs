using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.futures
{
    public static class BucketExt
    {
        internal static IObservable<IList<OHLC>> Bucket_V1(this IObservable<Tick> source, TimeSpan period)
        {
            return source.Slice(x => x.TradedAt.Ticks / period.Ticks)
                .SelectMany(grp => grp.GroupBy(tk => tk.Security.Symbol).OHLC());
        }
        internal static IObservable<IList<OHLC>> Bucket_V2(this IObservable<Tick> source, TimeSpan period)
        {
            return source.Publish(xs => {
                var bound = xs.Select(x => x.TradedAt.Ticks / period.Ticks).DistinctUntilChanged();
                return xs.Slice(bound);
            }).SelectMany(grp => grp.GroupBy(tk => tk.Security.Symbol).OHLC());
        }
        internal static IObservable<IList<OHLC>> Bucket_V2(this IObservable<IObservable<Tick>> source, TimeSpan period)
        {
            return source.SelectMany(x => x).Bucket_V2(period);
        }
    }
}
