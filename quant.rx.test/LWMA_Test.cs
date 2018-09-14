using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class LWMATest
    {
        /// <summary>
        /// Data from
        /// https://www.investopedia.com/terms/l/linearlyweightedmovingaverage.asp
        /// </summary>
        [TestMethod]
        public void LWMA_Test_1()
        {
            double[] items = { 90.91, 90.83, 90.28, 90.36, 90.90 };
            string lwma1 = null;
            string lwma2 = null;
            items.ToObservable().Publish(sr => {
                sr.LWMA_V1(5).Subscribe(x => lwma1 = x.ToString("0.00"));
                sr.LWMA_V2(5).Subscribe(x => lwma2 = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
                Debug.Assert(lwma1 == lwma2);
                Trace.WriteLine($"{val.ToString("0.00")}\t{lwma1}\t{lwma2}");
            });
        }
    }
}
