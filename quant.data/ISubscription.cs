using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Npgsql;
using quant.core;

namespace quant.data
{
    /// <summary>
    /// Base Class for Subscription
    /// </summary>
    internal abstract class ISubscription
    {
        internal static NpgsqlConnectionStringBuilder pgCS = new NpgsqlConnectionStringBuilder {
        };
        internal static Dictionary<string, double> factors = new Dictionary<string, double>() {
            { "GC",   10},  { "ZS",   100}, { "ZC",   100},
            { "ES",  100},  { "NQ",   100}, { "CL",   100},
            { "SI", 1000},  { "HG", 10000}, { "6B", 10000}
        };
        protected abstract IEnumerable<Tick> Query(DateTime dt);
        public IObservable<Tick> Query(DateTime dtFrom, DateTime dtTo) {
            return Enumerable.Range(0, (dtTo - dtFrom).Days).Select(t => dtFrom.AddDays(t))
                .AsParallel().AsOrdered().WithDegreeOfParallelism(2)
                .Select(dt => Query(dt)).SelectMany(x => x).ToObservable();
        }
    }
}
