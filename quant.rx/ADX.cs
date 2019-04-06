using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    /// <summary>
    /// Directional Movement Indicator (DMI)
    ///  1. +DI
    ///  2. -DI
    ///  3. ADX
    /// https://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:average_directional_index_adx
    /// Excel Data
    /// https://stockcharts.com/school/lib/exe/fetch.php?media=chart_school:technical_indicators_and_overlays:average_directional_index_adx:cs-adx.xls
    /// </summary>
    public static partial class QuantExt
    {
        internal static IObservable<double> DX(this IObservable<(double plsDI, double mnsDI)> source) {
            return source.Select(x => (diffDI: Math.Abs(x.plsDI - x.mnsDI), sumDI: x.plsDI + x.mnsDI)).Select(x => (100 * x.diffDI) / x.sumDI);
        }
        public static IObservable<double> ADX(this IObservable<OHLC> source, uint period)
        {
            return source.DMI(period).DX().WSMA(period);
        }

    }
}
