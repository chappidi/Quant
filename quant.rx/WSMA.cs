using quant.common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

/// <summary>
/// Wilder’s Smoothing AKA SMoothed Moving Average (WSMA)
/// SUM1=SUM (CLOSE, N)
/// WSMA1 = SUM1/ N
/// WSMA (i) = (SUM1 – WSMA1 + CLOSE(i) )/ N =  ( (WSMA1 * (N-1)) + CLOSE(i) )/N
///
/// The WSMA is almost identical to an EMA of twice the look back period. 
/// In other words, 20-period WSMA is almost identical to a 40-period EMA
/// </summary>
namespace quant.rx
{
    class WSMA : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly uint _period = 0;
        // variables
        double _total = 0;
        double _count = 0;

        #region ctor
        public WSMA(IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            _source = source;
            _offset = offset;
            _period = period;
        }
        #endregion
        void OnVal(double val, IObserver<double> obsvr)
        {
            // edge condition
            if (_period == 1) { obsvr.OnNext(val); return; }

            if (_count < _period) {
                // Initial SMA mode
                _total += val;
                ++_count;
                if (_count == _period)
                    obsvr.OnNext(_total / _period);
            } else {
                // normal calculation
                Debug.Assert(_count == _period);
                double wsma_one = _total / _period;
                _total = ((_total - wsma_one) + val);
                obsvr.OnNext(_total / _period);
            }
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr)
        {
            var ret = new CompositeDisposable();
            // offset calculations
            if (_offset != null) {
                ret.Add(_offset.Subscribe(ofst => _total += _count * ofst));
            }
            ret.Add(_source.Subscribe(val => OnVal(val, obsvr), obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion

    }

    /// <summary>
    /// Local Extensions
    /// </summary>
    internal static partial class WSMAExt
    {
        public static IObservable<double> WSMA_V1(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            uint count = 0;
            double total = 0;
            return Observable.Create<double>(obs => {
                var ret = new CompositeDisposable();
                ret.Add(source.Subscribe(val => {
                    // edge condition
                    if (period == 1) { obs.OnNext(val); return; }
                    // buffer not full
                    if (count < period) {
                        // Initial SMA mode
                        total += val;
                        ++count;
                        if (count == period)
                            obs.OnNext(total / period);
                    } else {
                        // normal calculation
                        double wsma_one = total / period;
                        total = (total - wsma_one + val);
                        obs.OnNext(total / period);
                    }
                }, obs.OnError, obs.OnCompleted));

                if(offset != null) {
                    ret.Add(offset.Subscribe(val => {

                    }, obs.OnError, obs.OnCompleted));
                }
                return ret;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        internal static IObservable<double> WSMA_V2(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return new WSMA(source, period, offset);
        }
    }
    /// <summary>
    /// Global Extensions
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// Standard Extension
        /// </summary>
        public static IObservable<double> WSMA(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return source.WSMA_V2(period, offset);
        }
        /// <summary>
        /// Tick based WSMA . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> WSMA(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).WSMA(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based WSMA of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> WSMA(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).WSMA(period, sr.Offset());
            });
        }

    }
}
