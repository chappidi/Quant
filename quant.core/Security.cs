using System;
using System.Collections.Generic;
using System.Text;

namespace quant.core
{
    /// <summary>
    /// aka Instrument which can be traded
    /// </summary>
    public class Security : IEquatable<Security>
    {
        #region static
        static object _lck = new object();
        static string codes = "_FGHJKMNQUVXZ";
        static Dictionary<string, Security> _prds = new Dictionary<string, Security>();
        /// <summary>
        /// Lookup by name
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Security Lookup(string sym, uint id=0) {
            _prds.TryGetValue(sym, out Security sec);
            if (sec == null) {
                lock (_lck) {
                    _prds.TryGetValue(sym, out sec);
                    if (sec == null)
                        _prds[sym] = sec = new Security(sym, id);
                }
            }
            return sec;
        }
        #endregion

        #region ctor
        public Security(string sym, uint id = 0) {
            Symbol = sym;
            ID = (id == 0) ? MonthCode : id;
        }

        #endregion
        #region properties
        public string Symbol { get; }
        public uint MonthCode => (uint)((10 + Symbol[3] - '0') * 100 + codes.IndexOf(Symbol[2]));
        public uint ID { get; }
        #endregion

        #region equality
        public static bool operator == (Security left, Security right) {
            if (((object)left) == null || ((object)right) == null)
                return Object.Equals(left, right);
            return left.Equals(right);
        }
        public static bool operator !=(Security left, Security right) {
            if (((object)left) == null || ((object)right) == null)
                return !Object.Equals(left, right);
            return !left.Equals(right);
        }
        public bool Equals(Security other) {
            if (other == null)
                return false;
            return Symbol == other.Symbol;
        }
        #endregion

        #region Object
        public override bool Equals(object obj) { var n = obj as Security; return n != null && this.Equals(n); }
        public override int GetHashCode() => Symbol.GetHashCode();
        public override string ToString() { return Symbol; }
        #endregion
    }
}
