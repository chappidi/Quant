using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using quant.common;

namespace quant.rx
{
    /// <summary>
    /// Based on SMA. Compare performance with MVWAP_V2
    /// </summary>
    class MVWAP_V3 : IObservable<double>
    {
        readonly IObservable<QTY_PX> _source;
        readonly IObservable<double> _offset;
        readonly uint _period;
        // variables
        readonly RingWnd<double> _ring = null;
        double _total = 0;
        uint _count = 0;

        #region ctor
        public MVWAP_V3(IObservable<QTY_PX> source, uint period, IObservable<double> offset) {
            _source = source;
            _period = period;
            _offset = offset;
            _ring = new RingWnd<double>(period);
        }
        #endregion
        void OnVal(double newVal, double oldVal) {
            // buffer not full
            if (_count < _period) {
                _count++;
                _total += newVal;
            }
            else {
                _total += (newVal - oldVal);
            }
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr)
        {
            var ret = new CompositeDisposable();
            if (_offset != null) {
                ret.Add(_offset.Subscribe(ofst => {
                    for (int itr = 0; itr < _count; ++itr) {
                        long idx = (_ring.head + itr) % _period;
                        _ring.buffer[idx] += ofst;
                    }
                    _total += ofst * _count;
                }));
            }
            ret.Add(_source.Subscribe(val => {
                // calculate  ( Insert QTY times)
                for(int itr =0; itr++ < val.QTY; itr++)
                    OnVal(val.PX, _ring.Enqueue(val.PX));
                // count matches window size  publish
                if (_count == _period)
                    obsvr.OnNext(_total / _period);
            }, obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion
    }
}
