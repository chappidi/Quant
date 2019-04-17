using System;
using System.Collections.Generic;
using System.Text;

namespace quant.core.futures
{
    public class F_OHLC : OHLC
    {
        public F_OHLC(Tick tck) :base(tck)
        {

        }
        public override (int hr, int tr, int lr) DM(OHLC prev)
        {
            return base.DM(prev);
        }
        public override long TR(OHLC prev)
        {
            return base.TR(prev);
        }
        public override void Add(Tick tck)
        {
            base.Add(tck);
        }
        public override int get_Offset(Tick old)
        {
            return base.get_Offset(old);
        }
        public override int get_Offset(OHLC old)
        {
            return base.get_Offset(old);
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
