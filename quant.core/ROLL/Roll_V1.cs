using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace quant.core
{
    /// <summary>
    /// identify the contract roll based on minVolume, volume Increment
    /// Also identifies the total volume to current contract volume.
    /// </summary>
    class RollV1 : IObservable<Tuple<Security, double>>
    {
        readonly TimeSpan _start = new TimeSpan(09, 0, 0); //9:00 AM
        readonly TimeSpan _end = new TimeSpan(13, 45, 0); //1:45 PM
        readonly IObservable<IList<OHLC>> _source;
        readonly uint _tgtVol = 0;
        readonly double _factor;
        OHLC _state = null;
        uint confirms = 0;
        #region ctor
        public RollV1(IObservable<IList<OHLC>> source, TimeSpan start, TimeSpan end, double factor) {
            _source = source;
            _factor = factor;
            _start = start;
            _end = end;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tgtVol"></param>
        /// <param name="factor"></param>
        public RollV1(IObservable<IList<OHLC>> source, uint tgtVol, double factor) {
            _source = source;
            _tgtVol = tgtVol;
            _factor = factor;
        }
        #endregion
        void OnMsg(IList<OHLC> lt, IObserver<Tuple<Security, double>> obsvr) {

            double totalVol = lt.Sum(x => x.Volume);
            // find the ohlc with max volume
            var maxV = lt.First(x => x.Volume == lt.Max(i => i.Volume));
            //1. check reset conditions
            if (_tgtVol == 0) {
                // Time based Roll
                if(!(maxV.Open.TradedAt.TimeOfDay > _start && maxV.Open.TradedAt.TimeOfDay < _end) ) {
                    confirms = 0;
                    return;
                }
            } else {
                // atleast 2 contracts to consider roll if target Volume is specified
                // Max(Volume) is less than tgtVol ignore 
                if (lt.Count < 2 || maxV.Volume < _tgtVol) {
                    confirms = 0;
                    // volume too small. It will skew the multiplier
                    return;
                }
            }
            // not first iteration, same symbol resend
            if (_state != null && maxV.Close.Security == _state.Close.Security) {
                confirms = 0;
                obsvr.OnNext(new Tuple<Security, double>(_state.Close.Security, totalVol / maxV.Volume));
                return;
            }

            // first iteration or new symbol with high volume
            var curSec = (_state == null) ? null : _state.Close.Security;
            // find the volume of current symbol
            var newV = lt.FirstOrDefault(x => x.Close.Security == curSec);

            // insufficient incremental jump in volume. resend old symbol
            if (newV != null && maxV.Volume < (newV.Volume * _factor)) {
                obsvr.OnNext(new Tuple<Security, double>(_state.Close.Security, totalVol / newV.Volume));
                confirms = 0;
                return;
            }
            // rolled early. suck it up. resend old symbol.
            if (newV != null && maxV.Close.Security.MonthCode < newV.Close.Security.MonthCode) {
                Trace.WriteLine($"BAD ROLLING ON {maxV} vs {newV}");
                obsvr.OnNext(new Tuple<Security, double>(_state.Close.Security, totalVol / newV.Volume));
                confirms = 0;
                return;
            }
            // finally first iteration or maxV > newVol and maxV.MonthCode > newV.MonthCode
            if (confirms > 0) {
                confirms = 0;
                _state = maxV;
                obsvr.OnNext(new Tuple<Security, double>(_state.Close.Security, totalVol / maxV.Volume));
                Trace.WriteLine($"ROLL DONE {maxV} vs {newV}");
            }
            else {
                confirms++;
            }
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<Tuple<Security, double>> obsvr) {
            return _source.Subscribe(lt => OnMsg(lt, obsvr));
        }
        #endregion
    }
}
