using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.common;
using quant.rx;

namespace ohlc.rx.test
{
    [TestClass]
    public class MvgAvg_Test
    {
        static string file = "..\\..\\..\\..\\BARS.csv";
        [TestMethod]
        public void SMA_TEST()
        {
            double? ma1 = null;
            double? ma2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr => {
                    sr.Select(x => x.raw).SMA(20).Subscribe(x => ma1 = x);
                    sr.Select(x => x.cnt).SMA(20).Subscribe(x => ma2 = x);
                    return sr;
                }).Subscribe(oh => {
                    Debug.Assert(ma1 == null || ma2 - ma1 == oh.ofst);
                    Trace.WriteLine($"{oh.raw}\t{ma1?.ToString("0.00")}\t{ma2?.ToString("0.00")}\t{ma2 - ma1}");
                });
            }
        }
        [TestMethod]
        public void WSMA_TEST()
        {
            double? ma1 = null;
            double? ma2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr => {
                    sr.Select(x => x.raw).WSMA(14).Subscribe(x => ma1 = x);
                    sr.Select(x => x.cnt).WSMA(14).Subscribe(x => ma2 = x);
                    return sr;
                }).Subscribe(oh => {
                    Debug.Assert(ma1 == null || Math.Abs((ma2.Value - ma1.Value) - oh.ofst) <= 0.0001);
                    Trace.WriteLine($"{oh.raw}\t{ma1?.ToString("0.00")}\t{ma2?.ToString("0.00")}\t{ma2 - ma1}");
                });
            }
        }
        [TestMethod]
        public void EMA_TEST()
        {
            double? ma1 = null;
            double? ma2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr => {
                    sr.Select(x => x.raw).EMA(14).Subscribe(x => ma1 = x);
                    sr.Select(x => x.cnt).EMA(14).Subscribe(x => ma2 = x);
                    return sr;
                }).Subscribe(oh => {
                    Debug.Assert(ma1 == null || Math.Abs((ma2.Value - ma1.Value) - oh.ofst) <= 0.0001);
                    Trace.WriteLine($"{oh.raw}\t{ma1?.ToString("0.00")}\t{ma2?.ToString("0.00")}\t{ma2 - ma1}");
                });
            }
        }
        [TestMethod]
        public void LWMA_TEST()
        {
            double? ma1 = null;
            double? ma2 = null;
            using (StreamReader rdr = new StreamReader(file))
            {
                rdr.OHLC().Publish(sr => {
                    sr.Select(x => x.raw).LWMA(14).Subscribe(x => ma1 = x);
                    sr.Select(x => x.cnt).LWMA(14).Subscribe(x => ma2 = x);
                    return sr;
                }).Subscribe(oh => {
                    Debug.Assert(ma1 == null || Math.Abs((ma2.Value - ma1.Value) - oh.ofst) <= 0.0001);
                    Trace.WriteLine($"{oh.raw}\t{ma1?.ToString("0.00")}\t{ma2?.ToString("0.00")}\t{ma2 - ma1}");
                });
            }
        }
    }
}
