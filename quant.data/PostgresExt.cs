using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace quant.data
{
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
}
