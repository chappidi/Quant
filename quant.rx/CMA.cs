using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

/// <summary>
/// Cumulative Moving Average
///  the average of all of the data up until the current datum point
///  http://en.wikipedia.org/wiki/Moving_average#Cumulative_moving_average
/// </summary>
namespace quant.rx
{
    class CMA : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly IObservable<double> _offset;
        #region ctor
        public CMA(IObservable<double> source, IObservable<double> offset = null)
        {
            _source = source;
            _offset = offset;
        }
        #endregion
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr)
        {
            var ret = new CompositeDisposable();
            return ret;
        }
        #endregion
    }
}
