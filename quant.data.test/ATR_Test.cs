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
        static TimeSpan timeSpan = TimeSpan.FromMinutes(10);
        static IEnumerator<(string, DateTime)> roll = DataSource.Query(prdt).GetEnumerator();
        OHLC ohlc = null;
        IObservable<OHLC> raw = DataSource.Query(prdt, dtStart, dtStart.AddDays(1), obs => obs.Stitch(roll)).ByInterval(timeSpan);
        IObservable<OHLC> cnt = DataSource.Query(prdt, dtStart, dtStart.AddDays(1)).ByInterval(timeSpan);

        [TestMethod]
        public void SMA_TEST()
        {
            var res1 = raw.Do(x => ohlc = x).SMA(14).Select(x => new { OHLC = ohlc, SMA = x }).ToList().Wait();
            var res2 = cnt.Do(x => ohlc = x).SMA(14).Select(x => new { OHLC = ohlc, SMA = x }).ToList().Wait();
            for (var itr = 0; itr < res1.Count; itr++)
            {
                Trace.WriteLine($"{res1[itr].OHLC.Close.Security}\t{res1[itr].SMA.ToString("0.00")}\t{res2[itr].OHLC.Close.Security}\t{res2[itr].SMA.ToString("0.00")}\t{(res2[itr].SMA - res1[itr].SMA).ToString("0.00")}");
            }
        }
        /// <summary>
        /// Validate EMA using Continuous pricing vs Discrete Roll 
        /// </summary>
        [TestMethod]
        public void EMA_TEST_2()
        {
            var res1 = raw.Do(x => ohlc = x).EMA(14).Select(x => new { OHLC = ohlc, EMA = x }).ToList().Wait();
            var res2 = cnt.Do(x => ohlc = x).EMA(14).Select(x => new { OHLC = ohlc, EMA = x }).ToList().Wait();
            for (var itr = 0; itr < res1.Count; itr++)
            {
                Trace.WriteLine($"{res1[itr].OHLC.Close.Security}\t{res1[itr].EMA.ToString("0.00")}\t{res2[itr].OHLC.Close.Security}\t{res2[itr].EMA.ToString("0.00")}\t{(res2[itr].EMA - res1[itr].EMA).ToString("0.00")}");
            }
        }
        /// <summary>
        /// Validate WSMA using Continuous pricing vs Discrete Roll 
        /// </summary>
        [TestMethod]
        public void WSMA_TEST_2()
        {
            var res1 = raw.Do(x => ohlc = x).WSMA(14).Select(x => new { OHLC = ohlc, WSMA = x }).ToList().Wait();
            var res2 = cnt.Do(x => ohlc = x).WSMA(14).Select(x => new { OHLC = ohlc, WSMA = x }).ToList().Wait();
            for (var itr = 0; itr < res1.Count; itr++)
            {
                Trace.WriteLine($"{res1[itr].OHLC.Close.Security}\t{res1[itr].WSMA.ToString("0.00")}\t{res2[itr].OHLC.Close.Security}\t{res2[itr].WSMA.ToString("0.00")}\t{(res2[itr].WSMA - res1[itr].WSMA).ToString("0.00")}");
            }
        }
        [TestMethod]
        public void ATR_TEST_2()
        {
            var res1 = raw.Do(x => ohlc = x).TR().WSMA(14).Select(x => new { OHLC = ohlc, ATR = x }).ToList().Wait();
            var res2 = cnt.Do(x => ohlc = x).TR().WSMA(14).Select(x => new { OHLC = ohlc, ATR = x }).ToList().Wait();
            for (var itr = 0; itr < res1.Count; itr++)
            {
                Trace.WriteLine($"{res1[itr].OHLC.Close.Security}\t{res1[itr].ATR.ToString("0.00")}\t{res2[itr].OHLC.Close.Security}\t{res2[itr].ATR.ToString("0.00")}\t{(res2[itr].ATR - res1[itr].ATR).ToString("0.00")}");
            }
        }
    }
}
