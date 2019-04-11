using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace quant.core
{
    public static partial class CoreExt
    {
        /// <summary>
        /// Find the offset during the roll
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<double> Offset(this IObservable<Tick> source)
        {
            Tick prevVal = null;
            return Observable.Create<double>(obs => {
                return source.Subscribe(newVal => {
                    if (prevVal != null && prevVal.Security != newVal.Security) {
                        obs.OnNext(newVal.Price - prevVal.Price);
                    }
                    prevVal = newVal;
                });
            });
        }
        /// <summary>
        /// Find the offset with the OHLC.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<double> Offset(this IObservable<OHLC> source)
        {
            Tick prevVal = null;
            return Observable.Create<double>(obs => {
                return source.Subscribe(ohlc => {
                    // Security has rolled in ohlc
                    if (prevVal != null && prevVal.Security != ohlc.Close.Security) {
                        obs.OnNext(ohlc.get_Offset(prevVal));
                    }
                    prevVal = ohlc.Close;
                });
            });
        }
    }
}
