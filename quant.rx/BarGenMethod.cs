using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;
using quant.common;
using System.Reactive.Subjects;
using System.Diagnostics;

namespace quant.rx
{
    public static class BarGenMethod
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="durationSelector"></param>
        /// <returns></returns>
        static IObservable<OHLC> OHLC(this IObservable<Tick> source, Func<OHLC, Tick, bool> durationSelector) {
            return Observable.Create<OHLC>(obs => {
                OHLC ohlc = null;
                return source.Subscribe((tck) => {
                    if (ohlc == null || durationSelector(ohlc, tck)) {
                        if(ohlc != null)
                            obs.OnNext(ohlc);
                        ohlc = new OHLC(tck);
                    }
                    else
                        ohlc.Add(tck);
                }, obs.OnError, () => {
                    //partial bar
                    if (ohlc != null)
                        obs.OnNext(ohlc);
                    obs.OnCompleted();
                });
            });
        }

        /// <summary>
        /// Bar generation By Price Range
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static IObservable<OHLC> ByPrice(this IObservable<Tick> source, uint range) {
            return source.OHLC((ohlc,tck) => (ohlc.High.Price - ohlc.Low.Price >= range));
        }
        /// <summary>
        /// Bar generation by Volume
        /// </summary>
        /// <param name="source"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static IObservable<OHLC> ByVol(this IObservable<Tick> source, uint volume) {
            return source.OHLC((ohlc, tck) => (ohlc.Volume >= volume));
        }
        /// <summary>
        /// Bar generation by Price and Volume
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static IObservable<OHLC> ByPxVol(this IObservable<Tick> source, uint range, uint volume) {
            return source.OHLC((ohlc, tck) => (ohlc.Volume >= volume) && (ohlc.High.Price - ohlc.Low.Price >= range));
        }
        /// <summary>
        /// Bar generation by interval
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static IObservable<OHLC> ByInterval(this IObservable<Tick> source, TimeSpan period) {
            return source.OHLC((ohlc, tck) => (ohlc.Open.TradedAt.Ticks/period.Ticks != tck.TradedAt.Ticks/period.Ticks));
        }
    }
}
