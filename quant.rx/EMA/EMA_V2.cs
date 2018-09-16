using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace quant.rx
{
    class EMA_V2 : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly uint _period = 0;
        readonly double m_factor = 0;
        // variables
        double m_ema = 0;
        double m_count = 0;

        #region ctor
        public EMA_V2(IObservable<double> source, uint period, IObservable<double> offset = null) {
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
                // count matches window size
                if (m_count == _period) {
                    m_ema = m_ema / m_count;
                    obsvr.OnNext(m_ema);
                }
            } else {
                m_ema = m_ema + (val - m_ema) * m_factor;
                obsvr.OnNext(m_ema);
            }
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
    /// 
    /// </summary>
    internal static class EMAV2Ext
    {
        internal static IObservable<double> EMA_V2(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new EMA_V2(source, period, offset);
        }
    }
}
