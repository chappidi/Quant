using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using quant.common;

namespace quant.rx
{
    /// <summary>
    /// TOBE DELETED
    /// </summary>
    public class MVWAP_V1 : IObservable<double>
    {
        Dictionary<Security, double> symOff = new Dictionary<Security, double>();
        readonly IObservable<Tick> _source;
        readonly uint WND_SIZE;

        LinkedList<Tick> que = new LinkedList<Tick>();
        double pxVol = 0;
        uint Vol = 0;

        #region ctor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        public MVWAP_V1(IObservable<Tick> source, uint period) {
            _source = source;
            WND_SIZE = period;
        }
        #endregion

        #region properties
        public Tick Seed => que.First.Value;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tk"></param>
        void UpdateOffset(Tick tk)
        {
            // first time or roll of security
            if (que.Count == 0 || que.Last.Value.Security != tk.Security) {
                // find the roll offset
                double offset = (que.Count == 0) ? 0 : (tk.Price - que.Last.Value.Price);
                // adjust the offset history
                foreach (var itm in symOff.ToList()) {
                    symOff[itm.Key] = itm.Value + offset;
                }
                // add the latest symbol
                symOff[tk.Security] = 0;
                // update pxVol as if all the prices of items in que are adjusted
                pxVol += Vol * offset;
            }
        }
        /// <summary>
        /// add at the end of buffer
        /// </summary>
        /// <param name="tk"></param>
        void Add(Tick tk) {
            // add to the end 
            que.AddLast(tk);
            // add to the total sum and volume
            pxVol += tk.PxVol;
            Vol += tk.Quantity;
        }
        /// <summary>
        /// adjust the buffer if the volume exceed the limit
        /// </summary>
        void Adjust()
        {
            while (Vol > WND_SIZE) {
                // remove  old value
                var oldTck = que.First.Value;
                que.RemoveFirst();
                // this required since we did not adjust prices in buffer. but pxVol is adjusted
                var offset = symOff[oldTck.Security];
                // if removed quantity is a lot
                if (oldTck.Quantity + WND_SIZE > Vol) {
                    // find amount to reduce
                    uint diff = Vol - WND_SIZE;
                    // add back the difference
                    que.AddFirst(new Tick(oldTck.Security, oldTck.Quantity - diff, oldTck.Price, oldTck.TradedAt) { Side = oldTck.Side, Live = oldTck.Live });
                    // reduce the aggregate amounts
                    pxVol -= (oldTck.Price + offset) * diff;
                    Vol -= diff;
                }
                else {
                    // reduce the aggregate amounts
                    pxVol -= (oldTck.Price + offset) * oldTck.Quantity;
                    Vol -= oldTck.Quantity;
                }
            }
        }
        /// <summary>
        /// publish if data is ready
        /// </summary>
        /// <param name="obsvr"></param>
        void Publish(IObserver<double> obsvr) {
            Debug.Assert(Vol <= WND_SIZE);
            // publish if count matches window size
            if (Vol >= WND_SIZE)
                obsvr.OnNext(pxVol / WND_SIZE);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tk"></param>
        /// <param name="obsvr"></param>
        void OnMsg(Tick tk) {
            UpdateOffset(tk);
            Add(tk);
            Adjust();
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr) {
            return _source.Subscribe(tk => {
                OnMsg(tk);
                Publish(obsvr);
            });
        }
        #endregion
    }
}
