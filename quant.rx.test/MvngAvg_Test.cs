using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class MvgAvgTest
    {
        /// <summary>
        /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_averages
        /// </summary>
        [TestMethod]
        public void MvgAvg_Test()
        {
            double[] items = { 22.2734, 22.1940, 22.0847, 22.1741, 22.1840, 22.1344,
                22.2337, 22.4323, 22.2436, 22.2933, 22.1542, 22.3926, 22.3816, 22.6109,
                23.3558, 24.0519, 23.7530, 23.8324, 23.9516, 23.6338, 23.8225, 23.8722,
                23.6537, 23.1870, 23.0976, 23.3260, 22.6805, 23.0976, 22.4025, 22.1725
            };
            string sma = null;
            string ema = null;
            items.ToObservable().Publish(sr => {
                sr.SMA(10).Subscribe(x => sma = x.ToString("0.00"));
                sr.EMA(10).Subscribe(x => ema = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
                Trace.WriteLine($"{val.ToString("0.00")}\t{sma}\t{ema}");
            });
        }

        /// <summary>
        /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi
        /// sample . verify the "Avg Gain" column
        /// </summary>
        [TestMethod]
        public void WSMA_Test()
        {
            // Input = Data from "Gain" column.
            // Output = Match the "Avg Gain" column
            double[] items = {
                0.0000, 0.0595, 0.0000, 0.7154, 0.4986,
                0.2691, 0.3290, 0.4188, 0.2393, 0.0000,
                0.1397, 0.0000, 0.6680, 0.0000, 0.0000,
                0.0300, 0.3788, 0.0000, 0.0000, 0.5683,
                0.0399, 0.0000, 0.7378, 0.0000, 0.0000,
                0.0000, 0.1495, 0.0398, 0.3491, 0.0000,
                0.0000, 0.4686 };

            string wsma = null;
            items.ToObservable().Publish(sr => {
                sr.WSMA(14).Subscribe(x => wsma = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
                Trace.WriteLine($"{val.ToString("0.00")}\t{wsma}");
            });
        }
    }
}
