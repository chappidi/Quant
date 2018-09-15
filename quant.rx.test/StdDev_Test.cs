using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class StdDevTest
    {
        [TestMethod]
        public void StdDev_Test_1()
        {
            double[] items = { 20, 22, 24, 25, 23, 26, 28, 26, 29, 27, 28, 30, 27, 29, 28 };

            string std1 = null;
            string std2 = null;
            string std3 = null;

            items.ToObservable().Publish(src => {
                src.StdDev_V1(3).Subscribe(x => std1 = x.ToString("0.00"));
                src.StdDev_V2(3).Subscribe(x => std2 = x.ToString("0.00"));
                src.StdDev_V3(3).Subscribe(x => std3 = x.ToString("0.00"));
                return src;
            }).Subscribe(val => {
                Debug.Assert(std1 == std2 && std1 == std3);
                Trace.WriteLine($"{val.ToString("0.00")}\t{std1}\t{std2}\t{std3}");
            }); ;
        }
        [TestMethod]
        public void StdDev_Test_2()
        {
            double[] items = { 20, 22, 24, 25, 23, 26, 28, 26, 29, 27, 28, 30, 27, 29, 28 };
            items.ToObservable().StdDev(3).Subscribe(x => {
                Trace.WriteLine(x);
            });
        }
    }
}
