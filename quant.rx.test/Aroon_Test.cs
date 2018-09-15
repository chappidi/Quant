using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class AroonTest
    {
        [TestMethod]
        public void Aroon_Up_Test()
        {
            var data = BarData.DATA.Select(x => x.High).ToList();

            string arV1 = null;
            string arV2 = null;
            data.ToObservable().Publish(sr => {
                sr.AroonUp_V1(24).Subscribe(x => arV1 = x.ToString("0.00"));
                sr.AroonUp_V2(24).Subscribe(x => arV2 = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
                Debug.Assert(arV1 == arV2);
                Trace.WriteLine($"{val.ToString("0")}\t{arV1}\t{arV2}");
            });
        }
        [TestMethod]
        public void Aroon_Down_Test()
        {
            var data = BarData.DATA.Select(x => x.Low).ToList();

            string arV1 = null;
            string arV2 = null;
            data.ToObservable().Publish(sr => {
                sr.AroonDown_V1(24).Subscribe(x => arV1 = x.ToString("0.00"));
                sr.AroonDown_V2(24).Subscribe(x => arV2 = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
                Debug.Assert(arV1 == arV2);
                Trace.WriteLine($"{val.ToString("0")}\t{arV1}\t{arV2}");
            });
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
            var cnt = data.ToObservable().AroonUp_V1(240).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds}\t{cnt}");
            sw = new Stopwatch();
            sw.Start();
            cnt = data.ToObservable().AroonUp_V2(240).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds}\t{cnt}");
        }

    }
}
