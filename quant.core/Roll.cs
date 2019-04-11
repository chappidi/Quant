using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;

namespace quant.core
{
    /// <summary>
    /// Extension Methods on Roll
    /// </summary>
    public static class RollExt
    {
        /// <summary>
        /// roll schedule : when to roll and to what security is gven.
        /// monitor the input stream for timestamp and publish the roll schedule
        /// TODO: How to Handle if MoveNext returns false.
        /// </summary>
        /// <param name="source">timestamp stream</param>
        /// <param name="roll">roll schedule</param>
        /// <returns></returns>
        internal static IObservable<Security> Roll(this IObservable<DateTime> source, IEnumerator<(string symbol, DateTime rollAt)> roll) {
            return Observable.Create<Security>(obs => {
                return source.Subscribe(tsAt => {
                    if (roll.Current.ToTuple() == null)
                        roll.MoveNext();
                    // checks the tick source to observe timestamp
                    while (tsAt > roll.Current.rollAt) {
                        roll.MoveNext();
                        if (tsAt < roll.Current.rollAt) {
                            Trace.WriteLine($"ROLL:{roll.Current} @{tsAt}");
                            obs.OnNext(Security.Lookup(roll.Current.symbol));
                        }
                    }
                });
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dtStart"></param>
        /// <param name="roll"></param>
        /// <returns></returns>
        internal static IObservable<Security> Roll(this IObservable<Tick> source, DateTime dtStart, IEnumerator<DateTime> roll) {
            return new RollV2(source, dtStart, roll);
        }
        /// <summary>
        /// Roll based on min Volume and factor of increment
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tgtVol"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        internal static IObservable<Tuple<Security, double>> Roll(this IObservable<IList<OHLC>> source, uint tgtVol, double factor) {
            // picks up the security based on various factors.
            return new RollV1(source, tgtVol, factor);
        }
        /// <summary>
        /// (Default) : Time Restricted Roll between 9:30 AM to 1:45 PM
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static IObservable<Tuple<Security, double>> Roll(this IObservable<IList<OHLC>> source, TimeSpan start, TimeSpan end, double factor) {
            // picks up the security based on various factors.
            return new RollV1(source, start, end, factor);
        }
    }
}
