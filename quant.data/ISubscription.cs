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
        public static IEnumerable<(string, DateTime)> Query(string prdt) {
            string sql = $@" SELECT endtime, symbol FROM boundaries 
                    WHERE productgroup = '{prdt}' AND generic = 1 AND rolltime_eastern = '12:30:00' 
                    AND starttime <= '{DateTime.Now}'::TIMESTAMP AT TIME ZONE 'America/New_York' ORDER BY starttime";
            return pgCS.Query(new NpgsqlCommand(sql)).Select(rdr => (Convert.ToString(rdr[1]), (DateTime)rdr[0]));
        }
        protected abstract IEnumerable<Tick> Query(DateTime dt);
        public IObservable<Tick> Query(DateTime dtFrom, DateTime dtTo) {
            return Enumerable.Range(0, (dtTo - dtFrom).Days).Select(t => dtFrom.AddDays(t))
                .AsParallel().AsOrdered().WithDegreeOfParallelism(2)
                .Select(dt => Query(dt)).SelectMany(x => x).ToObservable();
        }
    }
}
