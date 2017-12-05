using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace quant.core.test
{
    [TestClass]
    public class MvngAvgTest
    {
        /// <summary>
        /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_averages
        /// </summary>
        [TestMethod]
        public void EMATest()
        {
            double[] items = { 22.27, 22.19, 22.08, 22.17, 22.18, 22.13, 22.23, 22.43, 22.24, 22.29, 22.15, 22.39, 22.38, 22.61, 23.36 };
            EMA eMA = new EMA(10);
            foreach (var val in items)
            {
                var retVal = eMA.Calc(val);
                Trace.WriteLine($"{val.ToString("0.00")}\t{retVal.ToString("0.00")}");
            }
        }
        /// <summary>
        /// sample . verify the "Avg Gain" column
        /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi
        /// 
        /// </summary>
        [TestMethod]
        public void WSMATest()
        {
            // Data from "Gain" column.            
            double[] items = { 0, .06, 0, .72, .5, .27, .33, .42, .24, 0, .14, 0, .67, 0, 0, .03, .38, 0, 0, .57, .04, 0, .74 };
            WSMA sMA = new WSMA(14);
            foreach (var val in items)
            {
                var retVal = sMA.Calc(val);
                Trace.WriteLine($"{val.ToString("0.00")}\t{retVal.ToString("0.00")}");
            }
        }
    }
}
