using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using quant.common;

namespace quant.rx
{
    public static class MvngAvgExt
    {
        public static IObservable<OHLC> Continuous(this IObservable<IList<OHLC>> source, double factor)
        {
            return null;
        }
        /// <summary>
        /// Continuous Pricing. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static IObservable<Tuple<OHLC, OHLC>> ContinuousX(this IObservable<IList<OHLC>> source, double factor) {
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
                        Trace.WriteLine($"BAD ROLLING ON {maxV.Open.TradedAt} TO {maxV.Open.Security.Symbol} @ {maxV.Volume} vs {_prev.Open.Security.Symbol} @ {_prev.Volume}\t{((double)maxV.Volume/_prev.Volume).ToString("0.00")}");

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
                            Trace.WriteLine($"BAD ROLLING ON {maxV.Open.TradedAt} TO {maxV.Open.Security.Symbol} @ {maxV.Volume} vs {newV.Open.Security.Symbol} @ {newV.Volume}");
                    }
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
        public static MVWAP_V1 MVWAPX(this IObservable<Tick> source, uint period)
        {
            return new MVWAP_V1(source, period);
        }
    }
}
