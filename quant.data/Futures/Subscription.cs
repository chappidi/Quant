using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Npgsql;
using quant.core;

namespace quant.data.futures
{
    /// <summary>
    /// Futures Stitch Raw Contracts with Roll
    /// </summary>
    internal class Subscription : ISubscription
    {
        public string Product { get; }
        public Subscription(string prdt) { Product = prdt; }
        protected override IEnumerable<Tick> Query(DateTime dt) {
            double factor = factors[Product];
            string sql = $@"
                SELECT exch_time, qty, side, symbol, px FROM devexperts_{Product}   
                WHERE exch_time > '{dt}'::TIMESTAMP AT TIME ZONE 'America/New_York' 
                AND exch_time < '{dt.AddDays(1)}'::TIMESTAMP AT TIME ZONE 'America/New_York' 
                ORDER BY exch_time, seqno ";

            return pgCS.Query(new NpgsqlCommand(sql)).Select(rdr => {
                var sec = Security.Lookup(Convert.ToString(rdr[3]));
                double px = Convert.ToDouble(rdr[4]) * factor;
                return new Tick(sec, Convert.ToUInt32(rdr[1]), (uint)Math.Round(px), (DateTime)rdr[0]) {
                    Side = (Aggressor)Convert.ToUInt32(rdr[2])
                };
            });
        }
    }
}
