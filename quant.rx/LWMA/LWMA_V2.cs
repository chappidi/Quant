using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace quant.rx
{
    class LWMA_V2 : IObservable<double>
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
        public LWMA_V2(IObservable<double> source, uint period, IObservable<double> offset)
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
                    //step 1
                    _weighted += ofst * m_weight;
                    // step 2
                    for (int itr = 0; itr < _count; ++itr) {
                        long idx = (_ring.head + itr) % _period;
                        _ring.buffer[idx] += ofst;
                    }
                    _total += ofst * _count;
                }));
            }
            ret.Add(_source.Subscribe(val => OnVal(val, _ring.Enqueue(val), obsvr), obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion
    }
    internal static partial class LWMAV2Ext
    {
        internal static IObservable<double> LWMA_V2(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new LWMA_V2(source, period, offset);
        }
    }
}
