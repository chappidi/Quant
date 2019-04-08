using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    /// <summary>
    /// https://www.investopedia.com/terms/w/williamsr.asp?lgl=rira-layout
    /// %R = (Highest High – Closing Price) / (Highest High – Lowest Low) x -100
    /// The Williams %R oscillates from 0 to -100
    /// 0 to -20 indicates overbought market conditions. 
    /// -80 to -100 indicates oversold market conditions
    /// 
    /// 
    /// he’d mark when William’s %R touched 0 (indicating overbought conditions)
    /// after five days, if Williams %R reached -15, he’d sell
    /// He applied the opposite strategy if a security was oversold
    /// 
    /// Data
    /// https://stockcharts.com/school/lib/exe/fetch.php?media=chart_school:technical_indicators:williams_r:cs-percentr.xls
    /// </summary>
    public static partial class QuantExt
    {
        static IObservable<double> WilliamsR(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(sr => {
                return sr.Offset().Publish(of => {
                    var hhObs = sr.Select(x => (double)x.High.Price).Max(period, of);
                    var llObs = sr.Select(x => (double)x.Low.Price).Min(period, of);
                    return hhObs.Zip(llObs, (hh, ll) => (hh, ll)).WithLatestFrom(sr, (s, oh) => (s.hh, s.ll, cc: oh.Close.Price));
                });
            }).Select(x => {
                return (x.hh - x.cc) / (x.hh - x.ll) * -100;
            });
        }
    }
}
