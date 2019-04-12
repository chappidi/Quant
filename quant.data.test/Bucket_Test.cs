using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.core;
using quant.futures;

namespace quant.data.test
{
    [TestClass]
    public class Bucket_Test
    {
        static string prdt = "ES";
        static DateTime dtStart = DateTime.Parse("03-13-2018");
        [TestMethod]
        public void BUCKET_TEST_1()
        {
            var timeSpan = TimeSpan.FromHours(1);
            var source = new Subscription(prdt, Resolution.Tick).Query(dtStart, dtStart.AddDays(1)).ToObservable();
            var abc = source.Publish(src => {
                src.Bucket_V1(timeSpan).Subscribe(lt => {
                    foreach (var itm in lt) {
                        Trace.WriteLine($"B:{itm}");
                    }
                    Trace.WriteLine("\n");
                });
                src.Bucket_V2(timeSpan).Subscribe(lt => {
                    foreach (var itm in lt) {
                        Trace.WriteLine($"B:{itm}");
                    }
                    Trace.WriteLine("\n");
                });
                return src;
            });
            abc.Subscribe(tk => { });
        }
        [TestMethod]
        public void Perf_Test()
        {
            var timeSpan = TimeSpan.FromHours(1);
            var data = new Subscription(prdt, Resolution.Tick).Query(dtStart, dtStart.AddDays(10)).ToObservable().ToList().Wait();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var cnt = data.ToObservable().Bucket_V1(timeSpan).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds}\t{cnt}");
            sw = new Stopwatch();
            sw.Start();
            cnt = data.ToObservable().Bucket_V2(timeSpan).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds}\t{cnt}");
        }
    }
}
