using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.core;
using quant.core.futures;
using quant.rx;

namespace quant.data.test
{
    [TestClass]
    public class MVWAP_Test
    {
        static string prdt = "CL";
        static DateTime dtStart = DateTime.Parse("2015-Jan-01 00:00:01");
        static DateTime dtEnd = DateTime.Parse("2016-Jan-31 16:59:59");
        [TestMethod]
        public void MVWAP_By_Vol()
        {
            // query raw data. we need some buffer to figure inital roll (contract) before the dtStart.
            // this is similar to TestRollOnlyDate
            // Stitch contract Roll with the conditions to roll. 
            var src = new Subscription(prdt, Resolution.Tick).Query(dtStart.AddDays(-2), dtEnd).ToObservable().Stitch(TimeSpan.FromMinutes(30), 1000, 1.2).Where(x => x.Side != Aggressor.NA);
            // filter any data before dtStart
            src.Where(x => x.TradedAt >= dtStart).MVWAP(10000, 250).Subscribe(oh => { Trace.WriteLine(oh); });
        }
    }
}
