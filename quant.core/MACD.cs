using System;
using System.Collections.Generic;
using System.Text;

namespace quant.core
{
    /// <summary>
    /// MACD : Moving Average Convergence and Divergence
    /// http://cns.bu.edu/~gsc/CN710/fincast/Technical%20_indicators/Moving%20Average%20Convergence-Divergence%20(MACD).htm
    /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_average_convergence_divergence_macd
    /// MACD Line: (12-day EMA - 26-day EMA)
    /// </summary>
    public class MACD
    {
        EMA fast;
        EMA slow;
        public MACD(uint fastPeriod, uint slowPeriod) {
            if (fastPeriod >= slowPeriod)
                throw new ArgumentException("Invalid Periods");
            fast = new EMA(fastPeriod);
            slow = new EMA(slowPeriod);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public double Calc(double val) {
            return fast.Calc(val) - slow.Calc(val);
        }
    }
    /// <summary>
    /// PPO : Percentage Price Oscillator
    /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:price_oscillators_ppo
    /// PPO Line: {(12-day EMA - 26-day EMA)/26-day EMA} x 100 = ((12-day EMA / 26-day EMA) - 1) * 100
    /// </summary>
    public class PPO
    {
        EMA fast;
        EMA slow;
        /// <summary>
        /// 
        /// </summary>
        public PPO(uint fastPeriod, uint slowPeriod) {
            fast = new EMA(fastPeriod);
            slow = new EMA(slowPeriod);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public double Calc(double val) {
            return ((fast.Calc(val) / slow.Calc(val)) - 1) * 100;
        }
    };
    /// <summary>
    /// MACD/PPO Histogram : MACD Line - Signal Line
    /// Signal Line : 9 - day EMA of MACD Line
    /// Histogram<MACD> or Histogram<PPO>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Histogram<T> where T : MACD
    {
        T line;
        EMA signal;
        public Histogram(uint signalPeriod, Func<T> func) {
            line = func();
            signal = new EMA(signalPeriod);
            //empty
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public double Calc(double val) {
            var retVal = line.Calc(val);
            return retVal - signal.Calc(retVal);
        }
    };
}
