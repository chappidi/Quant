using System;

namespace quant.core
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Exponential Moving Average
    // Initial SMA: 10-period sum / 10 
    // Multiplier: (2 / (Time periods + 1) ) = (2 / (10 + 1) ) = 0.1818 (18.18%)
    // EMA: {Close - EMA(previous day)} x multiplier + EMA(previous day). 
    // EMA = Close * multiplier + EMA(previous day) x { 1 - multiplier}
    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class EMA
    {
        readonly double m_factor = 0;
        // Initial average of first m_size elements
        double m_ema = 0;
        double m_count = 0;
        /// <summary>
        /// 
        /// </summary>
        public uint Period { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="period"></param>
        public EMA(uint period) {
            if (period == 0)
                throw new ArgumentException(message: "cannot be zero", paramName: "period");

            Period = period;
            m_factor = (2.0 / (Period + 1));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double Calc(double input) {
            //edge condition
            if (double.IsNaN(input))    return double.NaN;
            if (Period == 1)            return input;

            // buffer not full
            if (m_count < Period) {
                m_ema += input; 
                ++m_count;
                // insufficient samples
                if (m_count != Period)
                    return double.NaN;
                // count matches window Period
                m_ema = m_ema / Period;
            }
            else
                m_ema = (input * m_factor) + (m_ema * (1.0 - m_factor));
            return m_ema;
        }
    }
}
