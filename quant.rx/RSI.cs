﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using quant.common;

[assembly: InternalsVisibleTo("quant.rx.test")]

namespace quant.rx
{
    /// <summary>
    /// Relative Strength Indicator (RSI)
    /// RSI is a momentum oscillator that measures the speed and change of price movements. RSI oscillates between zero and 100. 
    /// RSI is considered overbought when above 70 and oversold when below 30. 
    /// Signals can also be generated by looking for divergences, failure swings, and centerline crossovers. 
    /// 
    /// RSI can also be used to identify the general trend
    /// RSI work best when prices move sideways within a range
    ///
    /// RSI = 100 - (100/(1 + RS)) 
    /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi
    /// http://cns.bu.edu/~gsc/CN710/fincast/Technical%20_indicators/Relative%20Strength%20Index%20(RSI).htm
    /// </summary>
    internal static class RSExt
    {
        /// <summary>
        /// Relative Strength
        /// RS = Average Gain / Average Loss
        /// AverageGain  = WSMA of Gain(Period)  ?? or SMA of Gain/Loss(Period)
        /// AverageLoss = WSMA of Loss(Period)
        /// </summary>
        /// <param name="source">relative offset from previous close. it can be +ve (gain) or -ve (loss)</param>
        /// <param name="period">averaging period</param>
        /// <returns></returns>
        internal static IObservable<double> RS(this IObservable<double> source, uint period)
        {
            return source.Publish(src => {
                // change --> loss or gain
                var gain = src.Select(ch => (ch > 0) ? ch : 0.0);
                var loss = src.Select(ch => (ch < 0) ? Math.Abs(ch) : 0.0);
                // averages over the period
                return gain.WSMA(period).Zip(loss.WSMA(period), (gn, ls) => { return (ls == 0) ? 100.0 : (gn / ls); });
            });
        }
    }
    /// <summary>
    /// 
    /// </summary>
    internal static class DeltaExt
    {
        internal static IObservable<double> Delta(this IObservable<double> source)
        {
            return Observable.Create<double>(obs => {
                double oldVal = double.NaN;
                return source.Subscribe((newVal) => {
                    if (!double.IsNaN(oldVal))
                        obs.OnNext(newVal - oldVal);
                    oldVal = newVal;
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// TODO: Work in Progress
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<double> Delta(this IObservable<OHLC> source)
        {
            return Observable.Create<double>(obs => {
                OHLC oldVal = null;
                return source.Subscribe((newVal) => {
                    if (oldVal != null) {
                        // Roll happened  (either in middle or start of new bar.
                        // what to do ?
                        if (newVal.Offset != 0 || oldVal.Close.Security != newVal.Open.Security)
                        {

                        }
                        obs.OnNext(newVal.Close.Price - oldVal.Close.Price);
                    }
                    oldVal = newVal;
                }, obs.OnError, obs.OnCompleted);
            });
        }
    }
    /// <summary>
    /// RSI = SRC.Delta().RS().RSI()
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> RSI(this IObservable<double> source, uint period) {
            return source.Delta().RS(period).Select(rs => 100.0 - (100.0 / (1 + rs)));
        }
        public static IObservable<double> RSI(this IObservable<OHLC> source, uint period) {
            return source.Delta().RS(period).Select(x => 100 - (100 / (1 + x)));
        }
    }
}