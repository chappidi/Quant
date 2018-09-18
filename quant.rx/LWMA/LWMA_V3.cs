using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    class LWMA_V3 : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly uint _period;
        readonly uint m_weight = 0;
        //Variables
        double _total = 0;
        double _weighted = 0;
        uint _count = 0;
        #region ctor
        public LWMA_V3(IObservable<double> source, uint period, IObservable<double> offset)
        {
            _source = source.Publish().RefCount();
            _offset = offset?.Publish().RefCount();
            _period = period;
            for(uint i = 1; i <= period; ++i) {
                m_weight += i;
            }
        }
        #endregion
        void OnVal(double val)
        {
            _weighted += _period * val - _total;
            if (_count < _period) {
                _count++;
                _total += val;
            } 
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr) { 
            var ret = new CompositeDisposable();
            // step 1: offset calculations
            if (_offset != null) {
                ret.Add(_offset.Subscribe(ofst => {
                    _weighted += ofst * m_weight;
                    _total += ofst * _count;
                }));
            }
            // step 2: first the value
            ret.Add(_source.Subscribe(val => {
                OnVal(val);                             //calculate
                if (_count == _period)                  // publish
                    obsvr.OnNext(_weighted / m_weight);
            }, obsvr.OnError, obsvr.OnCompleted));
            // step 3: the sum. It uses perv sum.
            ret.Add(_source.SUM(_period, _offset).Subscribe(val => _total = val));
            return ret;
        }
        #endregion
    }
    internal static partial class LWMAV3Ext
    {
        internal static IObservable<double> LWMA_V3(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new LWMA_V3(source, period, offset);
        }
    }
}
