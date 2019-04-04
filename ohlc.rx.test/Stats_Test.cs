using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.common;
using quant.rx;

namespace ohlc.rx.test
{
    [TestClass]
    public class Stats_Test
    {
        static string file = "..\\..\\..\\..\\BARS.csv";
        [TestMethod]
        public void SUM_TEST()
        {
            double? s1 = null;
            double? s2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr => {
                    sr.Select(x => x.raw).SUM(20).Subscribe(x => s1 = x);
                    sr.Select(x => x.cnt).SUM(20).Subscribe(x => s2 = x);
                    return sr;
                }).Subscribe(oh => {
                    Debug.Assert(s1 == null || s2 - s1 == oh.ofst * 20);
                    Trace.WriteLine($"{oh.raw}\t{s1?.ToString("0.00")}\t{s2?.ToString("0.00")}\t{(s2 - s1) / 20}");
                });
            }
        }
        [TestMethod]
        public void StdDev_TEST()
        {
            double? s1 = null;
            double? s2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr => {
                    sr.Select(x => x.raw).StdDev(20).Subscribe(x => s1 = x);
                    sr.Select(x => x.cnt).StdDev(20).Subscribe(x => s2 = x);
                    return sr;
                }).Subscribe(oh => {
                    Debug.Assert(s2 == s1);
                    Trace.WriteLine($"{oh.raw}\t{s1?.ToString("0.00")}\t{s2?.ToString("0.00")}\t{(s2 - s1)}");
                });
            }
        }
        [TestMethod]
        public void MAX_TEST()
        {
            double? s1 = null;
            double? s2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr => {
                    sr.Select(x => x.raw).Max(20).Subscribe(x => s1 = x);
                    sr.Select(x => x.cnt).Max(20).Subscribe(x => s2 = x);
                    return sr;
                }).Subscribe(oh => {
                    Debug.Assert(s1 == null || s2 - s1 == oh.ofst);
                    Trace.WriteLine($"{oh.raw}\t{s1?.ToString("0.00")}\t{s2?.ToString("0.00")}\t{(s2 - s1)}");
                });
            }
        }
        [TestMethod]
        public void MIN_TEST()
        {
            double? s1 = null;
            double? s2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr => {
                    sr.Select(x => x.raw).Min(20).Subscribe(x => s1 = x);
                    sr.Select(x => x.cnt).Min(20).Subscribe(x => s2 = x);
                    return sr;
                }).Subscribe(oh => {
                    Debug.Assert(s1 == null || s2 - s1 == oh.ofst);
                    Trace.WriteLine($"{oh.raw}\t{s1?.ToString("0.00")}\t{s2?.ToString("0.00")}\t{(s2 - s1)}");
                });
            }
        }
    }
}
