using System;
using System.Collections.Generic;
using System.Text;

namespace quant.rx
{
    public interface IOperator<in T>
    {
        bool Check(T x, T y);
    }

    internal sealed class GreaterThan : IOperator<int>, IOperator<uint>, IOperator<double>
    {
        public bool Check(int x, int y) => (x > y);
        public bool Check(uint x, uint y) => (x > y);
        public bool Check(double x, double y) => (x - y) > 0.0000001;
    }
    internal sealed class LessThan : IOperator<int>, IOperator<uint>, IOperator<double>
    {
        public bool Check(int x, int y) => (x < y);
        public bool Check(uint x, uint y) => (x < y);
        public bool Check(double x, double y) => (x - y) < 0.0000001;
    }


}
