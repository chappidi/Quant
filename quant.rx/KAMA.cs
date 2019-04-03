using System;
using System.Collections.Generic;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// Kaufman Adaptive Moving Average
    /// https://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:kaufman_s_adaptive_moving_average
    /// Excel Data
    /// https://stockcharts.com/school/lib/exe/fetch.php?media=chart_school:technical_indicators_and_overlays:kaufman_s_adaptive_moving_average:cs-kama.xls
    /// </summary>
    public static partial class QuantExt
    {
        public static IObservable<double> KAMA(this IObservable<double> source, uint periodER, uint periodFast, uint periodSlow)
        {
            return null;
        }
    }
}
