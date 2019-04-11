using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using quant.core;

/// <summary>
/// https://www.metatrader5.com/en/terminal/help/indicators/trend_indicators/fama
/// http://etfhq.com/blog/2010/09/30/fractal-adaptive-moving-average-frama/#How
/// 
/// Fractal Adaptive Moving Average 
/// The advantage of FRAMA is the possibility to follow strong trend movements and 
///     to sufficiently slow down at the moments of price consolidation
///     
/// FRAMA(i) = A(i) * Price(i) + (1 - A(i)) * FRAMA(i-1)
///          = FRAMA(i-1) + A(i) * (Price(i) - FRAMA(i-1))
///     FRAMA(i) — current value of FRAMA
///     Price(i) — current close price
///     FRAMA(i-1) — previous value of FRAMA
///     A(i) — current factor of exponential smoothing.
///     
/// A(i) = EXP(-4.6 * (D(i) - 1))
///     D(i) — current fractal dimension
///     D(i) = (Log(N1 + N2) - Log(N3)) / Log(2.0);
///
/// N(Length,i) = (HighestPrice(i) - LowestPrice(i))/Length
///     HighestPrice(i) — current maximal value for Length periods;
///     LowestPrice(i) — current minimal value for Length periods;
///     
///     Values N1, N2 and N3 are respectively equal to:
///     N1(i) = N(Length, i)
///     N2(i) = N(Length, i + Length)
///     N3(i) = N(2 * Length, i)
/// </summary>
namespace quant.rx
{
    class FRAMA : IObservable<double>
    {
        readonly IObservable<double> _source;
        readonly uint _period;
        double m_frama = 0;
        #region ctor
        public FRAMA(IObservable<double> source, uint period, IObservable<double> offset)
        {
            _source = source;
            _period = period;
        }
        #endregion
        void OnVal(double val, IObserver<double> obsvr)
        {
            //double N1 = (Hi1 - Lo1) / _period;
            //double N2 = (Hi2 - Lo2) / _period;
            //double N3 = (Hi3 - Lo3) / (2 * _period);

            //double D = (Math.Log(N1 + N2) - Math.Log(N3)) / Math.Log(2.0);
            //double alpha = Math.Exp(-4.6 * (D - 1.0));
            //m_frama = alpha * val + (1 - alpha) * m_frama;
        }
        #region IObservable
        public IDisposable Subscribe(IObserver<double> obsvr)
        {
            var ret = new CompositeDisposable();
            return ret;
        }
        #endregion
    }

    public static partial class QuantExt
    {
        /// <summary>
        /// Fractional Dimension
        /// </summary>
        /// <param name="source"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        internal static IObservable<double> FD(this IObservable<OHLC> source, uint period)
        {
            uint pd1 = period / 2;
            uint pd2 = period / 2;
            return source.Publish(sc => {
                var HL1 = sc.Select(x => x.High).Max(pd1).Zip(sc.Select(x => x.Low).Min(pd1), (max, min) => (max - min) / pd1);
                var HL2 = sc.Select(x => x.High).Max(pd2).Zip(sc.Select(x => x.Low).Min(pd2), (max, min) => (max - min) / pd2).Skip((int)pd2);
                var HL = sc.Select(x => x.High).Max(period).Zip(sc.Select(x => x.Low).Min(period), (max, min) => (max - min) / period);
                return Observable.When(HL.And(HL1).And(HL2).Then((N, N1, N2) => (Math.Log(N1 + N2) - Math.Log(N)) / Math.Log(2.0)));
            });
        }
        public static IObservable<double> FRAMA(this IObservable<OHLC> source, uint period)
        {
            return source.Publish(sc => {
                return Observable.Create<double>(obs => {
                    var ret = new CompositeDisposable();
                    double _prev = 0;
                    double _alpha = 1;
                    ret.Add(sc.FD(period).Select(fd => Math.Exp(-4.6 * (fd - 1))).Subscribe(alp => {
                        _alpha = alp;
                    }));
                    ret.Add(sc.Offset().Subscribe(ofst => {
                        if(ofst != 0)
                            Trace.WriteLine($"OFFSET:{ofst}");
                    }));
                    ret.Add(sc.Subscribe(oh => {
                        _prev = _prev + _alpha * (oh.Close.Price - _prev);
                        obs.OnNext(_prev);
                    }));
                    return ret;
                }).Skip((int)period-1);
            });
        }
    }
}
