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
          this._values = this._ordered = Array.Empty<double>();
        }
        else
        {
          this._values = value;
          this._ordered = null;
        }
      }
    }
    private double[] _values = Array.Empty<double>();

    private double[] Ordered
    {
      get
      {
        if (this._ordered == null)
        {
          this._ordered = this._values.OrderBy(v => v).ToArray();
        }
        return this._ordered;
      }
    }
    private double[]? _ordered;

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

          var wasOrderedNull = this._ordered == null;

          if (this._values.Length % 2 == 0)
          {
            this._median = (this.Ordered[half] + this.Ordered[half - 1]) / 2;
          }
          else
          {
            this._median = this.Ordered[half];
          }

          if (wasOrderedNull)
          {
            this._ordered = null;
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

    public StatisticSingleArray()
    {
    }

    public StatisticSingleArray(double[] values)
    {
      this.Values = values;
    }

    /// <summary>
    /// 偏差値を計算する
    /// </summary>
    /// <param name="val">計算したい点数</param>
    /// <returns>偏差値</returns>
    public double CalcDeviationValue(double val)
    {
      return CalcDeviationValue(val, this.Average, this.Deviation);
    }

    /// <summary>
    /// 配列のインデックスを「０から１」の割合で指定して、値を取得する。値が大きいほど、大きな値となる
    /// </summary>
    /// <param name="pos0to1"></param>
    /// <returns></returns>
    public double GetPositionValue(double pos0to1)
    {
      var index = (int)((this.Ordered.Length - 1) * pos0to1);
      return this.Ordered[index];
    }

    public static double CalcDeviationValue(double value, double average, double deviation)
    {
      return (value - average) / deviation * 10 + 50;
    }
  }
}
