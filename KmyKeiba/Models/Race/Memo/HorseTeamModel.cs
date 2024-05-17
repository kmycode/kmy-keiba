using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Memo
{
  public class HorseTeamModel : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private IReadOnlyList<RaceHorseAnalyzer> _raceHorses;
    private IReadOnlyList<RaceHorseMemoItem> _raceHorseMemos;

    public ReactiveCollection<HorseTeamSection> TeamSections { get; } = new();

    public HorseTeamModel(IReadOnlyList<RaceHorseAnalyzer> raceHorses, ReactiveCollection<RaceHorseMemoItem> raceHorseMemos)
    {
      this._raceHorses = raceHorses;
      this._raceHorseMemos = raceHorseMemos;

      void SetRaceMemoItemEvent(RaceMemoItem item2)
      {
        item2.Point.Where(i => item2.IsUseLabel.Value).Subscribe(_ =>
        {
          var section = this.TeamSections.FirstOrDefault(s => s.Config?.Id == item2.Config.Id);
          if (section == null)
          {
            // isUseLabelがTrueなのにセクションがないので作る
            this.OnHorseMemoConfigUpdated(item2.Config.Id);
            return;
          }

          var memo = section.Teams.SelectMany(t => t.Memos).FirstOrDefault(m => m.Memo.Data.Id == item2.Data.Id);
          if (memo == null) return;

          this.OnHorseMemoLabelChanged(memo);
        }).AddTo(this._disposables);
      }

      void SetRaceHorseMemoItemEvent(RaceHorseMemoItem item)
      {
        item.Memos.CollectionChangedAsObservable().Subscribe(ev2 =>
        {
          var oldItems2 = ev2.OldItems?.OfType<RaceMemoItem>() ?? Enumerable.Empty<RaceMemoItem>();
          var newItems2 = ev2.NewItems?.OfType<RaceMemoItem>() ?? Enumerable.Empty<RaceMemoItem>();

          foreach (var item2 in newItems2)
          {
            SetRaceMemoItemEvent(item2);
          }
        }).AddTo(this._disposables);

        foreach (var memo in item.Memos)
        {
          SetRaceMemoItemEvent(memo);
        }
      }

      foreach (var raceHorseMemo in raceHorseMemos)
      {
        SetRaceHorseMemoItemEvent(raceHorseMemo);
      }

      raceHorseMemos.CollectionChangedAsObservable().Subscribe(ev =>
      {
        var oldItems = ev.OldItems?.OfType<RaceHorseMemoItem>() ?? Enumerable.Empty<RaceHorseMemoItem>();
        var newItems = ev.NewItems?.OfType<RaceHorseMemoItem>() ?? Enumerable.Empty<RaceHorseMemoItem>();

        foreach (var item in newItems)
        {
          SetRaceHorseMemoItemEvent(item);
        }
      }).AddTo(this._disposables);

      foreach (var configId in raceHorseMemos.SelectMany(m => m.Memos)
                                             .OrderBy(m => m.Config.Order)
                                             .OrderBy(m => m.Config.MemoGroup)
                                             .Select(m => m.Config.Id)
                                             .Distinct()
                                             .Where(m => !this.TeamSections.Any(s => s.Config?.Id == m)))
      {
        var section = this.GenerateTeamSection(configId);
        if (section == null) continue;

        this.TeamSections.Add(section);
      }
    }

    private HorseTeamSection? GenerateTeamSection(uint configId)
    {
      var memos = this._raceHorseMemos
        .OrderBy(m => m.RaceHorse.Data.Number)
        .Select(m => new { m.RaceHorse, Memo = m.Memos.FirstOrDefault(mm => mm.Config.Id == configId)!, })
        .Where(m => m.Memo != null)
        .ToArray();
      if (memos.Any(m => m.Memo.LabelConfig.Value == null)) return null;

      var labels = memos.FirstOrDefault()?.Memo.LabelConfig.Value?.Items;
      if (labels == null) return null;

      var section = new HorseTeamSection();

      foreach (var label in labels)
      {
        var team = new HorseTeam();
        team.PointLabel.Value = label;

        foreach (var memo in memos.Where(m => m.Memo.Data.Point == label.Data.Point))
        {
          team.Memos.Add(new RaceHorseSingleMemoItem(memo.RaceHorse.Race, memo.RaceHorse, memo.Memo));
        }

        section.Teams.Add(team);
      }

      return section;
    }

    private void ReorderTeamSections()
    {
      var newSections = this.TeamSections.OrderBy(t => t.Config?.Order).OrderBy(t => t.Config?.MemoGroup).ToArray();

      this.TeamSections.Clear();
      foreach (var section in newSections)
      {
        this.TeamSections.Add(section);
      }
    }

    public void OnHorseMemoConfigUpdated(uint configId)
    {
      var trySection = this.GenerateTeamSection(configId);
      var config = trySection?.SampleMemo?.Memo.Config;
      var section = this.TeamSections.FirstOrDefault(s => s.Config?.Id == configId);

      if (trySection != null)
      {
        if (config == null) return;

        if (section == null)
        {
          // 新しいメモ設定が追加された
          var prevItem = this.TeamSections.OrderBy(s => s.Config?.Order)
                                          .OrderBy(s => s.Config?.MemoGroup)
                                          .LastOrDefault(s => s.Config?.MemoGroup < config.MemoGroup || (s.Config?.MemoGroup == config.MemoGroup && s.Config?.Order < config.Order));
          if (prevItem != null)
          {
            var index = this.TeamSections.IndexOf(prevItem) + 1;
            this.TeamSections.Insert(index, trySection);
          }
          else
          {
            this.TeamSections.Add(trySection);
          }
        }
        else
        {
          // 既存のメモの設定が変更された
          this.ReorderTeamSections();
        }
      }
      else
      {
        if (section == null)
        {
          // 対象外（ラベル置換されていないメモ）が設定変更しただけ。何もしない
        }
        else
        {
          // メモが削除された
          this.TeamSections.Remove(section);
        }
      }
    }

    private void OnHorseMemoLabelChanged(RaceHorseSingleMemoItem memo)
    {
      var section = this.TeamSections.FirstOrDefault(t => t.Config?.Id == memo.Memo.Config.Id);
      if (section == null) return;

      var oldTeam = section.Teams.FirstOrDefault(t => t.Memos.Any(m => m.RaceHorse.Data.Id == memo.RaceHorse.Data.Id));
      var newTeam = section.Teams.FirstOrDefault(t => t.PointLabel.Value.Data.Point == memo.Memo.Data.Point);

      void AddToNewTeam()
      {
        var prevItem = newTeam.Memos.LastOrDefault(m => m.RaceHorse.Data.Number < memo.RaceHorse.Data.Number);
        var index = prevItem == null ? 0 : newTeam.Memos.IndexOf(prevItem) + 1;
        newTeam.Memos.Insert(index, memo);
      }

      if (oldTeam == newTeam)
      {
        if (newTeam != null && !newTeam.Memos.Any(m => m.RaceHorse.Data.Id == memo.RaceHorse.Data.Id))
        {
          AddToNewTeam();
        }

        return;
      }

      if (oldTeam != null)
      {
        var oldTeamMemo = oldTeam.Memos.FirstOrDefault(m => m.RaceHorse.Data.Id == memo.RaceHorse.Data.Id);
        if (oldTeamMemo == null) return;

        oldTeam.Memos.Remove(oldTeamMemo);
      }

      if (newTeam == null) return;

      AddToNewTeam();
    }

    public void Dispose() => this._disposables?.Dispose();
  }

  public class HorseTeamSection
  {
    public ReactiveCollection<HorseTeam> Teams { get; } = new();

    public ReactiveProperty<string> Header { get; } = new();

    public RaceHorseSingleMemoItem? SampleMemo => this.Teams.SelectMany(t => t.Memos).FirstOrDefault();

    public ExpansionMemoConfig? Config => this.SampleMemo?.Memo.Config;
  }

  public class HorseTeam
  {
    public ReactiveProperty<PointLabelConfigItem> PointLabel { get; } = new();

    public ReactiveCollection<RaceHorseSingleMemoItem> Memos { get; } = new();
  }
}
