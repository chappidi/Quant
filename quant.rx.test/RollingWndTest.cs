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
    }
}
