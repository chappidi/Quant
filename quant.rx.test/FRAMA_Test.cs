using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.core;

namespace quant.rx.test
{
    /// <summary>
    /// sample excel data file
    /// http://www.etfhq.com/download.php?fileID=9
    /// </summary>
    [TestClass]
    public class FRAMA_Test
    {
        /// <summary>
        /// Data from etfhq.com-frama.xls
        /// </summary>
        static IList<BarData> INPUT {
            get {
                return new BarData[] {
                    new BarData("1/11/2011", 47.0, 47.3, 46.8, 47.1),
                    new BarData("1/12/2011", 47.2, 47.6, 47.0, 47.3),
                    new BarData("1/13/2011", 47.3, 47.7, 47.1, 47.6),
                    new BarData("1/14/2011", 47.5, 47.9, 47.4, 47.8),
                    new BarData("1/15/2011", 47.9, 48.1, 47.8, 48.0),

                    new BarData("1/16/2011", 48.2, 48.9, 48.1, 48.8),
                    new BarData("1/17/2011", 48.7, 49.2, 48.6, 48.8),
                    new BarData("1/18/2011", 48.6, 49.0, 48.4, 48.7),
                    new BarData("1/19/2011", 48.5, 49.2, 48.3, 48.7)
                };
            }
        }
        static IObservable<OHLC> OHLC {
            get {
                return INPUT.ToObservable().Select(bd => {
                    var sec = Security.Lookup("DMK3");
                    var oh = new OHLC(new Tick(sec, 1, (uint)(bd.Open * 10), bd.Date));
                    oh.Add(new Tick(sec, 1, (uint)(bd.High * 10), bd.Date));
                    oh.Add(new Tick(sec, 1, (uint)(bd.Low * 10), bd.Date));
                    oh.Add(new Tick(sec, 1, (uint)(bd.Close * 10), bd.Date));
                    return oh;
                });
            }
        }
        [TestMethod]
        public void Test_2()
        {
            OHLC.Publish(sc=> {
                return sc.WithLatestFrom(sc.FRAMA(6), (ohlc, frama) =>(ohlc, frama));
            }).Subscribe(x => Trace.WriteLine($"{x.ohlc}{(x.frama / 10).ToString("0.00")}"));
        }
        [TestMethod]
        public void Test_1()
        {
            INPUT.ToObservable().Publish(sc => {
                var HL1 = sc.Select(x => x.High).Max(3).Zip(sc.Select(x => x.Low).Min(3), (max, min) => (max - min) / 3);
                var HL2 = sc.Select(x => x.High).Max(3).Zip(sc.Select(x => x.Low).Min(3), (max, min) => (max - min) / 3).Skip(3);
                var HL = sc.Select(x => x.High).Max(6).Zip(sc.Select(x => x.Low).Min(6), (max, min) => (max - min) / 6);
                var plan = HL.And(HL1).And(HL2).Then((hl, hl1, hl2) => (hl, hl1, hl2));
                return sc.WithLatestFrom(Observable.When(plan), (bar, pln) => (bar,pln));
            }).Subscribe(x=> {
                double N1 = x.pln.hl1;
                double N2 = x.pln.hl2;
                double N = x.pln.hl;
                double fd = (Math.Log(N1 + N2) - Math.Log(N)) / Math.Log(2.0);
                double A = Math.Exp(-4.6 * (fd - 1));
                Trace.WriteLine($"{x.bar.Close.ToString("0.0")}\t{N1.ToString("0.00")}\t{N2.ToString("0.00")}\t{N.ToString("0.00")}\t{fd.ToString("0.00")}\t{A.ToString("0.00")}");
            });
        }
    }
}
