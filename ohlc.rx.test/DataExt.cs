using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using quant.core;

namespace ohlc.rx.test
{
    public static class DataExt
    {
        static IEnumerable<string> ReadAllLines(this StreamReader rdr)
        {
            string line;
            while ((line = rdr.ReadLine()) != null)
            {
                yield return line;
            }
        }
        public static IObservable<(OHLC raw, int ofst, OHLC cnt)> OHLC(this StreamReader rdr)
        {
            // bar_end, symbol, qty, open, high, low, close
            var dict = new Dictionary<string, int>();
            OHLC prev = null;
            var src = rdr.ReadAllLines().Skip(1).Select(ln => {
                var rec = ln.Split(',');
                var sec = Security.Lookup(rec[1]);
                var dt = DateTime.Parse(rec[0]);
                var oh = new OHLC(new Tick(sec, uint.Parse(rec[2]), uint.Parse(rec[3]), dt.AddMinutes(-59)));
                oh.Add(new Tick(sec, 1, uint.Parse(rec[4]), dt.AddMinutes(-40)));
                oh.Add(new Tick(sec, 1, uint.Parse(rec[5]), dt.AddMinutes(-20)));
                oh.Add(new Tick(sec, 1, uint.Parse(rec[6]), dt.AddMinutes(-1)));
                return oh;
            }).Select(oh => {
                var ofst = oh.get_Offset(prev);
                if (ofst != 0)
                    dict.Keys.ToList().ForEach(ky => dict[ky] += ofst);
                if (!dict.ContainsKey(oh.Open.Security.Symbol))
                    dict[oh.Open.Security.Symbol] = 0;
                prev = oh;
                return oh;
            }).ToList();

            return src.ToObservable().Select(raw => {
                var sec = Security.Lookup("DMK3");
                var ofst = dict[raw.Open.Security.Symbol];
                var cnt = new OHLC(new Tick(sec, raw.Open.Quantity, (uint)(raw.Open.Price + ofst), raw.Open.TradedAt));
                cnt.Add(new Tick(sec, 1, (uint)(raw.High.Price + ofst), raw.High.TradedAt));
                cnt.Add(new Tick(sec, 1, (uint)(raw.Low.Price + ofst), raw.Low.TradedAt));
                cnt.Add(new Tick(sec, 1, (uint)(raw.Close.Price + ofst), raw.Close.TradedAt));
                return (raw, ofst, cnt);
            });
        }
    }
}
