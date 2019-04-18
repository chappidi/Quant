using System;
using System.Collections.Generic;
using System.Text;

namespace quant.core
{
    public interface IOHLC<T>
    {
        T Open { get; }
        T Close { get; }
        T High { get; }
        T Low { get; }
        void Add(T val);
    }
}
