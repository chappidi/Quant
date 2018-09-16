using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Text;

namespace quant.rx
{
    class WSMA_V2 : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly uint _period = 0;
        // variables
        double _total = 0;
        double _count = 0;

        #region ctor
        public WSMA_V2(IObservable<double> source, uint period, IObservable<double> offset = null)
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
    /// 
    /// </summary>
    internal static class WSMAV2Ext
    {
        internal static IObservable<double> WSMA_V2(this IObservable<double> source, uint period, IObservable<double> offset = null) {
            return new WSMA_V2(source, period, offset);
        }
    }
}
