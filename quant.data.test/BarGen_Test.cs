using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.core;
using quant.core.futures;
namespace quant.data.test
{
    [TestClass]
    public class BarGen_Test
    {
        [TestMethod]
        public void Interval()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var data = DataSource.Query("CL", dtFrom, dtFrom.AddDays(5), obs => obs.Stitch(TimeSpan.FromMinutes(30), 10000, 1.2));
            data.ByInterval(TimeSpan.FromMinutes(5)).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void Volume()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var data = DataSource.Query("CL", dtFrom, dtFrom.AddDays(5), obs => obs.Stitch(TimeSpan.FromMinutes(30), 10000, 1.2));
            data.ByVolume(10000).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void Price_1()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var data = DataSource.Query("CL", dtFrom, dtFrom.AddDays(5), obs => obs.Stitch(TimeSpan.FromMinutes(30), 10000, 1.2));
            data.ByPrice_V1(100).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void Price_2()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var data = DataSource.Query("CL", dtFrom, dtFrom.AddDays(5), obs => obs.Stitch(TimeSpan.FromMinutes(30), 10000, 1.2));
            data.ByPrice_V2(100).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void Price()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var data = DataSource.Query("CL", dtFrom, dtFrom.AddDays(5), obs => obs.Stitch(TimeSpan.FromMinutes(30), 10000, 1.2));
            data.Publish(sc => {
                sc.ByPrice_V1(10).Subscribe(x => Trace.WriteLine($"A:{x}"));
                sc.ByPrice_V2(10).Subscribe(x => Trace.WriteLine($"B:{x}"));
                return sc;
            }).Subscribe(x => { });
        }
        [TestMethod]
        public void Perf_Test()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var data = DataSource.Query("CL", dtFrom, dtFrom.AddDays(5), obs => obs.Stitch(TimeSpan.FromMinutes(30), 10000, 1.2)).ToList().Wait();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var cnt = data.ToObservable().ByPrice_V1(10).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds}\t{cnt}");
            sw = new Stopwatch();
            sw.Start();
            cnt = data.ToObservable().ByPrice_V2(10).Count().Wait();
            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds}\t{cnt}");
        }
    }
}
