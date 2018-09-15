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
    /// </summary>
    public static partial class QuantExt
    {
        static IObservable<double> WilliamsR(this IObservable<OHLC> source, uint period)
        {
            source.Publish(sr => {
                return sr.Offset().Publish(of => {
                    sr.Select(x => (double)x.High.Price).Max(period,of);
                    sr.Select(x => (double)x.Low.Price).Min(period, of);
                    sr.Select(x => x.Close.Price);
                    return sr;
                });
            });
            return null;
        }
    }
}
