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
    public class TechInd_Test
    {
        static string file = "..\\..\\..\\..\\BARS.csv";
        [TestMethod]
        public void RSI_TEST()
        {
            double? s1 = null;
            double? s2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr =>
                {
                    sr.Select(x => x.raw).RSI(14).Subscribe(x => s1 = x);
                    sr.Select(x => x.cnt).RSI(14).Subscribe(x => s2 = x);
                    return sr;
                }).Subscribe(oh =>
                {
                    Debug.Assert(s2 == s1);
                    Trace.WriteLine($"{oh.raw}\t{s1?.ToString("0.00")}\t{s2?.ToString("0.00")}\t{s2 - s1}");
                });
            }
        }
        [TestMethod]
        public void STOCH_TEST()
        {
            double? s1 = null;
            double? s2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr => {
                    sr.Select(x => x.raw).STOCH(14).Subscribe(x => s1 = x);
                    sr.Select(x => x.cnt).STOCH(14).Subscribe(x => s2 = x);
                    return sr;
                }).Subscribe(oh => {
                    Debug.Assert(s2 == s1);
                    Trace.WriteLine($"{oh.raw}\t{s1?.ToString("0.00")}\t{s2?.ToString("0.00")}\t{s2 - s1}");
                });
            }
        }
    }
}
