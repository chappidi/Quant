using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class MACDTest
    {
        /// <summary>
        /// Excelsheet "MACD Tutorial" from
        /// http://investexcel.net/how-to-calculate-macd-in-excel/
        /// </summary>
        [TestMethod]
        public void MACD_Test_1()
        {
            double[] data = { 459.99, 448.85, 446.06, 450.81, 442.8, 448.97,
                444.57, 441.4, 430.47, 420.05, 431.14, 425.66, 430.58, 431.72,
                437.87, 428.43, 428.35, 432.5, 443.66, 455.72, 454.49, 452.08, 452.73,
                461.91, 463.58, 461.14, 452.08, 442.66, 428.91, 429.79, 431.99, 427.72,
                423.2, 426.21, 426.98, 435.69, 434.33, 429.8, 419.85, 426.24, 402.8, 392.05,
                390.53, 398.67, 406.13, 405.46, 408.38, 417.2, 430.12, 442.78, 439.29, 445.52,
                449.98, 460.71, 458.66, 463.84, 456.77, 452.97, 454.74, 443.86, 428.85, 434.58,
                433.26, 442.93, 439.66, 441.35 };

            string ema_12 = null;
            string ema_26 = null;
            string macd = null;
            string sig = null;
            string hist = null;
            data.ToObservable().Publish(sr => {
                sr.EMA(12).Subscribe(x => ema_12 = x.ToString("0.0000"));
                sr.EMA(26).Subscribe(x => ema_26 = x.ToString("0.0000"));
                sr.MACD(12, 26).Subscribe(x => macd = x.ToString("0.0000"));
                sr.MACD(12, 26).EMA(9).Subscribe(x => sig = x.ToString("0.0000"));
                sr.MACD(12, 26).Histogram(9).Subscribe(x => hist = x.ToString("0.0000"));
                return sr;
            }).Subscribe(val => {
                if (!string.IsNullOrEmpty(sig))
                {
                    int k = 0;
                }
                Trace.WriteLine($"{val.ToString("0.00")}\t{ema_12}\t{ema_26}\t{macd}\t{sig}\t{hist}");
            });
        }
        /// <summary>
        /// https://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:price_oscillators_ppo
        /// </summary>
        [TestMethod]
        public void MACD_Test_2()
        {

        }

    }
}
