using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Npgsql;
using quant.core;

namespace quant.data.equity
{
    internal class Subscription : ISubscription
    {
        public string Product { get; }
        public Subscription(string prdt) { Product = prdt; }
        protected override IEnumerable<Tick> Query(DateTime dt)
        {
            var sec = Security.Lookup("DMK3");
            double factor = factors[Product];
            string sql = $@"
                SELECT  p.exch_time, p.qty, p.side, p.symbol, (p.px + b.cumdollarpxadjustment) as px
                FROM devexperts_{Product} p, boundaries b  WHERE p.productgroup = b.productgroup AND p.symbol = b.symbol 
                AND b.rolltime_eastern = '12:30:00' AND b.generic = 1 AND p.exch_time between b.starttime AND b.endtime 
                AND exch_time > '{dt}'::TIMESTAMP AT TIME ZONE 'America/New_York'
                AND exch_time < '{dt.AddDays(1)}'::TIMESTAMP AT TIME ZONE 'America/New_York' 
                ORDER BY exch_time,seqno ";

            return pgCS.Query(new NpgsqlCommand(sql)).Select(rdr => {
                double px = Convert.ToDouble(rdr[4]) * factor;
                return new Tick(sec, Convert.ToUInt32(rdr[1]), (uint)Math.Round(px), (DateTime)rdr[0]) {
                    Side = (Aggressor)Convert.ToUInt32(rdr[2])
                };
            });
        }
    }
}
