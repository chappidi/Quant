using System;
using System.Collections.Generic;
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
        internal static IObservable<double> DM(this IObservable<OHLC> source)
        {
            return null;
        }
        /// <summary>
        /// Plus Directional Indicator (+DI)
        /// </summary>
        public static IObservable<double> PlusDI(this IObservable<double> source, uint period)
        {
            return null;
        }
        /// <summary>
        /// Minus Directional Indicator (-DI)
        /// </summary>
        public static IObservable<double> MinusDI(this IObservable<double> source, uint period)
        {
            return null;
        }
        /// <summary>
        /// Average Directional Index
        /// </summary>
        public static IObservable<double> ADX(this IObservable<double> source, uint period)
        {
            return null;
        }

    }
}
