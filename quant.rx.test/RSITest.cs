using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace quant.rx.test
{
    [TestClass]
    public class RSITest
    {
        /// <summary>
        /// Data From ExcelSheet on Page
        /// http://stockcharts.com/school/lib/exe/fetch.php?media=chart_school:technical_indicators_and_overlays:relative_strength_in:cs-rsi.xls
        /// </summary>
        [TestMethod]
        public void RSI_Test()
        {
            double[] items = {
                44.3389, 44.0902, 44.1497, 43.6124, 44.3278,
                44.8264, 45.0955, 45.4245, 45.8433, 46.0826,
                45.8931, 46.0328, 45.6140, 46.2820, 46.2820,
                46.0028, 46.0328, 46.4116, 46.2222, 45.6439,
                46.2122, 46.2521, 45.7137, 46.4515, 45.7835,
                45.3548, 44.0288, 44.1783, 44.2181, 44.5672,
                43.4205, 42.6628, 43.1314};

            string rs = null;
            string rsi = null;
            items.ToObservable().Publish(sr => {
                sr.RS(14).Subscribe(x => rs = x.ToString());
                sr.RSI(14).Subscribe(x => rsi = x.ToString());
                return sr;
            }).Subscribe(val=> {
                Trace.WriteLine($"{val.ToString("0.0000")}\t{rs}\t{rsi}");
            });
        }
    }
}
