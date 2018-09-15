using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// Running Total
    /// </summary>
    internal class SUM_V1 : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly uint _period;
        // variables
        readonly RingWnd<double> _ring = null;
        double _total = 0;
        uint _count = 0;

        #region ctor
        public SUM_V1(IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            _source = source;
            _period = period;
            _offset = offset;

            _ring = new RingWnd<double>(period);
        }
        #endregion
        void OnVal(double newVal, double oldVal, IObserver<double> obsvr)
        {
            // buffer not full
            if (_count < _period)
            {
                _count++;
                _total += newVal;
            }
            else
            {
                _total += newVal - oldVal;
            }
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr)
        {
            var ret = new CompositeDisposable();
            // offset calculations are associated with future product rolls.
            if (_offset != null)
            {
                ret.Add(_offset.Subscribe(ofst =>
                {
                    for (int itr = 0; itr < _count; ++itr)
                    {
                        long idx = (_ring.head + itr) % _period;
                        _ring.buffer[idx] += ofst;
                    }
                    _total += ofst * _count;
                }));
            }
            // data subscription
            ret.Add(_source.Subscribe(val =>
            {
                OnVal(val, _ring.Enqueue(val), obsvr);                  //    calculate                
                if (_count == _period) obsvr.OnNext(_total);           //     publish
            }, obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion
    }
}
