using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.data
{
    public enum Resolution { Tick, Second, Minute, Hour, Daily }
    public static class DataSource
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prdt"></param>
        /// <returns></returns>
        public static IEnumerable<(string, DateTime)> Query(string prdt) => ISubscription.Query(prdt);
        /// <summary>
        /// continuous Pricing.
        /// 1. prices are adjusted for roll
        /// 2. single symbol for all ticks
        /// </summary>
        /// <param name="prdt"></param>
        /// <param name="dtFrom"></param>
        /// <param name="dtTo"></param>
        /// <returns></returns>
        public static IObservable<Tick> Query(string prdt, DateTime dtFrom, DateTime dtTo) {
            return new equity.Subscription(prdt).Query(dtFrom, dtTo);
        }
        /// <summary>
        /// Contracts Stitched. 
        /// 1. Prices are not adjusted for roll
        /// 2. original contract symbols are used
        /// </summary>
        /// <param name="prdt"></param>
        /// <param name="dtFrom"></param>
        /// <param name="dtTo"></param>
        /// <param name="stitcher"></param>
        /// <returns></returns>
        public static IObservable<Tick> Query(string prdt, DateTime dtFrom, DateTime dtTo, Func<IObservable<Tick>, IObservable<Tick>> stitcher) {
            var src = new futures.Subscription(prdt).Query(dtFrom, dtTo);
            return stitcher(src);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtFrom"></param>
        /// <param name="dtTo"></param>
        /// <returns></returns>
        public static IObservable<IGroupedObservable<Security, Tick>> QueryX(string prdt, DateTime dtFrom, DateTime dtTo)
        {
            return Query(prdt, dtFrom, dtTo, obs => obs).GroupBy(tk => tk.Security);
        }
    }
}