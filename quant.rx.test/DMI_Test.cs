using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.rx;

namespace quant.rx.test
{
    [TestClass]
    public class DMI_Test
    {
        [TestMethod]
        public void DMI_TEST_1()
        {
            uint period = 14;
            BarData.OHLC.Publish(sc =>
            {
                return sc.DM().Publish(dm =>
                {
                    var trObs = dm.Select(x => (double)x.tr).WSMA(period).Select(x => x * period);
                    var plsObs = dm.Select(x => (x.hr > x.lr) ? Math.Max((double)x.hr, 0) : 0).WSMA(period).Select(x => x * period);
                    var mnsObs = sc.DM().Select(x => (x.lr > x.hr) ? Math.Max((double)x.lr, 0) : 0).WSMA(period).Select(x => x * period);
                    return Observable.When(trObs.And(plsObs).And(mnsObs).Then((tr, plsDM, mnsDM) => (plsDI: (100 * plsDM) / tr, mnsDI: (100 * mnsDM) / tr)));
                });
            }).Select(x => (Math.Abs(x.plsDI - x.mnsDI), x.plsDI + x.mnsDI)).Select(x => x.Item1 / x.Item2).WSMA(period)
           .Subscribe(x => Trace.WriteLine($"{x}"));
//            .Subscribe(x => Trace.WriteLine($"{x.Item1.ToString("0.00")}\t{x.Item2.ToString("0.00")}"));
        }
        [TestMethod]
        public void DMI_TEST_2()
        {
            BarData.OHLC.Publish(sc => {
                return sc.DMI(14);
            }).Subscribe(x => Trace.WriteLine($"{x}"));
        }
        /// <summary>
        /// Excel Data
        /// https://stockcharts.com/school/lib/exe/fetch.php?media=chart_school:technical_indicators_and_overlays:average_directional_index_adx:cs-adx.xls
        /// </summary>
        [TestMethod]
        public void ADX_TEST_1()
        {
            BarData.OHLC.Publish(sc => {
                return sc.ADX(14);
            }).Subscribe(x => Trace.WriteLine($"{x}"));
        }
    }
}