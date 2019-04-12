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
        /// Bucket by Range.  Use This to capture the OHLC 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static IObservable<uint> Range(this IObservable<double> source, double range, IObservable<double> offset = null)
        {
            double maxVal = double.MinValue;
            double minVal = double.MaxValue;
            return Observable.Create<uint>(obs => {
                var ret = new CompositeDisposable();
                if(offset != null)
                {
                    ret.Add(offset.Subscribe(ofst => {
                        maxVal += ofst;
                        minVal += ofst;
                    }));
                }
                ret.Add(source.Subscribe(val => {
                    maxVal = Math.Max(val, maxVal);
                    minVal = Math.Min(val, minVal);
                    if (maxVal - minVal >= range) {
                        obs.OnNext(1);
                        maxVal = double.MinValue;
                        minVal = double.MaxValue;
                    }
                }));
                return ret;
            });
        }

        /// <summary>
        /// Bar generation By Price Range
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static IObservable<OHLC> ByPrice_OLD(this IObservable<Tick> source, uint range) {
            return source.OHLC((ohlc,tck) => (ohlc.High.Price - ohlc.Low.Price >= range));
        }
        public static IObservable<OHLC> ByPrice(this IObservable<Tick> source, uint range) {
            return source.OHLC((oh, tck) => {
                // take care of roll. 
                //TODO: Need to be tested.
                long high = oh.High.Price;
                long low = oh.Low.Price;
                var ofst = oh.Offset;
                // take care of offset 
                if (oh.Close.Security != tck.Security)
                    ofst += (int)(tck.Price - oh.Close.Price);
                if (oh.High.Security != tck.Security)
                    high += ofst;
                if (oh.Low.Security != tck.Security)
                    low += ofst;
                return (high - low >= range);
            });
        }
        public static IObservable<OHLC> ByPriceX(this IObservable<Tick> source, uint range) {
            return source.Publish(src => {
//                return src.Slice(src.Select(x => (double)x.Price).Range(range, src.Offset())).OHLC();
                return src.Slice(src.Select(x => (double)x.Price).Range(range, src.Offset())).SelectMany(x => x.OHLC());
            });
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
            // TO DO:  need to consider the Roll and Offset
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
