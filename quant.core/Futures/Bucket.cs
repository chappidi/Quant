using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using quant.core;

[assembly: InternalsVisibleTo("quant.data.test")]

namespace quant.core.futures
{
    public static class BucketExt
    {
        /// <summary>
        /// Utility function : can be moved where Bucket() code exists
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        static IObservable<IList<OHLC>> OHLC(this IObservable<IObservable<Tick>> source)  {
            return source.SelectMany(x => x.OHLC()).ToList();
        }
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
