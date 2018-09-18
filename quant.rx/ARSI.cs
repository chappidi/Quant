using System;
using System.Collections.Generic;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// Adaptive Relative Strength Index
    /// http://www.fxcorporate.com/help/MS/NOTFIFO/i_ARSI.html
    /// ARSIi = ARSIi-1 + K x (Pricei - ARSIi-1)
    /// 
    /// where:
    /// ARSIi - is the Adaptive Relative Strength Index value of the period being calculated.
    /// ARSIi-1 - is the Adaptive Relative Strength Index value of the period immediately preceding the one being calculated.
    /// Pricei - is the Data Source price of the period being calculated.
    /// K - is the smoothing constant calculated using the following formula:
    /// 
    /// K = |(RSIi / 100 - 0.5)| x 2
    /// </summary>
    class ARSI
    {
    }
}
