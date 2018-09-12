using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    public static class RxExtMethod
    {
        public static IObservable<double> TR(this IObservable<Tuple<OHLC, OHLC>> source)
        {
            return Observable.Create<double>(obs => {
                OHLC prev = null;
                return source.Subscribe(val => {
                    var offset = val.Item1.Close.Price - val.Item2.Close.Price;
                    if (prev != null)
                        obs.OnNext(val.Item1.TR(prev));
                    prev = val.Item1;
                }, obs.OnError, obs.OnCompleted);
            });
        }


        public static IObservable<double> ATR(this IObservable<Tuple<OHLC, OHLC>> source, uint period) {
            return source.TR().WSMA(period);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<double> ATR(this IObservable<OHLC> source, uint period) {
            return source.TR().WSMA(period);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="signalPeriod"></param>
        /// <param name="multiplier"></param>
        /// <param name="atrPeriod"></param>
        /// <returns></returns>
        public static IObservable<double> KELT(this IObservable<OHLC> source, uint signalPeriod, double atrMultiplier, uint atrPeriod) {
            // signal EMA
            var signal = source.EMA(signalPeriod);
            var envelope = source.ATR(atrPeriod);
            signal.WithLatestFrom(envelope, (sig, atr) => new Tuple<double,double>( sig, atr * atrMultiplier));
            return Observable.Create<double>(obs => {
                return source.Subscribe(
                    (val) => {

                }, obs.OnError, obs.OnCompleted);
            });
        }
    }
}