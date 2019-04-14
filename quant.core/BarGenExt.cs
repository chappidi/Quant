using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace quant.core
{
    public static class BarGenExt
    {
        /// <summary>
        /// boundary generation by range 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static IObservable<uint> Range(this IObservable<double> source, double range, IObservable<double> offset = null)
        {
            object lck = new object();
            double maxVal = double.MinValue;
            double minVal = double.MaxValue;
            return Observable.Create<uint>(obs => {
                var ret = new CompositeDisposable();
                // if offset is provided
                if(offset != null) {
                    ret.Add(offset.Subscribe(ofst => {
                        lock (lck) {
                            maxVal += ofst;
                            minVal += ofst;
                        }
                    }));
                }
                ret.Add(source.Subscribe(val => {
                    lock (lck) {
                        maxVal = Math.Max(val, maxVal);
                        minVal = Math.Min(val, minVal);
                        if (maxVal - minVal >= range) {
                            obs.OnNext(1);
                            maxVal = double.MinValue;
                            minVal = double.MaxValue;
                        }
                    }
                }));
                return ret;
            });
        }

        public static IObservable<OHLC> ByPrice_V1(this IObservable<Tick> source, uint range) {
            return source.OHLC((oh, tck) => {
                long high = oh.High.Price;
                long low = oh.Low.Price;
                var ofst = oh.Offset;
                // take care of offset 
                if (oh.Close.Security != tck.Security) {
                    ofst += (int)(tck.Price - oh.Close.Price);
                }
                if (oh.High.Security != tck.Security) {
                    high += ofst;
                }
                if (oh.Low.Security != tck.Security) {
                    low += ofst;
                }
                return (high - low >= range);
            });
        }
        public static IObservable<OHLC> ByPrice_V2(this IObservable<Tick> source, uint range) {
            return source.Publish(src => {
                var bound = src.Select(x => (double)x.Price).Range(range, src.Offset());
                // cannot use slice, use Window
                return src.Window(bound).SelectMany(x => x.OHLC());
            });
        }
        /// <summary>
        /// Bar generation by Volume
        /// </summary>
        /// <param name="source"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static IObservable<OHLC> ByVolume(this IObservable<Tick> source, uint volume)
        {
            return source.OHLC((ohlc, tck) => (ohlc.Volume >= volume));
        }
        /// <summary>
        /// OHLC generation by interval
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static IObservable<OHLC> ByInterval(this IObservable<Tick> source, TimeSpan period)
        {
            return source.OHLC((ohlc, tck) => (ohlc.Open.TradedAt.Ticks/period.Ticks != tck.TradedAt.Ticks/period.Ticks));
        }
    }
}
