using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace quant.rx
{

    /// <summary>
    /// Using RingWnd. Helps do adjustments for future rolls
    /// </summary>
    class SMA_V3 : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        readonly uint _period;
        // variables
        readonly RingWnd<double> _ring = null;
        double _total = 0;
        uint _count = 0;

        #region ctor
        public SMA_V3(IObservable<double> source, uint period, IObservable<double> offset = null) {
            _source = source;
            _period = period;
            _offset = offset;

            _ring = new RingWnd<double>(period);
        }
        #endregion
        void OnVal(double newVal, double oldVal, IObserver<double> obsvr)
        {
            // add to the total sum
            _total += newVal;
            // buffer not full
            if (_count < _period)
                _count++;
            else
                _total -= oldVal;

            // count matches window size
            if (_count == _period)
                obsvr.OnNext(_total / _period);
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr)
        {
            var ret = new CompositeDisposable();
            // offset calculations are associated with future product rolls.
            if (_offset != null) {
                ret.Add(_offset.Subscribe(ofst => {
                    for (int itr = 0; itr < _count; ++itr) {
                        long idx = (_ring.head + itr) % _period;
                        _ring.buffer[idx] += ofst;
                    }
                    _total += ofst * _count;
                }));
            }
            // data subscription
            ret.Add(_source.Subscribe(val => OnVal(val, _ring.Enqueue(val), obsvr), obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion
    }
    internal static class SMAV3Ext
    {
        /// <summary>
        /// VERSION 3:  Using RingWnd( Performance and Roll Adjustments)
        /// Performance: Avoid Rollingwindow create Tuples.
        /// </summary>
        internal static IObservable<double> SMA_V3(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new SMA_V3(source, period, offset);
        }
    }
}
