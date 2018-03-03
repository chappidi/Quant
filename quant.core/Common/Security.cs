using System;
using System.Collections.Generic;
using System.Text;

namespace quant.common
{
    /// <summary>
    /// aka Instrument which can be traded
    /// </summary>
    public class Security : IEquatable<Security>
    {
        static string codes = "_FGHJKMNQUVXZ";

        #region ctor
        public Security(string sym) { Symbol = sym; }
        #endregion

        #region properties
        public string Symbol { get; }
        public int MonthCode => (10 + Symbol[3] - '0') * 100 + codes.IndexOf(Symbol[2]);
        #endregion

        #region equality
        public static bool operator ==(Security left, Security right) => left.Equals(right);
        public static bool operator !=(Security left, Security right) => !left.Equals(right);
        public bool Equals(Security other) => (other != null && Symbol == other.Symbol);
        #endregion

        #region Object
        public override bool Equals(object obj) { var n = obj as Security; return n != null && this.Equals(n); }
        public override int GetHashCode() => Symbol.GetHashCode();
        public override string ToString() { return Symbol; }
        #endregion
    }
}
