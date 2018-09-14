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
