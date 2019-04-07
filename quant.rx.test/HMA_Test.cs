using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    /// <summary>
    /// data from
    /// https://tulipindicators.org/hma
    /// https://tulipindicators.org/wma
    /// </summary>
    [TestClass]
    public class HMA_Test
    {
        static double[] DATA
        {
            get {
                double[] data = {
                    81.59, 81.06, 82.87, 83.00, 83.61,
                    83.15, 82.84, 83.99, 84.55, 84.36,
                    85.53, 86.54, 86.89, 87.77, 87.29
                };
                return data;
            }
        }

        [TestMethod]
        public void Test_1()
        {
            double? ma = null;
            DATA.ToObservable().Publish(sr =>
            {
//                sr.LWMA(5).Subscribe(x => ma = x);
                sr.HMA(5).Subscribe(x => ma = x);
                return sr;
            }).Subscribe(x => Trace.WriteLine($"{x}\t{ma?.ToString("0.00")}"));
        }
    }
}
