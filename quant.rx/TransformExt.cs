using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace quant.rx
{
    /// <summary>
    /// https://www.mesasoftware.com/papers/TheInverseFisherTransform.pdf
    /// </summary>
    public static class TransformExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<double> IFish(this IObservable<double> source)
        {
            return Observable.Create<double>(obs => {
                return source.Subscribe((val) => {
                    var ex = Math.Exp(2.0 * val);
                    obs.OnNext((ex-1)/(ex+1));
                }, obs.OnError, obs.OnCompleted);
            });
        }
        public static IObservable<double> Fish(this IObservable<double> source)
        {
            return Observable.Create<double>(obs => {
                return source.Subscribe((val) => {
                    obs.OnNext(0.50 * Math.Log((1 + val)/(1 - val)));
                }, obs.OnError, obs.OnCompleted);
            });
        }
    }
}
