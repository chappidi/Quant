using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Text;
using quant.common;

namespace quant.rx
{
    /// <summary>
    /// Using Rolling Window
    /// </summary>
    class SMA_V2 : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly uint _period;
        // variables
        double _total = 0;
        uint _count = 0;

        #region ctor
        public SMA_V2(IObservable<double> source, uint period) {
            _source = source;
            _period = period;
        }
        #endregion
        void OnVal(double newVal, double oldVal, IObserver<double> obsvr) {
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
        public IDisposable Subscribe(IObserver<double> obsvr) {
            return _source.RollingWindow(_period).Subscribe(val => OnVal(val.Item1, val.Item2, obsvr), obsvr.OnError, obsvr.OnCompleted);
        }
        #endregion
    }

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

    /// <summary>
    /// Local Extension.
    /// </summary>
    internal static class SMAExt {
        /// <summary>
        /// VERSION 1: basic raw
        /// </summary>
        internal static IObservable<double> SMA_V1(this IObservable<double> source, int period) {
            return source.Buffer(period, 1).Where(x => x.Count == period).Select(x => x.Sum() / period);
        }
        /// <summary>
        /// VERSION 2:  Using RollingWindow ( Better Performace)
        /// </summary>
        internal static IObservable<double> SMA_V2(this IObservable<double> source, uint period) {
            return new SMA_V2(source, period);
        }
        /// <summary>
        /// VERSION 3:  Using RingWnd( Performance and Roll Adjustments)
        /// Performance: Avoid Rollingwindow create Tuples.
        /// </summary>
        internal static IObservable<double> SMA_V3(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return new SMA_V3(source, period, offset);
        }
        /// <summary>
        /// based on Running Total
        /// </summary>
        internal static IObservable<double> SMA_V4(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return source.SUM(period, offset).Select(val => val / period);
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
        public static IObservable<double> SMA(this IObservable<double> source, uint period, IObservable<double> offset = null) {
//            return source.SMA_V2(period);
            return source.SMA_V3(period, offset);
        }
        /// <summary>
        /// Tick based SMA . Takes care of adjustments for futures roll / continuous pricing
        /// </summary>
        public static IObservable<double> SMA(this IObservable<Tick> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Price).SMA(period, sr.Offset());
            });
        }
        /// <summary>
        /// OHLC based SMA of Close Price.
        /// TODO: Extend to choose Open High, Low. 
        /// </summary>
        public static IObservable<double> SMA(this IObservable<OHLC> source, uint period) {
            return source.Publish(sr => {
                return sr.Select(x => (double)x.Close.Price).SMA(period, sr.Offset());
            });
        }
    }
}
