using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Subjects;
using System.Diagnostics;

namespace quant.rx.test
{
    [TestClass]
    public class MvngAvgTest
    {
        [TestMethod]
        public void EMATest()
        {
            double[] items = { 22.27, 22.19, 22.08, 22.17, 22.18, 22.13, 22.23, 22.43, 22.24, 22.29, 22.15, 22.39, 22.38, 22.61, 23.36 };
            var ticks = new Subject<double>();
            ticks.EMA(10).Subscribe(x => { Trace.WriteLine(x.ToString("0.00")); });
            foreach (var val in items) {
                ticks.OnNext(val);
            }
        }
    }
}
