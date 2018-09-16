using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// Basic Implementation
    /// </summary>
    internal static partial class LWMAV1Ext
    {
        internal static IObservable<double> LWMA_V1(this IObservable<double> source, int period)
        {
            uint weight = 0;
            for (uint i = 1; i <= period; ++i) {
                weight += i;
            }

            return source.Buffer(period, 1).Where(x => x.Count == period).Select(x => {
                double total = 0;
                for (int i = 0; i < period; i++) {
                    total += x[i] * (i + 1);
                }
                return total / weight;
            });
        }
    }
}
