using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;

namespace quant.rx.test
{
    [TestClass]
    public class RollingWndTest
    {
        [TestMethod]
        public void RollingTest()
        {
            double[] items = { 27, 19, 08, 17, 18, 13, 23, 43, 24, 29, 15, 39, 38, 61, 36 };
            items.ToObservable().RollingWindow(5).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void MinTest()
        {
            double[] items = { 27, 19, 08, 17, 18, 13, 23, 43, 24, 29, 15, 39, 38, 61, 36 };
            items.ToObservable().Min(3).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void MaxTest()
        {
            double[] items = { 27, 19, 08, 17, 18, 13, 23, 43, 24, 29, 15, 39, 38, 61, 36 };
            items.ToObservable().Max(3).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void SMATest()
        {
            double[] items = { 27, 19, 08, 17, 18, 13, 23, 43, 24, 29, 15, 39, 38, 61, 36 };
//            double[] items = { 22.27, 22.19, 22.08, 22.17, 22.18, 22.13, 22.23, 22.43, 22.24, 22.29, 22.15, 22.39, 22.38, 22.61 };
            items.ToObservable().SMA(3).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void VarianceTest_X()
        {
            double[] items = { 20, 22, 24, 25, 23, 26, 28, 26, 29, 27, 28, 30, 27, 29, 28 };
            items.ToObservable().Buffer(3,1).Subscribe(x => {
                Trace.WriteLine(x.StdDev());
//                Trace.WriteLine(x.Variance());
            });
        }
        [TestMethod]
        public void VarianceTest()
        {
            double[] items = { 20, 22, 24, 25, 23, 26, 28, 26, 29, 27, 28, 30, 27, 29, 28 };
            items.ToObservable().StdDev(3).Subscribe(x => {
                Trace.WriteLine(x);
            });
        }
    }
}
