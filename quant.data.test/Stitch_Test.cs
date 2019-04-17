using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.core;
using quant.core.futures;

namespace quant.data.test
{
    [TestClass]
    public class Stitch_Test
    {
        static string prdt = "ZS";
        static DateTime dtStart = DateTime.Parse("2016-Jan-01 00:00:01");
        static DateTime dtEnd = DateTime.Parse("2016-Mar-15 16:59:59");
        [TestMethod]
        public void TestRAW()
        {
            // raw data as you get live on solace. across all contracts for given product
            var rawSrc = DataSource.Query(prdt, dtStart, dtEnd, obs => obs).Where(x => x.Side != Aggressor.NA);
            var xy = rawSrc.ToList().Wait();
            Trace.WriteLine($"RAW::{xy[0]}\t{xy.Last()}\t{xy.Count}");
        }
        /// <summary>
        /// This works best for both backtesting and live.
        /// It autodetects when to roll based on Volume of contracts.
        /// No need for boundaries table.
        /// Looks at 30 mins bar to figure out the volume and compares to see if ready to roll.
        /// the bar should have atleast min volume. two consecutive confirms and volume of next contract has to be X% higher than current.
        /// </summary>
        [TestMethod]
        public void TestRoll_ByVol()
        {
            // query raw data. we need some buffer to figure inital roll (contract) before the dtStart.
            // this is similar to TestRollOnlyDate
            // Stitch contract Roll with the conditions to roll. 
            var src = DataSource.Query(prdt, dtStart.AddDays(-2), dtEnd, obs => obs.Stitch(TimeSpan.FromMinutes(30), 1000, 1.2)).Where(x => x.Side != Aggressor.NA);
            // filter any data before dtStart
            var xy = src.Where(x => x.TradedAt >= dtStart).ToList().Wait();
            Trace.WriteLine($"XXX::{xy[0]}\t{xy.Last()}\t{xy.Count}");
        }
        [TestMethod]
        public void Roll_ByVol_CL()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var src = DataSource.Query("CL", dtFrom, dtFrom.AddDays(5), obs => obs.Stitch(TimeSpan.FromMinutes(30)))
                .ByInterval(TimeSpan.FromMinutes(5));
            // filter any data before dtStart
            src.Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void Roll_ByVol_CL2()
        {
            DateTime dtFrom = DateTime.Parse("2017-May-15 00:00:01");
            var src = DataSource.Query("CL", dtFrom, dtFrom.AddDays(5), obs => obs.Stitch(TimeSpan.FromMinutes(30), 10000, 1.2))
                .ByInterval(TimeSpan.FromMinutes(5));
            // filter any data before dtStart
            src.Subscribe(x => Trace.WriteLine(x));
        }
    }
}
