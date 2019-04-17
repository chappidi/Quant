using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using quant.core;

namespace quant.core.futures
{
    /// <summary>
    /// Roll based on dates. capture total volume to identify which contract has high volume and to roll to.
    /// </summary>
    class RollV2 : IObservable<Security>
    {
        readonly IObservable<Tick>      _source;
        readonly IEnumerator<DateTime>  _rollObs;
        Dictionary<Security, uint>      _symVol = new Dictionary<Security, uint>();
        DateTime _dtRoll;
        Security _active;

        #region ctor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dtStart">initial date could fall between predefined roll dates</param>
        /// <param name="roll">pre defined roll dates</param>
        public RollV2(IObservable<Tick> source, DateTime dtStart, IEnumerator<DateTime> roll) {
            _source = source;
            _rollObs = roll;
            _dtRoll = dtStart;
        }
        #endregion

        void OnMsg(Tick tk, IObserver<Security> obsvr)
        {
            if(tk.TradedAt < _dtRoll) {
                // collect stats
                if(tk.Security != _active) {
                    _symVol.TryGetValue(tk.Security, out var curVol);
                    _symVol[tk.Security] = curVol + 1;
                }
            } else {
                // identify the roll
                _active = _symVol.First(x => x.Value == _symVol.Max(i => i.Value)).Key;
                obsvr.OnNext(_active);
                // move to next date
                while (tk.TradedAt > _rollObs.Current) {
                    _rollObs.MoveNext();
                }
                // reset to capture next roll contract
                _dtRoll = _rollObs.Current;
                _symVol = new Dictionary<Security, uint>();
            }
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<Security> obsvr) {
            return _source.Subscribe(tk => OnMsg(tk, obsvr));
        }
        #endregion
    }
}
