using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.rx
{
    /// <summary>
    /// https://www.investopedia.com/ask/answers/031115/what-common-strategy-traders-implement-when-using-volume-weighted-average-price-vwap.asp
    /// https://tradingsim.com/blog/vwap-indicator/
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> MVWAP(this IObservable<QTY_PX> source, uint period, IObservable<double> offset = null) {
            return source.MVWAP_V4(period, offset);
//            return new MVWAP_V2(source, period, offset);
        }

        public static IObservable<double> MVWAP(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => new QTY_PX(x.Quantity, x.Price)).MVWAP(period, sr.Offset());
            });
        }
        public static IObservable<double> MVWAP(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => new QTY_PX(x.Volume, x.Close.Price)).MVWAP(period, sr.Offset());
            });
        }
        public static IObservable<OHLC> MVWAP(this IObservable<Tick> source, uint period, uint range) {
            return source.Publish(sr => {
                var bound = sr.MVWAP(period).Range(range, sr.Offset());
                //should I use Slice or Window ??
//                return src.Window(bound).SelectMany(x => x.OHLC());
                return sr.Slice(bound).SelectMany(x => x.OHLC());
            });
        }
    }
}
