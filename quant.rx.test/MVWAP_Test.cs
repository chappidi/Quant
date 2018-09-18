using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.common;
using quant.rx;

namespace quant.rx.test
{
    [TestClass]
    public class MVWAPTest
    {
        [TestMethod]
        public void MVWAP_Test_1()
        {
            double[] items = { 22.2734, 22.1940, 22.0847, 22.1741, 22.1840, 22.1344,
                22.2337, 22.4323, 22.2436, 22.2933, 22.1542, 22.3926, 22.3816, 22.6109,
                23.3558, 24.0519, 23.7530, 23.8324, 23.9516, 23.6338, 23.8225, 23.8722,
                23.6537, 23.1870, 23.0976, 23.3260, 22.6805, 23.0976, 22.4025, 22.1725
            };

            string sma1 = null;
            string sma2 = null;
            string sma3 = null;
            items.ToObservable().Publish(sr => {
                sr.SMA_V1(10).Subscribe(x => sma1 = x.ToString("0.00"));
                sr.Select(px => new QTY_PX(1,px)).MVWAP_V4(10).Subscribe(x => sma2 = x.ToString("0.00"));
                sr.SMA_V4(10).Subscribe(x => sma3 = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
                Debug.Assert(sma1 == sma2 && sma1 == sma3);
                Trace.WriteLine($"\t{val.ToString("0.00")}\t{sma1}\t{sma2}\t{sma3}");
            });
        }
        [TestMethod]
        public void MVWAP_Test_2()
        {
            QTY_PX[] items = {
                new QTY_PX(02, 1.2),
                new QTY_PX(04, 1.3),
                new QTY_PX(06, 1.4),
                new QTY_PX(08, 1.5),
                new QTY_PX(10, 1.6),
                new QTY_PX(12, 1.7),
                new QTY_PX(16, 1.8)
            };

            string vw2 = null;
            string vw3 = null;
            string vw4 = null;
            items.ToObservable().Publish(sr => {
                sr.MVWAP_V2(6).Subscribe(x => vw2 = x.ToString("0.00"));
                sr.MVWAP_V3(6).Subscribe(x => vw3 = x.ToString("0.00"));
                sr.MVWAP_V4(6).Subscribe(x => vw4 = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
//                Debug.Assert(vw4 == vw2 && vw4 == vw3);
                Trace.WriteLine($"\t{val.PX.ToString("0.00")}\t{vw2}\t{vw3}\t{vw4}");
            });
        }
        [TestMethod]
        public void Perf_Test()
        {
            Random rnd = new Random();
            var data = new List<QTY_PX>();
            for (int itr = 0; itr < 1000000; itr++)
            {
                data.Add(new QTY_PX((uint)rnd.Next(1, 5), rnd.Next(5, 20)));
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var cnt = data.ToObservable().MVWAP_V2(1000).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"V2: {sw.ElapsedMilliseconds}\t{cnt}");
            sw = new Stopwatch();
            sw.Start();
            cnt = data.ToObservable().MVWAP_V3(1000).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"V3: {sw.ElapsedMilliseconds}\t{cnt}");
            sw = new Stopwatch();
            sw.Start();
            cnt = data.ToObservable().MVWAP_V4(1000).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"V4: {sw.ElapsedMilliseconds}\t{cnt}");
        }

    }
}
