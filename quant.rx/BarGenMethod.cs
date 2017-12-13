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
        static TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        public static DateTime ToEST(this DateTime utc) => TimeZoneInfo.ConvertTime(utc, est);

        public static IObservable<Tick> FromCSV(string file)
        {
            return File.ReadLines(file).ToObservable().Skip(1).Select(x =>
            {
                var row = x.Replace("\"", "").Split(',');
                return new Tick(uint.Parse(row[8]), double.Parse(row[7]), DateTime.Parse(row[3]));
            });
        }
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
                        ohlc = new OHLC();
                    }
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
            return source.OHLC((ohlc, tck) => (ohlc.Open.Time.Ticks/period.Ticks != tck.Time.Ticks/period.Ticks));
        }
        public static IObservable<OHLC> MVWAP_Y(this IObservable<Tick> source, uint period, uint range)
        {
            return source.OHLC((ohlc, tck) => {
                return (ohlc.Volume >= period) && (ohlc.High.Price - ohlc.Low.Price >= range);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static IObservable<OHLC> MVWAP(this IObservable<Tick> source, uint period, uint range)
        {
            return Observable.Create<OHLC>(obs => {
                // variables
                double maxVal = double.MinValue;
                double minVal = double.MaxValue;
                var tk = new Tick();
                var ohlc = new OHLC();

                return source.Select(tck => {
                    // capture tick which generates the mvwap
                    tk = tck;
                    return tck;
                }).MVWAP(period).Subscribe((vw) => {
                    // add the tick to OHLC
                    ohlc.Add(tk);
                    // capture max and min moving vwap
                    maxVal = Math.Max(vw, maxVal);
                    minVal = Math.Min(vw, minVal);
                    // if range is exceeded.
                    if (maxVal - minVal >= range)
                    {
//                        Trace.WriteLine($"\tX\t{tk.Time}\t{maxVal-minVal}");
                        // publish the bar
                        obs.OnNext(ohlc);
                        // reset state
                        maxVal = double.MinValue;
                        minVal = double.MaxValue;
                        ohlc = new OHLC();
                    }
//                    Trace.WriteLine($"{x.ToString("0.000")}\t{tk.Time}\t{tk.Quantity}\t{tk.Price}");
                }, obs.OnError, () => {
                    //publish final bar
                    obs.OnNext(ohlc);
                    obs.OnCompleted();
                });
            });
        }
    }
}
