using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RiderAnalysisData
  {
    public IReadOnlyList<RaceHorseAnalysisData> Source { get; }

    public ResultOrderGradeMap AllGrade { get; }

    public ResultOrderGradeMap SameHorse { get; }

    public RiderAnalysisData(IReadOnlyList<RaceHorseAnalysisData> source)
    {
      this.Source = source;
      this.AllGrade = new ResultOrderGradeMap(source.Select(s => s.Data).ToArray());
    }

    public RiderAnalysisData(IReadOnlyList<RaceHorseAnalysisData> source, RaceHorseAnalysisData self)
      : this(source)
    {
      this.SameHorse = new ResultOrderGradeMap(source
        .Where(s => s.Data.Key == self.Data.Key).Select(s => s.Data).ToArray());
    }
  }

  public struct ResultOrderGradeMap
  {
    public int FirstCount { get; }

    public int SecondCount { get; }

    public int ThirdCount { get; }

    public int LoseCount { get; }

    public int AllCount => this.FirstCount + this.SecondCount + this.ThirdCount + this.LoseCount;

    public bool IsZero => this.AllCount == 0;

    /// <summary>
    /// 複勝勝率
    /// </summary>
    public float PlacingBetsRate { get; }

    public ResultOrderGradeMap(IReadOnlyList<RaceHorseData> source)
    {
      this.FirstCount = source.Count(f => f.ResultOrder == 1);
      this.SecondCount = source.Count(f => f.ResultOrder == 2);
      this.ThirdCount = source.Count(f => f.ResultOrder == 3);
      this.LoseCount = source.Count(f => f.ResultOrder > 3);

      this.PlacingBetsRate = (this.FirstCount + this.SecondCount + this.ThirdCount) / (float)source.Count;
    }
  }
}
