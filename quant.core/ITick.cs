using System;
using System.Collections.Generic;
using System.Text;

namespace quant.core
{
    public interface ITick
    {
        uint Quantity       { get; }
        uint Price          { get; }
        DateTime TradedAt   { get; }
    }
}
