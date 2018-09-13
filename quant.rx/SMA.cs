using quant.common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// 
    /// </summary>
    internal class SMA : IObservable<double>
    {
        readonly RingBuffer<double> _vals;
        readonly IObservable<double> _data;
        readonly IObservable<double> _offset;

        double _total = 0;

        #region ctor
        public SMA(IObservable<double> source, uint wnd, IObservable<double> offset = null) {
            _vals = new RingBuffer<double>(wnd);
            _data = source;
            _offset = offset;
        }
        #endregion
        private void OnVal(double Val, IObserver<double> obsvr) {
            if (_vals.IsFull)
                _total -= _vals.Dequeue();
            _vals.Enqueue(Val);
            _total += Val;
            if (_vals.IsFull)
                obsvr.OnNext(_total / _vals.Size);
        }
        private void OnAdjust(double offset) {
            _vals.Adjust(x => x + offset);
            _total += offset * _vals.Size;
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr) {
            var ret = new CompositeDisposable();
            // offset calculations
            if (_offset != null) {
                ret.Add(_offset.Subscribe(ofst => {
                    OnAdjust(ofst);
                }));
            }
            ret.Add(_data.Subscribe(val => OnVal(val, obsvr), obsvr.OnError, obsvr.OnCompleted));
            return ret;
        }
        #endregion
    }
    public static class SMAExt
    {
        public static IObservable<double> SMA_X(this IObservable<double> source, uint period)
        {
            return new SMA(source, period);
        }
        public static IObservable<double> SMA_X(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(src => {
                var offset = src.Buffer(2, 1).Select(x => (x.Count == 2) ? x[1].get_Offset(x[0]) : 0.0).Where(y => y != 0);
                return new SMA(src.Select(x => (double)x.Close.Price), period, offset);
            });
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static partial class QuantExt
    {
        /// <summary>
        /// Simple Moving Average
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<double> SMA(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            double total = 0;
            double count = 0;   // count of elements
            return Observable.Create<double>(obs => {
                var ret = new CompositeDisposable();
                //offset adjustments has to be first
                if(offset != null) {
                    ret.Add(offset.Subscribe(val => {

                    }, obs.OnError, obs.OnCompleted));
                }
                // data next
                ret.Add(source.RollingWindow(period).Subscribe(val => {
                        // add to the total sum
                        total += val.Item1;
                        // buffer not full
                        if (count < period)
                            count++;
                        else
                            total -= val.Item2;

                        // count matches window size
                        if (count == period)
                            obs.OnNext(total / period);
                    }, obs.OnError, obs.OnCompleted));

                return ret;
            });
        }
    }
}
