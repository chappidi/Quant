using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.core.futures
{
    /// <summary>
    /// extension functions to provide continuous pricing data for futures.
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// rolls to the next given symbol on a given date
        /// </summary>
        /// <param name="source"></param>
        /// <param name="roll"></param>
        /// <returns></returns>
        public static IObservable<Tick> Stitch(this IObservable<Tick> source, IEnumerator<(string, DateTime)> roll) {
            return source.Publish(obs => {
                var fltr = obs.Select(x => x.TradedAt).Roll(roll);
                return obs.Where(fltr);
            });
        }
        /// <summary>
        /// identifies the next symbol by max volume of 1 minute bar on the given date
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dtStart"></param>        
        /// <param name="roll"></param>
        /// <returns></returns>
        public static IObservable<Tick> Stitch(this IObservable<Tick> source, DateTime dtStart, IEnumerator<DateTime> roll) {
            return source.Publish(obs => {
                var fltr = obs.Roll(dtStart, roll);
                return obs.Where(fltr);
            });
        }
        /// <summary>
        /// provides continuous pricing data.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="timeSpan"></param>
        /// <param name="tgtVol"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static IObservable<Tick> Stitch(this IObservable<Tick> source, TimeSpan timeSpan, uint tgtVol, double factor) {
            return source.Publish(obs => {
                var fltr = obs.Bucket_V1(timeSpan).Roll(tgtVol, factor).Select(x => x.Item1);
                return obs.Where(fltr);
            });
        }
        public static IObservable<Tick> Stitch(this IObservable<IObservable<Tick>> source, TimeSpan timeSpan, uint tgtVol, double factor) {
            return source.SelectMany(x => x).Stitch(timeSpan, tgtVol, factor);
        }
        /// <summary>
        /// Roll between 9:30 AM to 1:45 PM. 
        /// factor = 1.1 (default)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static IObservable<Tick> Stitch(this IObservable<Tick> source, TimeSpan timeSpan, double factor = 1.1) {
            return source.Publish(obs => {
                var fltr = obs.Bucket_V1(timeSpan).Roll(new TimeSpan(09, 0, 0), new TimeSpan(13, 45, 0), factor).Select(x => x.Item1);
                return obs.Where(fltr);
            });
        }
        public static IObservable<Tick> Stitch(this IObservable<IObservable<Tick>> source, TimeSpan timeSpan, double factor = 1.1) {
            return source.SelectMany(x => x).Stitch(timeSpan,factor);
        }
    }
}
