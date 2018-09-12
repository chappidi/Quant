using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class AroonTest
    {
        [TestMethod]
        public void Aroon_Up_Test()
        {
            var data = BarData.DATA.Select(x => x.High).ToList();
            string aroon = null;
            data.ToObservable().Publish(sr => {
                sr.AroonUp_X(24).Subscribe(x => aroon = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
                Trace.WriteLine($"{val.ToString("0")}\t{aroon}");
            });
        }
        [TestMethod]
        public void Aroon_Down_Test()
        {
            var data = BarData.DATA.Select(x => x.Low).ToList();
            string aroon = null;
            data.ToObservable().Publish(sr => {
                sr.AroonDown_X(24).Subscribe(x => aroon = x.ToString("0.00"));
                return sr;
            }).Subscribe(val => {
                Trace.WriteLine($"{val.ToString("0")}\t{aroon}");
            });
        }
    }
}
