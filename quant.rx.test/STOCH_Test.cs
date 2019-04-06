using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.common;

namespace quant.rx.test
{
    /// <summary>
    /// Data from Excel
    /// https://chartpatterns.files.wordpress.com/2011/11/excel-indicators.xls
    /// </summary>
    [TestClass]
    public class STOCHTest
    {
        [TestMethod]
        public void STOCH_Test()
        {
            var data = BarData.DATA;
            data.ToObservable().Publish(src =>
            {
                var one = src.Select(x => x.High).Max(5);
                var two = src.Select(x => x.Low).Min(5);
                var three = src.Skip(5 - 1).Select(x => x.Close);
                var pattern = one.And(two).And(three);
                return Observable.When(pattern.Then((hh, ll, cl) => new { HH = hh, LL = ll, CLS = cl }));
                //                return src;
            })
            .Select(x => ((x.CLS - x.LL) * 100.0) / (x.HH - x.LL))
            .Subscribe(x => Trace.WriteLine(x));
        }
        [TestMethod]
        public void STOCH_Test_2()
        {
            BarData.OHLC.Publish(sc=> {
                return sc.STOCH(5);
            }).Subscribe(x => Trace.WriteLine(x));
        }
    }
}
