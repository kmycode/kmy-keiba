using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMath = System.Math;

namespace KmyKeiba.Models.Analysis.Math
{
  public class StatisticSingleArray
  {
    public double[] Values
    {
      get => this._values;
      set
      {
        if (value.Length == 0)
        {
          this._values = Array.Empty<double>();
        }
        else
        {
          this._values = value;
        }
      }
    }
    private double[] _values = Array.Empty<double>();

    /// <summary>
    /// 平均
    /// </summary>
    public double Average
    {
      get
      {
        if (this._average == null)
        {
          var sum = 0.0;
          foreach (var val in this._values)
          {
            sum += val;
          }
          this._average = sum / this._values.Length;
        }
        return this._average.Value;
      }
    }
    private double? _average;

    /// <summary>
    /// 中央値
    /// </summary>
    public double Median
    {
      get
      {
        if (this._median == null)
        {
          var half = this._values.Length / 2;

          if (this._values.Length % 2 == 0)
          {
            this._median = (this._values[half] + this._values[half - 1]) / 2;
          }
          else
          {
            this._median = this._values[half];
          }
        }
        return this._median.Value;
      }
    }
    private double? _median;

    /// <summary>
    /// 分散
    /// </summary>
    public double Variance
    {
      get
      {
        if (this._variance == null)
        {
          var sum = 0.0;
          var ave = this.Average;
          foreach (var val in this._values)
          {
            sum += (val - ave) * (val - ave);
          }
          this._variance = sum / this._values.Length;
        }
        return this._variance.Value;
      }
    }
    private double? _variance;

    /// <summary>
    /// 標準偏差
    /// </summary>
    public double Deviation
    {
      get
      {
        if (this._deviation == null)
        {
          this._deviation = SMath.Sqrt(this.Variance);
        }
        return this._deviation.Value;
      }
    }
    private double? _deviation;

    /// <summary>
    /// 偏差値を計算する
    /// </summary>
    /// <param name="val">計算したい点数</param>
    /// <returns>偏差値</returns>
    public double CalcDeviationValue(double val)
    {
      return CalcDeviationValue(val, this.Average, this.Deviation);
    }

    public static double CalcDeviationValue(double value, double average, double deviation)
    {
      return (value - average) / deviation * 10 + 50;
    }
  }
}
