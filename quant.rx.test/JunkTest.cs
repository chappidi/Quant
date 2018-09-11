using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.common;

namespace quant.rx.test
{
    [TestClass]
    public class JunkTest
    {
        [TestMethod]
        public void FromCSVOHLC()
        {
            var srcObs = BarGenMethod.OHLCFromCSV(@"D:\GIT_DIR_VSTS\CL_OHLC_2017.txt");
            var cc = srcObs.ToObservable().Continuous(1.2).EMA(10).Subscribe(x => Trace.WriteLine(x));
            srcObs.ToObservable().Continuous(1.2).Subscribe(x => {
                Trace.WriteLine($"{x.Item1.Close.TradedAt}\t{x.Item1.Open.Security.Symbol}\t{x.Item1.Volume}\t{x.Item1.Close.Price - x.Item2.Close.Price}");
            });
        }
        [TestMethod]
        public void FromCSVTest()
        {
//            BarGenMethod.FromCSV(@"D:\GIT_DIR_VSTS\CC_TICK.txt").Bucket((x) => x.Time.Hour).OHLC().Subscribe(x => Trace.WriteLine($"OHLC:\t{x.Open.Time.ToEST()}\t{x.Close.Time.ToEST()}\tVOL:{x.Volume}"));
//            BarGenMethod.FromCSV(@"D:\GIT_DIR_VSTS\CC_TICK.txt").MVWAP(1500).Subscribe(x => Trace.WriteLine($"OHLC:\t{x.ToString("0.00")}"));
            var srcObs = Observable.Interval(TimeSpan.FromMilliseconds(100)).Publish();
            var scnObsX = srcObs.Scan((acum, item) => acum + 1);
            var scnObs = srcObs.Scan((acum, item) => {
                return acum + item;
            });
            scnObs.Zip(scnObsX, (x,y)=>new Tuple<long,long>(x,y)).Subscribe(x => { Trace.WriteLine(x); });
            srcObs.Connect();
            Thread.Sleep(2000);
        }
        [TestMethod]
        public void FromCSVTest2()
        {
            var srcObs = BarGenMethod.FromCSV(@"D:\GIT_DIR_VSTS\CC_TICK.txt").Publish();
            srcObs.MVWAP(1500).WithLatestFrom(srcObs, (x, y) => new Tuple<double, Tick>(x, y)).Subscribe(x => { Trace.WriteLine($"{x.Item1.ToString("0.000")}\t{x.Item2.TradedAt}\t{x.Item2.Quantity}\t{x.Item2.Price}"); });
//            srcObs.MVWAP(1500).Subscribe(x => { Trace.WriteLine($"OHLC:\t{x.ToString("0.00")}"); });
            var dtStart = DateTime.Now;
            srcObs.Connect();
            Trace.WriteLine($"{(DateTime.Now - dtStart).TotalMilliseconds}");
        }
        [TestMethod]
        public void FromCSVTest3()
        {
            var srcObs = BarGenMethod.FromCSV(@"D:\GIT_DIR_VSTS\CC_TICK.txt").ToList().Wait();
            var dtStart = DateTime.Now;
            var vwObs = srcObs.ToObservable().Publish(x => x.MVWAP(1500).OHLC(x, 20));
            vwObs.Subscribe(x => { /*Trace.WriteLine($"OHLC:\t{x.Open.Time.ToEST()}\t{x.Close.Time.ToEST()}\tVOL:{x.Volume}\t{x.High.Price - x.Low.Price}");*/ });
            Trace.WriteLine($"{(DateTime.Now - dtStart).TotalMilliseconds}");
        }
        [TestMethod]
        public void BarGenTest()
        {
            var srcObs = BarGenMethod.FromCSV(@"D:\GIT_DIR_VSTS\CC_TICK.txt").Publish();
            srcObs.ByPrice(40).Subscribe(x => {
                Trace.WriteLine($"OHLC-X:\t{x.Open.TradedAt.ToEST()}\t{x.Close.TradedAt.ToEST()}\tVOL:{x.Volume}\t{x.High.Price - x.Low.Price}");
            });
            srcObs.ByVol(1000).Subscribe(x => {
                Trace.WriteLine($"OHLC-Y:\t{x.Open.TradedAt.ToEST()}\t{x.Close.TradedAt.ToEST()}\tVOL:{x.Volume}\t{x.High.Price - x.Low.Price}");
            });
            srcObs.ByInterval(TimeSpan.FromHours(1)).Subscribe(x => {
                Trace.WriteLine($"OHLC-Z:\t{x.Open.TradedAt.ToEST()}\t{x.Close.TradedAt.ToEST()}\tVOL:{x.Volume}");
            });
            srcObs.Connect();
        }
   }
}
