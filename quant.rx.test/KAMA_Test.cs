using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class KAMA_Test
    {
        static double[] DATA
        {
            get
            {
                double[] data = {
                110.46, 109.80, 110.17, 109.82, 110.15,
                109.31, 109.05, 107.94, 107.76, 109.24,
                109.40, 108.50, 107.96, 108.55, 108.85,
                110.44, 109.89, 110.70, 110.79, 110.22,
                110.00, 109.27, 106.69, 107.07, 107.92,
                107.95, 107.70, 107.97, 106.09, 106.03,
                107.65, 109.54, 110.26, 110.38, 111.94,
                113.59, 113.98, 113.91, 112.62, 112.20,
                111.10, 110.18, 111.13, 111.55, 112.08,
                111.95, 111.60, 111.39, 112.25
                };
                return data;
            }
        }
        [TestMethod]
        public void Test_1()
        {
            uint period = 10;
            double? pd = null;
            double? pv = null;
            double? er = null;
            double? sm = null;
            double ssc = 2.0 / (30 + 1);
            double fsc = 2.0 / (2.0 + 1);
            double? sc = null;

            DATA.ToObservable().Publish(sr => {
                sr.Buffer((int)period + 1, 1).Where(x => x.Count == period + 1).Select(x => Math.Abs(x[(int)period] - x[0])).Subscribe(x => pd = x);
                sr.Buffer(2, 1).Where(x => x.Count == 2).Select(x => Math.Abs(x[1] - x[0])).Subscribe(x => pv = x);
                sr.Buffer(2, 1).Where(x => x.Count == 2).Select(x => Math.Abs(x[1] - x[0])).SUM(period).Subscribe(x => sm = x);
                var pdObs = sr.Buffer((int)period + 1, 1).Where(x => x.Count == period + 1).Select(x => Math.Abs(x[(int)period] - x[0]));
                var pvObs = sr.Buffer(2, 1).Where(x => x.Count == 2).Select(x => Math.Abs(x[1] - x[0])).SUM(period);
                pdObs.Zip(pvObs, (x, y) => x / y).Subscribe(x => {
                    er = x;
                    sc = Math.Pow(er.Value * (fsc - ssc) + ssc, 2);
                });
                return sr;
            }).Subscribe(val => {
                Trace.WriteLine($"{val.ToString("0.00")}\t{pd?.ToString("0.00")}\t{pv?.ToString("0.00")}\t{er?.ToString("0.0000")}\t{sc?.ToString("0.0000")}");
            });
        }
        [TestMethod]
        public void Test_2()
        {
            uint period = 10;
            double? sc = null;

            DATA.ToObservable().Publish(sr => {
                sr.SC(period, 2, 30).Subscribe(x => sc = x);
                return sr;
            }).Subscribe(val => {
                Trace.WriteLine($"{val.ToString("0.00")}\t{sc?.ToString("0.0000")}");
            });
        }
        [TestMethod]
        public void Test_3()
        {
            double? ma = null;
            DATA.ToObservable().Publish(sr => {
                sr.KAMA(10, 2, 30).Subscribe(x => ma = x);
                return sr;
            }).Subscribe(cls => {
                Trace.WriteLine($"{cls.ToString("0.00")}\t{ma?.ToString("0.0000")}");
            });
        }
    }
}
