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
    public class ATR_Test
    {
        static string prdt = "ES";
        static DateTime dtStart = DateTime.Parse("03-13-2018");
        [TestMethod]
        public void ATR_TEST_2()
        {
            var roll = DataSource.Query(prdt).GetEnumerator();
            var timeSpan = TimeSpan.FromMinutes(10);
            OHLC ohlc = null;

            var abc = DataSource.Query(prdt, dtStart, dtStart.AddDays(1), obs => obs.Stitch(roll)).ByInterval(timeSpan).Do(x => ohlc = x).TR().WSMA(14);
            var xyz = DataSource.Query(prdt, dtStart, dtStart.AddDays(1)).ByInterval(timeSpan).Do(x => ohlc = x).TR().WSMA(14);
            var res1 = abc.Select(x => new { OHLC = ohlc, ATR = x }).ToList().Wait();
            var res2 = xyz.Select(x => new { OHLC = ohlc, ATR = x }).ToList().Wait();
            for (var itr = 0; itr < res1.Count; itr++)
            {
                Trace.WriteLine($"{res1[itr].OHLC.Close.Security.Symbol}\t{res1[itr].ATR.ToString("0.00")}\t{res2[itr].OHLC.Close.Security.Symbol}\t{res2[itr].ATR.ToString("0.00")}\t{(res2[itr].ATR - res1[itr].ATR).ToString("0.00")}");
            }
        }
    }
}
