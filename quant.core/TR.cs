using System;
using System.Collections.Generic;
using System.Text;
using quant.common;

namespace quant.core
{
    /// <summary>
    /// True Range
    /// </summary>
    class TR : IIndicator<OHLC>
    {
        OHLC _prev;
        /// <summary>
        /// 1. Current High less the current Low
        /// 2. Current High less the previous Close (absolute value)
        /// 3. Current Low less the previous Close (absolute value)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double Calc(OHLC input) {
            // first value. store the input
            if (_prev == null) {
                _prev = input;
                return double.NaN;
            }
            double tr = input.TR(_prev);
            _prev = input;
            return tr;
        }
    }
}
