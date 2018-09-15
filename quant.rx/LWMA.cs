using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Text;
using quant.common;

/// <summary>
/// Linear Weighted Moving Averages(5) 
/// 15 = 5+4+3+2+1
/// ((90.9*(5/15))+(90.36*(4/15))+(90.28*(3/15))+(90.83*(2/15))+(90.91*(1/15)))
/// </summary>
namespace quant.rx
{
    class LWMA : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly uint _period;
        readonly uint m_weight = 0;
        //Variables
        readonly RingWnd<double> _ring = null;
        double _total = 0;
        double _weighted = 0;
        uint _count = 0;

        #region ctor
        public LWMA(IObservable<double> source, uint period, IObservable<double> offset)
        {
            _source = source;
            _offset = offset;
            _period = period;
            for(uint i = 1; i <= period; ++i) {
                m_weight += i;
            }

            _ring = new RingWnd<double>(period);
        }
        #endregion
        void OnVal(double newVal, double oldVal, IObserver<double> obsvr)
        {
            // Step 1: calculate
            _weighted += _period * newVal - _total;
            if (_count < _period) {
                _count++;
                _total += newVal;
            } else {
                _total += newVal - oldVal;
            }
            // Step 2: publish
            if (_count == _period)
                obsvr.OnNext(_weighted / m_weight);
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr) { 
            var ret = new CompositeDisposable();
            // offset calculations
            if (_offset != null) {
                ret.Add(_offset.Subscribe(ofst => {
                    // empty
                }));
            }
            ret.Add(_source.Subscribe(val => OnVal(val, _ring.Enqueue(val), obsvr), obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion
    }
    /// <summary>
    /// Local Extensions
    /// </summary>
    internal static partial class LWMAExt
    {
        internal static IObservable<double> LWMA_V1(this IObservable<double> source, int period) {
            uint weight = 0;
            for (uint i = 1; i <= period; ++i) { weight += i; }

            return source.Buffer(period, 1).Where(x => x.Count == period).Select(x => {
                double total = 0;
                for(int i =0; i < period; i++) {
                    total += x[i] * (i+1);
                }
                return total / weight;
            });
        }
        internal static IObservable<double> LWMA_V2(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new LWMA(source, period, offset);
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
        public static IObservable<double> LWMA(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return source.LWMA_V2(period, offset);
        }
        /// <summary>
        /// Tick based WSMA . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> LWMA(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).WSMA(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based WSMA of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> LWMA(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).LWMA(period, sr.Offset());
            });
        }
    }
}
