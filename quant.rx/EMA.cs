using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using quant.common;

/// <summary>
/// Exponential Moving Average
/// Initial SMA: 10-period sum / 10 
/// Multiplier: (2 / (Time periods + 1) ) = (2 / (10 + 1) ) = 0.1818 (18.18%)
/// EMA: {Close - EMA(previous day)} x multiplier + EMA(previous day)
/// EMA = Close * multiplier + EMA(previous day) x { 1 - multiplier}
/// </summary>
namespace quant.rx
{
    class EMA : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly uint _period = 0;
        readonly double m_factor = 0;
        // variables
        double m_ema = 0;
        double m_count = 0;

        #region ctor
        public EMA(IObservable<double> source, uint period, IObservable<double> offset = null) {
            _source = source;
            _offset = offset;
            _period = period;
            m_factor = (2.0 / (period + 1));
        }
        #endregion
        void OnVal(double val, IObserver<double> obsvr)
        {
            // edge condition
            if (_period == 1) { obsvr.OnNext(val); return; }

            if (m_count < _period) {
                // Initial SMA mode
                m_ema += val;
                ++m_count;
                if (m_count != _period)
                    return;
                // count matches window size
                m_ema = m_ema / m_count;
            }
            else
                m_ema = m_ema + (val - m_ema) * m_factor;

            obsvr.OnNext(m_ema);
        }

        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr)
        {
            var ret = new CompositeDisposable();
            // offset calculations
            if (_offset != null) {
                ret.Add(_offset.Subscribe(ofst => m_ema += (m_count < _period) ? m_count * ofst : ofst));
            }
            ret.Add(_source.Subscribe(val => OnVal(val, obsvr), obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion
    }
    /// <summary>
    /// Local Extensions
    /// </summary>
    internal static partial class EMAExt
    {
        internal static IObservable<double> EMA_V1(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            double factor = 2.0 / (period + 1);
            uint count = 0;
            double ema = 0;
            return Observable.Create<double>(obs => {
                var ret = new CompositeDisposable();
                ret.Add(source.Subscribe(val => {
                    // edge condition
                    if (period == 1) { obs.OnNext(val); return; }
                    // buffer not full
                    if (count < period) {
                        // Initial SMA mode
                        ema += val;
                        ++count;
                        if (count != period)
                            return;
                        // count matches window size
                        ema = ema / count;
                    } else {
                        // normal calculation
                        ema = ema + (val - ema) * factor;
                    }
                    obs.OnNext(ema);
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
        internal static IObservable<double> EMA_V2(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return new EMA(source, period, offset);
        }
    }

    /// <summary>
    /// Global Extension.
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// Standard Extension
        /// </summary>
        public static IObservable<double> EMA(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return source.EMA_V2(period, offset);
        }
        /// <summary>
        /// Tick based EMA . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> EMA(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).EMA(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based EMA of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> EMA(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).EMA(period, sr.Offset());
            });
        }
    }
}
