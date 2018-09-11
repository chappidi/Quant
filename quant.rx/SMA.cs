using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    public static partial class QuantExt
    {
        public static IObservable<double> SMA(this IObservable<Tuple<OHLC, OHLC>> source, uint period)
        {
            Debug.Assert(period > 1);
            return Observable.Create<double>(obs => {
                double total = 0;
                double count = 0;   // count of elements
                return source.RollingWindow(period).Subscribe(
                    (input) => {
                        var addE = input.Item1; // item to be added
                        var subE = input.Item2; // item to be removed
                        var offset = addE.Item1.Close.Price - addE.Item2.Close.Price;  // offset for adjustment

                        // buffer not full
                        if (count < period)
                            count++;
                        else
                        {
                            Debug.Assert(count == period);
                            total -= subE.Item2.Close.Price;    // remove old Value
                        }

                        // adjust add price so that we can increase for all elements
                        total += (addE.Item1.Close.Price - offset);
                        // increase offset for all elements
                        if (offset != 0)
                            total += count * offset;

                        // count matches window size
                        if (count == period)
                            obs.OnNext(total / period);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> SMA(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs => {
                double total = 0;
                double count = 0;   // count of elements
                return source.RollingWindow(period).Subscribe(
                    (val) => {
                        // add to the total sum
                        total += val.Item1;
                        // buffer not full
                        if (count < period)
                            count++;
                        else
                            total -= val.Item2;

                        // count matches window size
                        if (count == period)
                            obs.OnNext(total / period);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
    }
}
