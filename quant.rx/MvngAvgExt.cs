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
                    if (maxV.Open.Security.Symbol == _prev.Open.Security.Symbol)    // No rolling happening
                    {
                        obs.OnNext(new Tuple<OHLC, OHLC>(maxV, maxV));
                        return;
                    }

                    // found a new symbol with higher volume

                    // find the OHLC of prev symbol
                    _prev = lt.First(x => x.Open.Security.Symbol == _prev.Open.Security.Symbol);
                    Debug.Assert(_prev != null);
                    // new volume is greater by factor and the new Symbol in next contract (either year is greater or month is greater)
                    if (maxV.Volume > (_prev.Volume * factor) && maxV.Open.Security.MonthCode > _prev.Open.Security.MonthCode)
                    {
                        obs.OnNext(new Tuple<OHLC, OHLC>(maxV, _prev));
                        _prev = maxV;
                    }
                    else
                    {
                        obs.OnNext(new Tuple<OHLC, OHLC>(_prev, _prev));
                    }
                    // Error Logging
                    if (maxV.Open.Security.MonthCode < _prev.Open.Security.MonthCode)
                        Trace.WriteLine($"BAD ROLLING ON {maxV.Open.Time} TO {maxV.Open.Security.Symbol} @ {maxV.Volume} vs {_prev.Open.Security.Symbol} @ {_prev.Volume}\t{((double)maxV.Volume/_prev.Volume).ToString("0.00")}");

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
                    if (maxV.Open.Security.Symbol != _prev.Open.Security.Symbol) {
                        // find the OHLC of current symbol
                        var newV = lt.First(x => x.Open.Security.Symbol == _prev.Open.Security.Symbol);
                        // new volume is greater by factor and the new Symbol in next contract (either year is greater or month is greater)
                        Debug.Assert(newV != null);
                        if (maxV.Volume > (newV.Volume * factor) && maxV.Open.Security.MonthCode > newV.Open.Security.MonthCode)
                        {
                            _prev = maxV;
                            obs.OnNext(new Tuple<OHLC, OHLC>(maxV, newV));
                        }
                        // Error Logging
                        if (maxV.Open.Security.MonthCode < newV.Open.Security.MonthCode) 
                            Trace.WriteLine($"BAD ROLLING ON {maxV.Open.Time} TO {maxV.Open.Security.Symbol} @ {maxV.Volume} vs {newV.Open.Security.Symbol} @ {newV.Volume}");
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
        /// https://www.investopedia.com/ask/answers/031115/what-common-strategy-traders-implement-when-using-volume-weighted-average-price-vwap.asp
        /// https://tradingsim.com/blog/vwap-indicator/
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> MVWAP(this IObservable<Tick> source, uint period)
        {
            return new MVWAP(source, period);
        }
    }
}
