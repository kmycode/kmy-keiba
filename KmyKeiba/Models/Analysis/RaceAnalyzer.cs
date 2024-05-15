using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.Models.Analysis
{
  public class RaceAnalyzer : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private RaceHorseMatchResult? _matchResult;

    /*
     * メモリリーク確認用コード

    #region checkmemoryleak

    private static readonly Dictionary<string, LogDicData> _logdic = new();
    private static long _newCount;
    private static long _disposeCount;
    private string _newStackTrace = string.Empty;
    private bool _isDisposed;
    private class LogDicData
    {
      public long Count { get; set; }
    }
    private static void IncrementLogCount(string stack)
    {
      if (_logdic.TryGetValue(stack, out var data))
      {
        data.Count++;
      }
      else
      {
        _logdic[stack] = new LogDicData { Count = 1, };
      }
    }
    private static void DecrementLogCount(string stack)
    {
      if (_logdic.TryGetValue(stack, out var data))
      {
        data.Count--;
      }
      else
      {
        _logdic[stack] = new LogDicData { Count = -1, };
      }
    }
    private static void IncrementNewCount(RaceAnalyzer self)
    {
      var stack = Environment.StackTrace;
      self._newStackTrace = stack;

      lock (_logdic)
      {
        _newCount++;
        IncrementLogCount(stack);
      }
    }
    private static void IncrementDisposeCount(RaceAnalyzer self)
    {
      if (self._isDisposed)
      {
        return;
      }
      self._isDisposed = true;

      lock (_logdic)
      {
        _disposeCount++;
        DecrementLogCount(self._newStackTrace);
      }
    }
    public static void OutputLogCount()
    {
      System.Diagnostics.Debug.WriteLine($"  COUNT  New: {_newCount} / Dispose: {_disposeCount}");

      lock (_logdic)
      {
        var text = string.Join("\n\n=========================\n\n", _logdic.Select(l => "【" + l.Value.Count + "】\n" + l.Key));
        System.IO.File.WriteAllText("d:\\log.txt", text);
      }
    }

    #endregion

    */

    public RaceData Data { get; }

    public RaceSubjectInfo Subject { get; }

    public bool IsRaceCanceled => this.Data.DataStatus == RaceDataStatus.Canceled;

    public RaceHorseData? TopHorseData => this.TopHorse.Data;

    public RaceHorseAnalyzer TopHorse { get; } = RaceHorseAnalyzer.Empty;

    public IReadOnlyList<RaceHorseAnalyzer> TopHorses { get; }

    public RunningStyle TopRunningStyle { get; }

    public IReadOnlyList<RunningStyle> RunningStyles { get; }

    public double ResultTimeDeviationValue { get; }

    public double A3HResultTimeDeviationValue { get; }

    public double MaxA3HResultTimeDeviationValue { get; }

    public double UntilA3HResultTimeDeviationValue { get; }

    public RacePace Pace { get; }

    public short NormalizedBefore3HaronTime { get; }

    public ReactiveProperty<string> Memo { get; } = new();

    public ReactiveProperty<bool> IsMemoSaving { get; } = new();

    public ReactiveProperty<bool> CanSave => DownloaderModel.Instance.CanSaveOthers;

    public ReactiveCollection<RaceHorseMatchResult> Matches { get; } = new();

    public IReadOnlyList<PrizeMoneyItem> PrizeMoneys { get; }

    public RaceMovieInfo Movie => this._movie ??= new(this.Data);
    private RaceMovieInfo? _movie;

    public RaceAnalyzer(RaceData race, IReadOnlyList<RaceHorseData> topHorses, RaceStandardTimeMasterData raceStandardTime)
    {
      var topHorse = topHorses.OrderBy(h => h.ResultOrder).FirstOrDefault(h => h.ResultOrder == 1) ?? new();

      this.Data = race;
      this.TopHorses = topHorses.Select(h => new RaceHorseAnalyzer(race, h, raceStandardTime)).ToArray();
      this.Subject = new RaceSubjectInfo(race);
      this.RunningStyles = topHorses.OrderBy(h => h.ResultOrder)
        .Take(3)
        .Select(rh => rh.RunningStyle)
        .Where(rs => rs != RunningStyle.Unknown)
        .ToArray();
      this.TopRunningStyle = this.RunningStyles.FirstOrDefault();
      this.NormalizedBefore3HaronTime = AnalysisUtil.NormalizeB3FTime(race);

      this.Memo.Value = race.Memo ?? string.Empty;
      AnalysisUtil.SetMemoEvents(() => this.Data.Memo ?? string.Empty, (db, m) =>
      {
        db.Races!.Attach(this.Data);
        this.Data.Memo = m;
      }, this.Memo, this.IsMemoSaving).AddTo(this._disposables);

      var prizeRaws = this.Data.GetPrizeMoneys();
      var prizes = new List<PrizeMoneyItem>();
      var i = 0;
      for (var place = 1; place <= 5; place++)
      {
        var num = System.Math.Max(1, topHorses.Count(h => h.ResultOrder == place));
        for (var n = 0; n < num; n++)
        {
          if (i >= prizeRaws.Length)
          {
            break;
          }
          prizes.Add(new PrizeMoneyItem
          {
            Place = place,
            PrizeMoney = ValueUtil.ToMoneyLabel((long)prizeRaws[i++] * 100),
          });
        }
      }
      this.PrizeMoneys = prizes;

      if (topHorse != null)
      {
        this.TopHorse = new RaceHorseAnalyzer(race, topHorse, raceStandardTime);

        this.Pace = AnalysisUtil.CalcRacePace(race);
        this.ResultTimeDeviationValue = this.TopHorse.ResultTimeDeviationValue;
        this.A3HResultTimeDeviationValue = this.TopHorse.A3HResultTimeDeviationValue;
        this.UntilA3HResultTimeDeviationValue = this.TopHorse.UntilA3HResultTimeDeviationValue;

        var maxA3HHorse = topHorses.OrderBy(h => h.AfterThirdHalongTime).FirstOrDefault();
        if (maxA3HHorse != null)
        {
          var maxa3h = new RaceHorseAnalyzer(race, maxA3HHorse, raceStandardTime);
          this.MaxA3HResultTimeDeviationValue = maxa3h.A3HResultTimeDeviationValue;
        }
      }
    }

    public void SetMatches(IReadOnlyList<RaceHorseAnalyzer> sameRaceHorses)
    {
      if (sameRaceHorses.Count < 2)
      {
        return;
      }

      foreach (var raceData in sameRaceHorses
        .SelectMany(h => h.History?.BeforeRaces ?? Enumerable.Empty<RaceHorseAnalyzer>())
        .Where(h => h.Race.DataStatus != RaceDataStatus.Canceled && !h.IsAbnormalResult)
        .GroupBy(history => history.Race.Key)
        .Where(h => h.ElementAtOrDefault(1) != null)
        .OrderByDescending(h => h.Key)
        .Take(30))
      {
        var match = new RaceHorseMatchResult(raceData.First().Race);
        match.RaceAnalyzer._matchResult = match;
        foreach (var horse in sameRaceHorses.OrderBy(h => h.Data.Number))
        {
          var history = raceData.FirstOrDefault(h => h.Data.Key == horse.Data.Key);
          match.Rows.Add(new RaceHorseMatchResult.Row
          {
            RaceHorse = history,
          });
        }
        this.Matches.Add(match);
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.TopHorse.Dispose();
      foreach (var h in this.TopHorses)
      {
        h.Dispose();
      }
      foreach (var m in this.Matches)
      {
        m.Dispose();
      }
    }

    #region Commands

    public ICommand PlayRaceMovieCommand =>
      this._playRaceMovieCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsRaceError.Select(e => !e)).WithSubscribe(async _ => await this.Movie.PlayRaceAsync());
    private AsyncReactiveCommand<object>? _playRaceMovieCommand;

    public ICommand PlayPaddockCommand =>
      this._playPaddockCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPaddockError.Select(e => !e)).WithSubscribe(async _ => await this.Movie.PlayPaddockAsync());
    private AsyncReactiveCommand<object>? _playPaddockCommand;

    public ICommand PlayPaddockForceCommand =>
      this._playPaddockForceCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPaddockForceError.Select(e => !e)).WithSubscribe(async _ => await this.Movie.PlayPaddockForceAsync());
    private AsyncReactiveCommand<object>? _playPaddockForceCommand;

    public ICommand PlayPatrolCommand =>
      this._playPatrolCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPatrolError.Select(e => !e)).WithSubscribe(async _ => await this.Movie.PlayPatrolAsync());
    private AsyncReactiveCommand<object>? _playPatrolCommand;

    public ICommand PlayMultiCamerasCommand =>
      this._playMultiCamerasCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsMultiCamerasError.Select(e => !e)).WithSubscribe(async _ => await this.Movie.PlayMultiCamerasAsync());
    private AsyncReactiveCommand<object>? _playMultiCamerasCommand;

    public ICommand OpenRaceWindowCommand =>
      this._openRaceWindowCommand ??=
        new ReactiveCommand<string>().WithSubscribe(key =>
        {
          if (this._matchResult != null)
          {
            OpenRaceRequest.Default.Request(key, this._matchResult.Rows
              .Where(r => r.RaceHorse != null).Select(r => r.RaceHorse!.Data.Key).ToArray());
          }
          else
          {
            OpenRaceRequest.Default.Request(key);
          }
        });
    private ReactiveCommand<string>? _openRaceWindowCommand;

    #endregion
  }

  public class RaceHorseMatchResult : IDisposable
  {
    public RaceData Race => this.RaceAnalyzer.Data;

    public RaceAnalyzer RaceAnalyzer { get; }

    public RaceSubjectInfo Subject { get; }

    public ReactiveCollection<Row> Rows { get; } = new();

    public RaceHorseMatchResult(RaceData race)
    {
      this.RaceAnalyzer = new RaceAnalyzer(race, Array.Empty<RaceHorseData>(), AnalysisUtil.DefaultStandardTime);
      this.Subject = new RaceSubjectInfo(race);
    }

    public void Dispose()
    {
      this.RaceAnalyzer.Dispose();
    }

    public class Row
    {
      public bool HasResult => this.RaceHorse != null;

      public RaceHorseAnalyzer? RaceHorse { get; init; }
    }
  }

  public class PrizeMoneyItem
  {
    public int Place { get; init; }

    public string PrizeMoney { get; init; } = string.Empty;
  }

  public enum RacePace
  {
    [Label("不明", "?")]
    Unknown,

    [Label("とても速い", "VH")]
    VeryHigh,

    [Label("速い", "H")]
    High,

    [Label("標準", "M")]
    Standard,

    [Label("遅い", "S")]
    Low,

    [Label("とても遅い", "VS")]
    VeryLow,
  }
}
