using quant.common;
using quant.core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace quant.rx
{
    public static class MvngAvgExt
    {
        /// <summary>
        /// Continuous Pricing. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static IObservable<Tuple<OHLC, OHLC>> Continuous(this IObservable<IList<OHLC>> source, double factor) {
            return Observable.Create<Tuple<OHLC, OHLC>>(obs => {
                OHLC _prev = null;
                return source.Subscribe(lt => {
                    if (lt.Count == 0) return;
                    // find the ohlc with max volume
                    var maxV = lt.First(x => x.Volume == lt.Max(i => i.Volume));

                    _prev = _prev ?? maxV;                    
                    if (maxV.Symbol == _prev.Symbol)    // No rolling happening
                    {
                        obs.OnNext(new Tuple<OHLC, OHLC>(maxV, maxV));
                        return;
                    }

                    // found a new symbol with higher volume

                    // find the OHLC of prev symbol
                    _prev = lt.First(x => x.Symbol == _prev.Symbol);
                    Debug.Assert(_prev != null);
                    // new volume is greater by factor and the new Symbol in next contract (either year is greater or month is greater)
                    if (maxV.Volume > (_prev.Volume * factor) && maxV.MonthCode > _prev.MonthCode)
                    {
                        obs.OnNext(new Tuple<OHLC, OHLC>(maxV, _prev));
                        _prev = maxV;
                    }
                    else
                    {
                        obs.OnNext(new Tuple<OHLC, OHLC>(_prev, _prev));
                    }
                    // Error Logging
                    if (maxV.MonthCode < _prev.MonthCode)
                        Trace.WriteLine($"BAD ROLLING ON {maxV.Open.Time} TO {maxV.Symbol} @ {maxV.Volume} vs {_prev.Symbol} @ {_prev.Volume}\t{((double)maxV.Volume/_prev.Volume).ToString("0.00")}");

                }, obs.OnError, obs.OnCompleted);
            });
        }

        public static IObservable<Tuple<OHLC, OHLC>> Roll(this IObservable<IList<OHLC>> source, double factor) {
            return Observable.Create<Tuple<OHLC, OHLC>>(obs => {
                OHLC _prev = null;
                return source.Subscribe(lt => {
                    if (lt.Count == 0) return;
                    // find the ohlc with max volume
                    var maxV = lt.First(x => x.Volume == lt.Max(i => i.Volume));

                    // First Iteration
                    if (_prev == null) {
                        _prev = maxV;
                        obs.OnNext(new Tuple<OHLC, OHLC>(_prev, _prev));
                        return;
                    }

                    // New Symbol has higher volume
                    if (maxV.Symbol != _prev.Symbol) {
                        // find the OHLC of current symbol
                        var newV = lt.First(x => x.Symbol == _prev.Symbol);
                        // new volume is greater by factor and the new Symbol in next contract (either year is greater or month is greater)
                        Debug.Assert(newV != null);
                        if (maxV.Volume > (newV.Volume * factor) && maxV.MonthCode > newV.MonthCode)
                        {
                            _prev = maxV;
                            obs.OnNext(new Tuple<OHLC, OHLC>(maxV, newV));
                        }
                        // Error Logging
                        if (maxV.MonthCode < newV.MonthCode) 
                            Trace.WriteLine($"BAD ROLLING ON {maxV.Open.Time} TO {maxV.Symbol} @ {maxV.Volume} vs {newV.Symbol} @ {newV.Volume}");
                    }
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> EMA(this IObservable<Tuple<OHLC, OHLC>> source, uint period)
        {
            Debug.Assert(period > 1);

            return Observable.Create<double>(obs => {
                double factor = (2.0 / (period + 1));
                int count = 0;
                double ema = 0;
                return source.Subscribe((val) => {
                    var input = val.Item1.Close.Price;
                    var offset = val.Item1.Close.Price - val.Item2.Close.Price;
                    // buffer not full
                    if (count < period) {
                        if(offset != 0)
                            ema += count * offset;    // roll offset
                        count++;
                        ema += input;
                        if (count == period)
                            obs.OnNext(ema / period);
                    }
                    else {
                        if (offset != 0)
                            ema += offset;
                        ema = (input * factor) + (ema * (1.0 - factor));
                        obs.OnNext(ema);
                    }
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> EMA(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs => {
                var ema = new EMA(period);
                return source.Subscribe( (val) => {
                        var retVal = ema.Calc(val);
                        if (!double.IsNaN(retVal))
                            obs.OnNext(retVal);
                    }, obs.OnError, obs.OnCompleted);
            });
        }

        public static IObservable<double> WSMA(this IObservable<Tuple<OHLC, OHLC>> source, uint period)
        {
            Debug.Assert(period > 1);
            return Observable.Create<double>(obs => {
                int count = 0;
                double wsma = 0;
                return source.Subscribe(val => {
                    var offset = val.Item1.Close.Price - val.Item2.Close.Price;
                    var input = val.Item1.Close.Price;
                    if (offset != 0)
                        wsma += count * offset;    // roll offset
                    // buffer not full
                    if (count < period) {
                        count++;
                        wsma += input;
                        if (count == period)
                            obs.OnNext(wsma / period);
                    } else {
                        Debug.Assert(count == period);
                        double wsma_one = wsma / period;
                        wsma = (wsma - wsma_one + input);
                        obs.OnNext(wsma / period);
                    }
                }, obs.OnError, obs.OnCompleted);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> WSMA(this IObservable<double> source, uint period)
        {
            return Observable.Create<double>(obs => {
                var ema = new WSMA(period);
                return source.Subscribe( (val) => {
                        var retVal = ema.Calc(val);
                        if (!double.IsNaN(retVal))
                            obs.OnNext(retVal);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> MVWAP(this IObservable<Tick> source, uint period)
        {
            // maintain symbol offset
            var symOff = new Dictionary<string, double>();
            return Observable.Create<double>(obs => {
                LinkedList<Tick>  que = null;
                double pxVol = 0;  // price * Vol
                uint Vol = 0;   // volume
                return source.Subscribe( (newTck) => {

                    // initialization
                    if(que == null) {
                        que = new LinkedList<Tick>();
                        symOff[newTck.Symbol] = 0;
                    } else {
                        double offset = (que.Last().Symbol != newTck.Symbol) ? (offset = newTck.Price - que.Last().Price) : 0;
                        if (offset != 0) {
                            foreach(var itm in symOff)
                                symOff[itm.Key] = itm.Value + offset;       // change offsets
                            symOff[newTck.Symbol] = 0;                      // add new element
                            pxVol += Vol * offset;                          // update pxVol
                        }
                    }

                    que.AddLast(newTck);    // add to the end                    
                    pxVol += newTck.PxVol;  // add to the total sum and volume
                    Vol += newTck.Quantity;

                    // if volume exceeded the limit
                    while(Vol > period) {
                        // remove  old value
                        var oldTck = que.First.Value;
                        que.RemoveFirst();
                        var offset = symOff[oldTck.Symbol];
                        if (oldTck.Quantity + period > Vol) {
                            // find amount to reduce
                            uint diff = Vol - period;
                            // add back the difference
                            que.AddFirst(new Tick(oldTck.Symbol, oldTck.Quantity - diff, oldTck.Price, oldTck.Time, oldTck.Side, oldTck.Live));
                            // reduce the aggregate amounts
                            pxVol -= (oldTck.Price + offset) * diff;
                            Vol -= diff;
                        }
                        else {
                            // reduce the aggregate amounts
                            pxVol -= (oldTck.Price + offset) * oldTck.Quantity;
                            Vol -= oldTck.Quantity;
                        }
                    }
                    // count matches window size
                    if (Vol >= period)
                        obs.OnNext(pxVol / period);
                    }, obs.OnError, obs.OnCompleted);
            });
        }
    }
}
