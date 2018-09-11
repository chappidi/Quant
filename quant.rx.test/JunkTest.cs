using System;
using System.Reactive.Subjects;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;
using System.Threading;
using quant.common;
using System.Linq;

namespace quant.rx.test
{
    [TestClass]
    public class JunkTest
    {
         [TestMethod]
        public void RSITest()
        {
            double[] items = { 44.34, 44.09, 44.15, 43.61, 44.33, 44.83, 45.10, 45.42, 45.84, 46.08, 45.89, 46.03, 45.61, 46.28, 46.28, 46.00, 46.03, 46.41, 46.22, 45.64, 46.21, 46.25, 45.71, 46.45, 45.78, 45.35, 44.03, 44.18, 44.22, 44.57, 43.42, 42.66, 43.13 };

            var ticks = new Subject<double>();
            ticks.EMA(10).Subscribe(x => { Trace.WriteLine(x.ToString("0.00")); });
            foreach (var val in items)
            {
                ticks.OnNext(val);
            }
        }
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

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void RSIObsTest()
        {

 /*
            Value1 = .1*(RSI(Close, 5) - 50);
            Value2 = WAverage(Value1, 9);
            IFish = (ExpValue(2*Value2) - 1) / (ExpValue(2*Value2) + 1);


            RSI-based trading system:

            a = rsi(14);
            buy = a > 70;

            Inverse Fisher Transform-based trading system:

            a = 0.1 * ( rsi(14) - 50 );
            a = inverseft(a);
            a = (a + 1) * 50; // Restore the initial interval [0-100]
            buy = a > 70;
*/
            var srcObs = BarGenMethod.FromCSV(@"D:\GIT_DIR_VSTS\CC_TICK.txt");
            srcObs.Select(x => (double)x.Price).RSI(14).Select(z => 0.1 *(z-50)).EMA(9).IFish().Select(a => (int)((a+1)*50)).Subscribe(r => Trace.WriteLine(r));
            Console.ReadLine();
        }
   }
}
