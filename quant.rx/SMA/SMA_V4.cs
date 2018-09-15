using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    internal static class SMAV4Ext
    {
        /// <summary>
        /// based on Running Total 
        /// </summary>
        internal static IObservable<double> SMA_V4(this IObservable<double> source, uint period, IObservable<double> offset = null)
        {
            return source.SUM(period, offset).Select(val => val / period);
        }
    }
}
