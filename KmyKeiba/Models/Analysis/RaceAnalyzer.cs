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

    public RaceData Data { get; }

    public RaceSubjectInfo Subject { get; }

    public RaceHorseData? TopHorseData => this.TopHorse.Data;

    public RaceHorseAnalyzer TopHorse { get; } = RaceHorseAnalyzer.Empty;

    public IReadOnlyList<RaceHorseAnalyzer> TopHorses { get; }

    public RunningStyle TopRunningStyle { get; }

    public IReadOnlyList<RunningStyle> RunningStyles { get; }

    /// <summary>
    /// 荒れ度
    /// </summary>
    public double RoughRate { get; }

    public double ResultTimeDeviationValue { get; }

    public double A3HResultTimeDeviationValue { get; }

    public double UntilA3HResultTimeDeviationValue { get; }

    public RacePace Pace { get; }

    public RacePace A3HPace { get; }

    public ReactiveProperty<string> Memo { get; } = new();

    public ReactiveProperty<bool> IsMemoSaving { get; } = new();

    public ReactiveCollection<RaceHorseMatchResult> Matches { get; } = new();

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

      this.Memo.Value = race.Memo ?? string.Empty;
      this.Memo.Skip(1).Subscribe(async m =>
      {
        if (this.IsMemoSaving.Value)
        {
          return;
        }

        this.IsMemoSaving.Value = true;

        using var db = new MyContext();
        db.Races!.Attach(this.Data);
        this.Data.Memo = m;

        await db.SaveChangesAsync();

        this.IsMemoSaving.Value = false;
      }).AddTo(this._disposables);

      this.RoughRate = AnalysisUtil.CalcRoughRate(topHorses);

      if (topHorse != null)
      {
        this.TopHorse = new RaceHorseAnalyzer(race, topHorse, raceStandardTime);

        this.Pace = this.TopHorse.ResultTimeDeviationValue < 38 ? RacePace.VeryLow :
          this.TopHorse.ResultTimeDeviationValue < 45 ? RacePace.Low :
          this.TopHorse.ResultTimeDeviationValue < 55 ? RacePace.Standard :
          this.TopHorse.ResultTimeDeviationValue < 62 ? RacePace.High : RacePace.VeryHigh;
        this.A3HPace = this.TopHorse.A3HResultTimeDeviationValue < 38 ? RacePace.VeryLow :
          this.TopHorse.A3HResultTimeDeviationValue < 45 ? RacePace.Low :
          this.TopHorse.A3HResultTimeDeviationValue < 55 ? RacePace.Standard :
          this.TopHorse.A3HResultTimeDeviationValue < 62 ? RacePace.High : RacePace.VeryHigh;
        this.ResultTimeDeviationValue = this.TopHorse.ResultTimeDeviationValue;
        this.A3HResultTimeDeviationValue = this.TopHorse.A3HResultTimeDeviationValue;
        this.UntilA3HResultTimeDeviationValue = this.TopHorse.UntilA3HResultTimeDeviationValue;
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
        .GroupBy(history => history.Race.Key)
        .Where(h => h.ElementAtOrDefault(1) != null)
        .Take(20))
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
    }

    #region Commands

    public ICommand PlayRaceMovieCommand =>
      this._playRaceMovieCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsRaceError.Select(e => !e).CombineLatest(DownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)).WithSubscribe(async _ => await this.Movie.PlayRaceAsync());
    private AsyncReactiveCommand<object>? _playRaceMovieCommand;

    public ICommand PlayPaddockCommand =>
      this._playPaddockCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPaddockError.Select(e => !e).CombineLatest(DownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)).WithSubscribe(async _ => await this.Movie.PlayPaddockAsync());
    private AsyncReactiveCommand<object>? _playPaddockCommand;

    public ICommand PlayPaddockForceCommand =>
      this._playPaddockForceCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPaddockForceError.Select(e => !e).CombineLatest(DownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)).WithSubscribe(async _ => await this.Movie.PlayPaddockForceAsync());
    private AsyncReactiveCommand<object>? _playPaddockForceCommand;

    public ICommand PlayPatrolCommand =>
      this._playPatrolCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPatrolError.Select(e => !e).CombineLatest(DownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)).WithSubscribe(async _ => await this.Movie.PlayPatrolAsync());
    private AsyncReactiveCommand<object>? _playPatrolCommand;

    public ICommand PlayMultiCamerasCommand =>
      this._playMultiCamerasCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsMultiCamerasError.Select(e => !e).CombineLatest(DownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)).WithSubscribe(async _ => await this.Movie.PlayMultiCamerasAsync());
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

  public class RaceHorseMatchResult
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

    public class Row
    {
      public bool HasResult => this.RaceHorse != null;

      public RaceHorseAnalyzer? RaceHorse { get; init; }
    }
  }

  public enum RacePace
  {
    [Label("とても速い")]
    VeryHigh,

    [Label("速い")]
    High,

    [Label("標準")]
    Standard,

    [Label("遅い")]
    Low,

    [Label("とても遅い")]
    VeryLow,
  }
}
