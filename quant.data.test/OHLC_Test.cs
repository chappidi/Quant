using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using quant.core;
using quant.core.futures;

namespace quant.data.test
{
    [TestClass]
    public class OHLC_Test
    {
       static string prdt = "ES";
        static DateTime dtStart = DateTime.Parse("03-13-2018");
        [TestMethod]
        public void OHLC_TIME_1()
        {
            var timeSpan = TimeSpan.FromHours(1);
            var source = new Subscription(prdt, Resolution.Tick).Query(dtStart, dtStart.AddDays(1)).ToObservable();
            var abc = source.Publish(src => {
                return src.Bucket_V2(timeSpan);
            });
            abc.Take(100).Subscribe(lt => {
                foreach (var itm in lt) {
                    Trace.WriteLine(itm);
                }
                Trace.WriteLine("\n");
            });
        }
        [TestMethod]
        public void OHLC_TIME_2()
        {
            var timeSpan = TimeSpan.FromHours(1);
            var source = new Subscription(prdt, Resolution.Tick).QueryX(dtStart, dtStart.AddDays(1));
            var abc = source.Publish(src => {
                return src.Bucket_V2(timeSpan);
            });
            abc.Take(100).Subscribe(lt => {
                foreach (var itm in lt)
                {
                    Trace.WriteLine(itm);
                }
                Trace.WriteLine("\n");
            });
        }
        [TestMethod]
        public void RunningSUM()
        {
            Observable.Repeat(1, 19).Scan((acum, item) => {
                return (acum + item)%3;
            }).Subscribe(x => Trace.WriteLine(x));
            Observable.Repeat(1,19).Scan((acum, item) => {
                return acum + item;
            }).Select(x => x/5).DistinctUntilChanged().Subscribe(x => Trace.WriteLine(x));
        }
    }
}
