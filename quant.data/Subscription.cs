using Npgsql;
using quant.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace quant.data
{
    public enum Resolution { Tick, Second, Minute, Hour, Daily }
    internal static class PostgresExt
    {
        internal static IEnumerable<NpgsqlDataReader> Query(this NpgsqlConnectionStringBuilder pgCS, NpgsqlCommand cmd)
        {
            using (var conn = new NpgsqlConnection(pgCS.ConnectionString)) {
                conn.Open();
                cmd.Connection = conn;
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    yield return rdr;
                conn.Close();
            }
        }
    }
    public class Subscription
    {
        static NpgsqlConnectionStringBuilder pgCS = new NpgsqlConnectionStringBuilder {
        };
        static Dictionary<string, double> factors = new Dictionary<string, double>() {
                { "GC", 10},
                { "ZS", 100},
                { "ZC", 100},
                { "ES", 100},
                { "NQ", 100},
                { "CL", 100},
                { "SI", 1000},
                { "HG", 10000},
                { "6B", 10000}
        };
        public string Product { get; }
        public Subscription(string prdt, Resolution res) {
            Product = prdt;
        }
        internal IEnumerable<Tick> Query(DateTime dt)
        {
            double factor = factors[Product];
            string sql = $@"
                SELECT exch_time, qty, side, symbol, px FROM devexperts_{Product}   
                WHERE exch_time > '{dt}'::TIMESTAMP AT TIME ZONE 'America/New_York' 
                AND exch_time < '{dt.AddDays(1)}'::TIMESTAMP AT TIME ZONE 'America/New_York' 
                ORDER BY exch_time, seqno ";

            return pgCS.Query(new NpgsqlCommand(sql)).Select(rdr => {
                var sec = Security.Lookup(Convert.ToString(rdr[3]));
                double px = Convert.ToDouble(rdr[4]) * factor;
                return new Tick(sec, Convert.ToUInt32(rdr[1]), (uint)Math.Round(px), (DateTime)rdr[0])
                {
                    Side = (Aggressor)Convert.ToUInt32(rdr[2])
                };
            });
        }
        /// <summary>
        public IEnumerable<Tick> Query(DateTime dtFrom, DateTime dtTo) {
            return Enumerable.Range(0, (dtTo - dtFrom).Days).Select(t => dtFrom.AddDays(t))
                .AsParallel().AsOrdered().WithDegreeOfParallelism(2)
                .Select(dt => Query(dt)).SelectMany(x => x);
        }
    }
}
