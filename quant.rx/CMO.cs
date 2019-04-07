using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    /// <summary>
    /// Type: Momentum Indicator
    /// Chande Momentum Oscillator (CMO)
    /// 
    /// CMO = ((TGain - TLoss) / (TGain + TLoss)) * 100
    /// ch = newClose - oldClose
    /// if ch gt 0  gain=ch and loss=0
    /// if ch lt 0 gain=0 and loss=Math.Abs(ch)
    /// TotalGain = TGain = gain.MovingTotal(10)
    /// TotalLoss = TLoss = loss.MovingTotal(10)
    /// </summary>
    internal static class CMOExt
    {
        internal static IObservable<double> ChandeM(this IObservable<double> source, uint period)
        {
            return source.Publish(src => {
                // change --> loss or gain
                var gain = src.Select(ch => (ch >= 0) ? ch : 0.0);
                var loss = src.Select(ch => (ch <= 0) ? -1 * ch : 0.0);
                // averages over the period
                return gain.SMA(period).Zip(loss.SMA(period), (gn, ls) => 100.0 * ((gn - ls) / (gn + ls)));
            });
        }
    }

    public static partial class QuantExt
    {
        public static IObservable<double> CMO(this IObservable<double> source, uint period)
        {
            return source.Delta().ChandeM(period);
        }
        public static IObservable<double> CMO(this IObservable<OHLC> source, uint period)
        {
            return source.Delta().Select(x => (double)x).ChandeM(period);
        }
    }
}
