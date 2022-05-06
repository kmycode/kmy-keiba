using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  /// <summary>
  /// 条件にあったレース傾向解析を選択
  /// </summary>
  public class RaceTrendAnalysisSelector
  {
    public RaceData Race { get; }

    private readonly Dictionary<AnalysisDataKey, RaceTrendAnalyzer> _analyzers = new();

    public record struct AnalysisDataKey(
      bool IsSameCourse, bool IsSameCondition, bool IsSameRaceName, bool IsSameSubject,
      bool IsSameTime, bool IsSameSeason);

    public RaceTrendAnalysisSelector(RaceData race)
    {
      this.Race = race;
    }

    public RaceTrendAnalyzer GetAnalyzerAndBeginAnalysis(MyContext db, AnalysisDataKey key)
    {
      this._analyzers.TryGetValue(key, out var existsAnalyzer);
      if (existsAnalyzer != null)
      {
        return existsAnalyzer;
      }

      var query = db.Races!.Where(r => r.StartTime < this.Race.StartTime);

      if (key.IsSameCourse)
      {
        query = query.Where(r => r.Course == this.Race.Course);
      }
      if (key.IsSameSeason)
      {
        query = this.Race.StartTime.Month switch
        {
          var mon when mon <= 2 || mon == 12 => query.Where(r => r.StartTime.Month <= 2 || r.StartTime.Month == 12),
          var mon when mon >= 3 && mon <= 5 => query.Where(r => r.StartTime.Month >= 3 && r.StartTime.Month <= 5),
          var mon when mon >= 6 && mon <= 8 => query.Where(r => r.StartTime.Month >= 6 && r.StartTime.Month <= 8),
          var mon when mon >= 9 && mon <= 11 => query.Where(r => r.StartTime.Month >= 9 && r.StartTime.Month <= 11),
          _ => query,
        };
      }
      if (key.IsSameCondition)
      {
        query = query.Where(r => r.TrackCondition == this.Race.TrackCondition);
      }
      if (key.IsSameRaceName && !string.IsNullOrWhiteSpace(this.Race.Name))
      {
        query = query.Where(r => r.Name == this.Race.Name);
      }
      if (key.IsSameSubject)
      {
        query = query.Where(r => r.SubjectName == this.Race.SubjectName);
      }
      if (key.IsSameTime)
      {
        query = query.Where(r => r.StartTime.Hour == this.Race.StartTime.Hour);
      }

      var analyzer = new RaceTrendAnalyzer(this.Race);
      this._analyzers[key] = analyzer;

      // 非同期でレースの読み込み・解析を開始
      var task = query.OrderByDescending(r => r.StartTime).Take(200).ToArrayAsync();
      task.ConfigureAwait(false);
      task.ContinueWith(t => analyzer.SetRaces(t.Result));

      return analyzer;
    }
  }

  public class RaceTrendAnalysisOperator
  {
    public ReactiveProperty<bool> IsSameCourse { get; } = new();

    public ReactiveProperty<bool> IsSameCondition { get; } = new();

    public ReactiveProperty<bool> IsSameRaceName { get; } = new();

    public ReactiveProperty<bool> IsSameSubject { get; } = new();

    public ReactiveProperty<bool> IsSameTime { get; } = new();

    public ReactiveProperty<bool> IsSameSeason { get; } = new();

    public ReactiveProperty<RaceTrendAnalyzer> Analyzer { get; } = new();

    private readonly RaceTrendAnalysisSelector _selector;

    public RaceTrendAnalysisOperator(RaceData race)
    {
      this._selector = new RaceTrendAnalysisSelector(race);
    }

    public void BeginLoad(MyContext db)
    {
      this.Analyzer.Value = this._selector.GetAnalyzerAndBeginAnalysis(db, new RaceTrendAnalysisSelector.AnalysisDataKey
      {
        IsSameCondition = this.IsSameCondition.Value,
        IsSameCourse = this.IsSameCourse.Value,
        IsSameRaceName = this.IsSameRaceName.Value,
        IsSameSeason = this.IsSameSeason.Value,
        IsSameSubject = this.IsSameSubject.Value,
        IsSameTime = this.IsSameTime.Value,
      });
    }
  }
}
