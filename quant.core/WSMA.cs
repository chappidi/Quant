using System;
using System.Collections.Generic;
using System.Text;

namespace quant.core
{
   //////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Wilder’s Smoothing AKA SMoothed Moving Average (WSMA)
    // SUM1=SUM (CLOSE, N)
    // WSMA1 = SUM1/ N
    // WSMA (i) = (SUM1 – WSMA1 + CLOSE(i) )/ N =  ( (WSMA1 * (N-1)) + CLOSE(i) )/N
    //
    // The WSMA is almost identical to an EMA of twice the look back period. 
    // In other words, 20-period WSMA is almost identical to a 40-period EMA
    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class WSMA
    {
        double m_wsma = 0;
        uint m_count = 0;
        /// <summary>
        /// 
        /// </summary>
        public uint Period { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="period"></param>
        public WSMA(uint period) {
            if (period == 0)
                throw new ArgumentException(message: "cannot be zero", paramName: "period");
            Period = period;
        }
        /// <summary>
        /// Insert a new value and return the SMMA value.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public double Calc(double input) {
            //edge condition
            if (Period == 1) return input;

            // buffer not full
            if (m_count < Period) {
                ++m_count;
                m_wsma += input;
                return (m_count == Period) ? (m_wsma / Period) : double.NaN;
            }
            double wsma_one = m_wsma / Period;
            m_wsma = (m_wsma - wsma_one + input);
            return m_wsma / Period;
        }
    }}
