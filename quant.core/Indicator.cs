using System;
using System.Collections.Generic;
using System.Text;

namespace quant.core
{
    public interface IIndicator<T>
    {
        double Calc(T input);
    }
}
