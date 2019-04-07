using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.common;
using quant.rx;

namespace quant.rx.test
{
    [TestClass]
    public class ABANDS_Test
    {
        /// <summary>
        /// Data Source
        /// https://github.com/QuantConnect/Lean/blob/master/Tests/TestData/spy_acceleration_bands_20_4.txt        
        /// </summary>
        static IList<BarData> INPUT
        {
            get {
                return new BarData[] {
                    new BarData("09/03/2015", 196.25, 198.05, 194.96, 195.55),
                    new BarData("09/04/2015", 192.88, 193.86, 191.61, 192.59),
                    new BarData("09/08/2015", 195.97, 197.61, 195.17, 197.43),
                    new BarData("09/09/2015", 199.32, 199.47, 194.35, 194.79),
                    new BarData("09/10/2015", 194.50, 197.22, 194.25, 195.85),
                    new BarData("09/11/2015", 195.32, 196.82, 194.53, 196.74),
                    new BarData("09/14/2015", 196.95, 197.01, 195.43, 196.01),
                    new BarData("09/15/2015", 196.59, 198.99, 195.96, 198.46),
                    new BarData("09/16/2015", 198.82, 200.41, 198.41, 200.18),
                    new BarData("09/17/2015", 199.96, 202.89, 199.28, 199.73),
                    new BarData("09/18/2015", 195.74, 198.68, 194.96, 195.45),
                    new BarData("09/21/2015", 196.45, 197.68, 195.21, 196.46),
                    new BarData("09/22/2015", 193.90, 194.46, 192.56, 193.91),
                    new BarData("09/23/2015", 194.13, 194.67, 192.91, 193.60),
                    new BarData("09/24/2015", 192.13, 193.45, 190.56, 192.90),
                    new BarData("09/25/2015", 194.61, 195.00, 191.81, 192.85),
                    new BarData("09/28/2015", 191.75, 191.91, 187.64, 188.01),
                    new BarData("09/29/2015", 188.24, 189.74, 186.93, 188.12),
                    new BarData("09/30/2015", 190.40, 191.83, 189.44, 191.63),
                    new BarData("10/01/2015", 192.03, 192.49, 189.82, 192.13),
                    new BarData("10/02/2015", 189.75, 195.03, 189.12, 195.00),
                    new BarData("10/05/2015", 196.47, 198.74, 196.33, 198.47),
                    new BarData("10/06/2015", 198.27, 198.98, 197.00, 197.79),
                    new BarData("10/07/2015", 198.85, 199.83, 197.48, 199.41),
                    new BarData("10/08/2015", 198.96, 201.55, 198.59, 201.21),
                    new BarData("10/09/2015", 201.40, 201.90, 200.58, 201.33),
                    new BarData("10/12/2015", 201.43, 201.76, 200.91, 201.52),
                    new BarData("10/13/2015", 200.65, 202.16, 200.05, 200.25),
                    new BarData("10/14/2015", 200.16, 200.87, 198.94, 199.29),
                    new BarData("10/15/2015", 200.05, 202.36, 199.64, 202.35),
                    new BarData("10/16/2015", 202.82, 203.29, 201.92, 203.27),
                    new BarData("10/19/2015", 202.53, 203.37, 202.13, 203.37),
                    new BarData("10/20/2015",202.86,203.84,202.55,203.11),
                    new BarData("10/21/2015",203.64,203.79,201.65,201.85),
                    new BarData("10/22/2015",203,205.51,201.85,205.26),
                    new BarData("10/23/2015",207.24,207.95,206.3,207.51),
                    new BarData("10/26/2015",207.27,207.37,206.56,207),
                    new BarData("10/27/2015",206.2,207,205.79,206.6),
                    new BarData("10/28/2015",207,208.98,206.21,208.95),
                    new BarData("10/29/2015",208.36,209.27,208.21,208.83),
                    new BarData("10/30/2015",209.07,209.44,207.74,207.93)
                };
            }
        }
        static IObservable<OHLC> OHLC
        {
            get
            {
                return INPUT.ToObservable().Select(bd => {
                    var sec = Security.Lookup("DMK3");
                    var oh = new OHLC(new Tick(sec, 1, (uint)(bd.Open * 100), bd.Date));
                    oh.Add(new Tick(sec, 1, (uint)(bd.High * 100), bd.Date));
                    oh.Add(new Tick(sec, 1, (uint)(bd.Low * 100), bd.Date));
                    oh.Add(new Tick(sec, 1, (uint)(bd.Close * 100), bd.Date));
                    return oh;
                });
            }
        }
        [TestMethod]
        public void ABAND_TEST()
        {
            OHLC.Publish(sc => {
                var ftr = sc.Select(oh => 4 * ((double)(oh.High.Price - oh.Low.Price)) / (oh.High.Price + oh.Low.Price));
                var xyz =  ftr.Publish(xs => {
                    var lbx = sc.Zip(xs, (oh, f) => oh.Low.Price * (1.0 - f)).SMA(20);
                    var hbx = sc.Zip(xs, (oh, f) => oh.High.Price * (1.0 + f)).SMA(20);
                    return Observable.When(sc.SMA(20).And(lbx).And(hbx).Then((hl, hl1, hl2) => (hl, hl1, hl2)));
//                    return sc.Zip(xs, (oh, f) => oh.High.Price * (1.0 + f));
                });
                return sc.WithLatestFrom(xyz, (ohlc, bnd) => (ohlc, bnd));
            }).Subscribe(x => Trace.WriteLine($"{x}"));

        }
        [TestMethod]
        public void ABAND_TEST_X()
        {
            OHLC.Publish(sc => {
                return sc.WithLatestFrom(sc.ABANDS(20, 4), (ohlc, bnd) => (ohlc, bnd));
            }).Subscribe(x => Trace.WriteLine($"{x.ohlc}\t{x.bnd}"));
        }
    }
}
