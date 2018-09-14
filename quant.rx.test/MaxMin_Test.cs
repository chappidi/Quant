using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class MaxMinTest
    {
        [TestMethod]
        public void Min_Test_1()
        {
            double[] items = { 27, 19, 08, 17, 18, 13, 23, 43, 24, 29, 15, 39, 38, 61, 36 };

            string v1 = null;
            string va = null;
            items.ToObservable().Publish(sr => {
                sr.Min_V1(3).Subscribe(x => v1 = x.ToString());
                sr.Min(3).Subscribe(x => va = x.ToString());
                return sr;
            }).Subscribe(x => {
                Debug.Assert(va == v1);
                Trace.WriteLine($"{x}\t{va}\t{v1}");
            });
        }
        [TestMethod]
        public void Max_Test_1()
        {
            double[] items = { 27, 19, 08, 17, 18, 13, 23, 43, 24, 29, 15, 39, 38, 61, 36 };

            string v1 = null;
            string va = null;
            items.ToObservable().Publish(sr => {
                sr.Max_V1(3).Subscribe(x => v1 = x.ToString());
                sr.Max(3).Subscribe(x => va = x.ToString());
                return sr;
            }).Subscribe(x => {
                Debug.Assert(va == v1);
                Trace.WriteLine($"{x}\t{va}\t{v1}");
            });
        }
        [TestMethod]
        public void MaxTest_2()
        {
            double[] items = {
                127.0090, 127.6159, 126.5911, 127.3472, 128.1730,
                128.4317, 127.3671, 126.4220, 126.8995, 126.8498,
                125.6460, 125.7156, 127.1582, 127.7154, 127.6855,
                128.2228, 128.2725, 128.0934, 128.2725, 127.7353,
                128.7700, 129.2873, 130.0633, 129.1182, 129.2873,
                128.4715, 128.0934, 128.6506, 129.1381, 128.6406 };
            items.ToObservable().Max(14).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void Perf_Test()
        {
            Random rnd = new Random();
            var data = new List<double>();
            for (int itr = 0; itr < 1000000; itr++)
            {
                data.Add(rnd.Next(1, 5));
                data.Add(rnd.Next(5, 10));
                data.Add(rnd.Next(10, 15));
                data.Add(rnd.Next(15, 20));
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var cnt = data.ToObservable().Max_V1(500).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds}\t{cnt}");
            sw = new Stopwatch();
            sw.Start();
            cnt = data.ToObservable().Max_V2(500).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds}\t{cnt}");
        }
    }
}
