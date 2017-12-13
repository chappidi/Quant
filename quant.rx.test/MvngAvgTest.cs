using System;
using System.Reactive.Subjects;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;
using System.Threading;
using quant.common;

namespace quant.rx.test
{
    [TestClass]
    public class MvngAvgTest
    {
        [TestMethod]
        public void EMATest()
        {
            double[] items = { 22.27, 22.19, 22.08, 22.17, 22.18, 22.13, 22.23, 22.43, 22.24, 22.29, 22.15, 22.39, 22.38, 22.61, 23.36 };
            var ticks = new Subject<double>();
            ticks.EMA(10).Subscribe(x => { Trace.WriteLine(x.ToString("0.00")); });
            foreach (var val in items) {
                ticks.OnNext(val);
            }
        }
        [TestMethod]
        public void ABCTest()
        {
            double[] items = { 22.27, 22.19, 22.08, 22.17, 22.18, 22.13, 22.23, 22.43, 22.24, 22.29, 22.15, 22.39, 22.38, 22.61, 23.36 };
            var src = items.ToObservable().Publish();
//            src.EMA(5).WithLatestFrom(src.EMA(10), (x,y)=> new Tuple<double, double>(x,y)).Subscribe(x => { Trace.WriteLine($"{x.Item1.ToString("0.00")}\t{x.Item2.ToString("0.000")}"); });
            src.WithLatestFrom(src, (x, y) => new Tuple<double, double>(x, y)).Subscribe(x => { Trace.WriteLine($"{x.Item1.ToString("0.00")}\t{x.Item2.ToString("0.000")}"); });
            src.Connect();
            // items.ToObservable().EMA(5).Subscribe(x => { Trace.WriteLine($"X-{x.ToString("0.00")}"); });
//            items.ToObservable().EMA(10).Subscribe(x => { Trace.WriteLine(x.ToString("0.000")); });
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
            srcObs.MVWAP(1500).WithLatestFrom(srcObs, (x, y) => new Tuple<double, Tick>(x, y)).Subscribe(x => { Trace.WriteLine($"{x.Item1.ToString("0.000")}\t{x.Item2.Time}\t{x.Item2.Quantity}\t{x.Item2.Price}"); });
//            srcObs.MVWAP(1500).Subscribe(x => { Trace.WriteLine($"OHLC:\t{x.ToString("0.00")}"); });
            var dtStart = DateTime.Now;
            srcObs.Connect();
            Trace.WriteLine($"{(DateTime.Now - dtStart).TotalMilliseconds}");
        }
        [TestMethod]
        public void FromCSVTest3()
        {
            var srcObs = BarGenMethod.FromCSV(@"D:\GIT_DIR_VSTS\CC_TICK.txt").Publish();
            var vwObs = srcObs.MVWAP(1500,20);
            vwObs.Subscribe(x => Trace.WriteLine($"OHLC:\t{x.Open.Time.ToEST()}\t{x.Close.Time.ToEST()}\tVOL:{x.Volume}\t{x.High.Price - x.Low.Price}"));
            var dtStart = DateTime.Now;
            srcObs.Connect();
            Trace.WriteLine($"{(DateTime.Now - dtStart).TotalMilliseconds}");
        }
        [TestMethod]
        public void BarGenTest()
        {
            var srcObs = BarGenMethod.FromCSV(@"D:\GIT_DIR_VSTS\CC_TICK.txt").Publish();
            srcObs.ByPrice(40).Subscribe(x => {
                Trace.WriteLine($"OHLC-X:\t{x.Open.Time.ToEST()}\t{x.Close.Time.ToEST()}\tVOL:{x.Volume}\t{x.High.Price - x.Low.Price}");
            });
            srcObs.ByVol(1000).Subscribe(x => {
                Trace.WriteLine($"OHLC-Y:\t{x.Open.Time.ToEST()}\t{x.Close.Time.ToEST()}\tVOL:{x.Volume}\t{x.High.Price - x.Low.Price}");
            });
            srcObs.ByInterval(TimeSpan.FromHours(1)).Subscribe(x => {
                Trace.WriteLine($"OHLC-Z:\t{x.Open.Time.ToEST()}\t{x.Close.Time.ToEST()}\tVOL:{x.Volume}");
            });
            srcObs.Connect();
        }
    }
}
