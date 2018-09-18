using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    public static class OHLCExt
    {
        /// <summary>
        /// OHLC for MVWAP
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tkSrc"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static IObservable<OHLC> OHLC(this MVWAP_V1 source, IObservable<Tick> tkSrc, uint range)
        {
            return Observable.Create<OHLC>(obs => {
                // variables
                var ret = new CompositeDisposable();
                double maxVal = double.MinValue;
                double minVal = double.MaxValue;
                Tick tck = null;
                OHLC ohlc = null;

                // subscribe to tick data
                ret.Add(tkSrc.Subscribe(x => {
                    if (tck != null && tck.Security != x.Security) {
                        double offset = (x.Price - tck.Price);
                        maxVal += offset;
                        minVal += offset;
                    }
                    tck = x;
                }));
                // subscribe to mvwap
                ret.Add(source.Subscribe((vw) => {
                    // if new bar
                    if (ohlc == null)
                        ohlc = new OHLC(tck);
                    else
                        ohlc.Add(tck);

                    // capture max and min moving vwap
                    maxVal = Math.Max(vw, maxVal);
                    minVal = Math.Min(vw, minVal);

                    // if range is exceeded.
                    if (maxVal - minVal >= range) {
//                        Trace.WriteLine(source.Seed);
//                        Trace.WriteLine($"\tX\t{tk.Time}\t{maxVal-minVal}");
                        // publish the bar
                        obs.OnNext(ohlc);
                        // reset state
                        maxVal = double.MinValue;
                        minVal = double.MaxValue;
                        ohlc = null;
                    }
                }, obs.OnError, () => {
                    //publish final bar
                    obs.OnNext(ohlc);
                    obs.OnCompleted();
                }));
                return ret;
            });
        }
    }
}
