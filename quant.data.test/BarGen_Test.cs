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
    public class BarGen_Test
    {
        [TestMethod]
        public void Interval()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var data = new Subscription("CL", Resolution.Tick).Query(dtFrom, dtFrom.AddDays(5)).ToObservable().Stitch(TimeSpan.FromMinutes(30), 10000, 1.2);
            data.ByInterval(TimeSpan.FromMinutes(5)).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void Volume()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var data = new Subscription("CL", Resolution.Tick).Query(dtFrom, dtFrom.AddDays(5)).ToObservable().Stitch(TimeSpan.FromMinutes(30), 10000, 1.2);
            data.ByVolume(10000).Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void Price()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var data = new Subscription("CL", Resolution.Tick).Query(dtFrom, dtFrom.AddDays(5)).ToObservable().Stitch(TimeSpan.FromMinutes(30), 10000, 1.2);
            data.ByPrice(100).Subscribe(x => Trace.WriteLine(x));
        }
    }
}
