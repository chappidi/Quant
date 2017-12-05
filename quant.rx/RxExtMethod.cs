using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.core;
using quant.common;

namespace quant.rx
{
    public static class RxExtMethod
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> RSI(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs =>
            {
                var ema = new RSI(period);
                return source.Subscribe(
                    (val) =>
                    {
                        var retVal = ema.Calc(val);
                        if (!double.IsNaN(retVal))
                            obs.OnNext(retVal);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="fastPeriod"></param>
        /// <param name="slowPeriod"></param>
        /// <returns></returns>
        public static IObservable<double> MACD(this IObservable<double> source, uint fastPeriod, uint slowPeriod)
        {
            return Observable.Create<double>(obs =>
            {
                var macd = new MACD(fastPeriod, slowPeriod);
                return source.Subscribe(
                    (val) =>
                    {
                        var retVal = macd.Calc(val);
                        if (!double.IsNaN(retVal))
                            obs.OnNext(retVal);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="fastPeriod"></param>
        /// <param name="slowPeriod"></param>
        /// <returns></returns>
        public static IObservable<double> PPO(this IObservable<double> source, uint fastPeriod, uint slowPeriod)
        {
            return Observable.Create<double>(obs =>
            {
                var ppo = new PPO(fastPeriod, slowPeriod);
                return source.Subscribe(
                    (val) =>
                    {
                        var retVal = ppo.Calc(val);
                        if (!double.IsNaN(retVal))
                            obs.OnNext(retVal);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// True Range
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<double> TR(this IObservable<OHLC> source)
        {
            return Observable.Create<double>(obs => {
                OHLC prev = null;
                return source.Subscribe(val => {
                        if (prev != null)
                            obs.OnNext(val.TR(prev));
                        prev = val;
                    }, obs.OnError, obs.OnCompleted);
            });
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
            var signal = source.Select(bar => bar.Close.Price).EMA(signalPeriod);
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