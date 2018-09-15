using System;
using System.Collections.Generic;
using System.Text;

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
        public SMA_V2(IObservable<double> source, uint period)
        {
            _source = source;
            _period = period;
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
            return _source.RollingWindow(_period).Subscribe(val => OnVal(val.Item1, val.Item2, obsvr), obsvr.OnError, obsvr.OnCompleted);
        }
        #endregion
    }

    internal static class SMAV2Ext
    {
        /// <summary>
        /// VERSION 2:  Using RollingWindow ( Better Performace)
        /// </summary>
        internal static IObservable<double> SMA_V2(this IObservable<double> source, uint period)
        {
            return new SMA_V2(source, period);
        }
    }
}
