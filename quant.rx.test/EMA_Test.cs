using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class EMA_Test
    {
        [TestMethod]
        public void EMA_Test_1()
        {
            /// <summary>
            /// Data from
            /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_averages
            /// </summary>
            double[] items = { 22.2734, 22.1940, 22.0847, 22.1741, 22.1840, 22.1344,
                22.2337, 22.4323, 22.2436, 22.2933, 22.1542, 22.3926, 22.3816, 22.6109,
                23.3558, 24.0519, 23.7530, 23.8324, 23.9516, 23.6338, 23.8225, 23.8722,
                23.6537, 23.1870, 23.0976, 23.3260, 22.6805, 23.0976, 22.4025, 22.1725
            };

            string ema1 = null;
            string ema2 = null;
            items.ToObservable().Publish(sr => {
                sr.EMA_V1(10).Subscribe(x => ema1 = x.ToString("0.00"));
                sr.EMA_V2(10).Subscribe(x => ema2 = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
                Debug.Assert(ema1 == ema2);
                Trace.WriteLine($"{val.ToString("0.00")}\t{ema1}\t{ema2}");
            });
        }
    }
}
