using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Math
{
  public class StatisticDoubleArray
  {
    private readonly StatisticSingleArray _valuesA;
    private readonly StatisticSingleArray _valuesB;

    public static StatisticDoubleArray Empty { get; }
      = new StatisticDoubleArray(new StatisticSingleArray(), new StatisticSingleArray());

    /// <summary>
    /// 共分散
    /// </summary>
    public double Covariance
    {
      get
      {
        if (this._covariance == null)
        {
          var sum = 0.0;
          var count = 0;
          foreach (var vals in this._valuesA.Values.Zip(this._valuesB.Values))
          {
            sum += (vals.First - this._valuesA.Average) * (vals.Second - this._valuesB.Average);
            count++;
          }
          this._covariance = sum / count;
        }
        return this._covariance.Value;
      }
    }
    private double? _covariance;

    /// <summary>
    /// 相関係数
    /// </summary>
    public double CorrelationCoefficient
    {
      get
      {
        if (this._correlationCoefficient == null)
        {
          this._correlationCoefficient = this.Covariance / (this._valuesA.Deviation * this._valuesB.Deviation);
        }
        return this._correlationCoefficient.Value;
      }
    }
    private double? _correlationCoefficient;

    /// <summary>
    /// 回帰直線の傾き
    /// </summary>
    public double Regressionline
    {
      get
      {
        if (this._regressionline == null)
        {
          this._regressionline = this.CorrelationCoefficient / this._valuesA.Variance;
        }
        return this._regressionline.Value;
      }
    }
    private double? _regressionline;

    /// <summary>
    /// 回帰直線の切片
    /// </summary>
    public double RegressionlineIntercept
    {
      get
      {
        if (this._regressionlineIntercept == null)
        {
          this._regressionlineIntercept = this._valuesB.Average - this.Regressionline * this._valuesA.Average;
        }
        return this._regressionlineIntercept.Value;
      }
    }
    private double? _regressionlineIntercept;

    public StatisticDoubleArray(StatisticSingleArray valuesA, StatisticSingleArray valuesB)
    {
      this._valuesA = valuesA;
      this._valuesB = valuesB;
    }

    /// <summary>
    /// 回帰直線上の値を計算する
    /// </summary>
    /// <param name="valueA">計算したい値</param>
    /// <returns>計算された値</returns>
    public double CalcRegressionValue(double valueA)
    {
      return this.Regressionline * valueA + this.RegressionlineIntercept;
    }
  }
}
