using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceHorseBloodTrendAnalysisSelectorMenu : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private Dictionary<BloodType, string>? _bloodCode;
    private Dictionary<BloodType, string>? _horseKey;

    public RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    public CheckableCollection<MenuItem> MenuItems { get; } = new();

    public ReactiveProperty<RaceHorseBloodTrendAnalysisSelector?> CurrentSelector { get; }

    public bool IsRequestedInitialization => this._bloodCode == null;

    public RaceHorseBloodTrendAnalysisSelectorMenu(RaceData race, RaceHorseData horse)
    {
      this.Race = race;
      this.RaceHorse = horse;
      this.CurrentSelector = this.MenuItems.ActiveItem.Select(i => i?.Selector).ToReactiveProperty().AddTo(this._disposables);
    }

    public void Dispose() => this._disposables.Dispose();

    public async Task InitializeBloodListAsync(MyContext db)
    {
      // 血統リストを作成
      if (this._bloodCode == null)
      {
        async Task<(string?, string?)> GetParentsAsync(string key)
        {
          var myBlood = await db.Horses!.FirstOrDefaultAsync(h => h.Code == key);
          if (myBlood != null)
          {
            var father = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Key == myBlood.FatherBreedingCode);
            var mother = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Key == myBlood.MotherBreedingCode);
            return (father?.Code, mother?.Code);
          }

          var myHorse = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Code == key);
          if (myHorse != null)
          {
            return (myHorse.FatherKey, myHorse.MotherKey);
          }

          return default;
        }

        // 繁殖登録番号
        this._bloodCode = new();

        // 血統登録番号
        this._horseKey = new();

        var horse = await db.Horses!.FirstOrDefaultAsync(h => h.Code == this.RaceHorse.Key);
        if (horse != null)
        {
          this._bloodCode[BloodType.Father] = horse.FatherBreedingCode;
          this._bloodCode[BloodType.Mother] = horse.MotherBreedingCode;
          this._bloodCode[BloodType.FatherFather] = horse.FFBreedingCode;
          this._bloodCode[BloodType.FatherMother] = horse.FMBreedingCode;
          this._bloodCode[BloodType.MotherFather] = horse.MFBreedingCode;
          this._bloodCode[BloodType.MotherMother] = horse.MMBreedingCode;
        }
        else
        {
          var born = await db.BornHorses!.FirstOrDefaultAsync(h => h.Code == this.RaceHorse.Key);
          if (born != null)
          {
            this._bloodCode[BloodType.Father] = born.FatherBreedingCode;
            this._bloodCode[BloodType.Mother] = born.MotherBreedingCode;
            this._bloodCode[BloodType.FatherFather] = born.FFBreedingCode;
            this._bloodCode[BloodType.FatherMother] = born.FMBreedingCode;
            this._bloodCode[BloodType.MotherFather] = born.MFBreedingCode;
            this._bloodCode[BloodType.MotherMother] = born.MMBreedingCode;
          }
          else
          {
            var blood = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Code == this.RaceHorse.Key);
            if (blood != null)
            {
              this._bloodCode[BloodType.Father] = blood.FatherKey;
              this._bloodCode[BloodType.Mother] = blood.MotherKey;
              if (blood.FatherKey != null)
              {
                var (ff, fm) = await GetParentsAsync(blood.FatherKey);
                if (ff != null) this._bloodCode[BloodType.FatherFather] = ff;
                if (fm != null) this._bloodCode[BloodType.FatherMother] = fm;
              }
              if (blood.MotherKey != null)
              {
                var (mf, mm) = await GetParentsAsync(blood.MotherKey);
                if (mf != null) this._bloodCode[BloodType.MotherFather] = mf;
                if (mm != null) this._bloodCode[BloodType.MotherMother] = mm;
              }
            }
          }
        }

        var bloodKeys = this._bloodCode.Select(p => p.Value).ToArray();

        // 血統番号と繁殖番号が結びついてるのはこのテーブルだけ
        var horses = await db.HorseBloods!.Where(h => bloodKeys.Contains(h.Key)).Select(h => new { h.Code, h.Key, h.Name }).ToArrayAsync();
        var items = new List<MenuItem>();
        foreach (var d in this._bloodCode
          .Join(horses, b => b.Value, h => h.Key, (b, h) => new { Type = b.Key, Key = h.Code, h.Name, })
          .Where(h => !string.IsNullOrEmpty(h.Name) && !h.Key.All(c => c == '0')))
        {
          this._horseKey.Add(d.Type, d.Key);
          items.Add(new MenuItem(new RaceHorseBloodTrendAnalysisSelector(this, this.Race, this.RaceHorse, d.Key, d.Name, d.Type))
          {
            Type = d.Type,
          });
        }

        ThreadUtil.InvokeOnUiThread(() =>
        {
          this.MenuItems.AddRangeOnScheduler(items);
          var firstItem = items.FirstOrDefault();
          if (firstItem != null)
          {
            firstItem.IsChecked.Value = true;
          }
        });
      }
    }

    public RaceHorseBloodTrendAnalysisSelector? GetSelector(BloodType type)
    {
      return this.MenuItems.FirstOrDefault(i => i.Type == type)?.Selector;
    }

    public class MenuItem : ICheckableItem
    {
      public ReactiveProperty<bool> IsChecked { get; } = new();

      public BloodType Type { get; init; }

      public RaceHorseBloodTrendAnalysisSelector Selector { get; }

      public MenuItem(RaceHorseBloodTrendAnalysisSelector selector)
      {
        this.Selector = selector;
      }
    }
  }

  public class RaceHorseBloodTrendAnalysisSelector : TrendAnalysisSelector<RaceHorseBloodTrendAnalysisSelector.Key, RaceHorseBloodTrendAnalyzer>
  {
    public enum Key
    {
      [Label("コース")]
      SameCourse,

      [Label("馬場状態")]
      SameCondition,

      [Label("天気")]
      SameWeather,

      [Label("条件")]
      SameSubject,

      [Label("格")]
      SameGrade,

      [Label("季節")]
      SameSeason,

      [Label("距離")]
      NearDistance,
    }

    private Dictionary<string, IReadOnlyList<RaceHorseAnalyzer>>? _horses;
    private readonly string _key;
    private IReadOnlyList<RaceHorseAnalyzer>? _allRaces;

    public override string Name { get; }

    public RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    public RaceHorseBloodTrendAnalysisSelectorMenu Menu { get; }

    public BloodType Type { get; }

    public RaceHorseBloodTrendAnalysisSelector(RaceHorseBloodTrendAnalysisSelectorMenu menu, RaceData race, RaceHorseData horse, string relativeKey, string relativeName, BloodType type) : base(typeof(Key))
    {
      this.Menu = menu;
      this.Race = race;
      this.RaceHorse = horse;
      this._key = relativeKey;
      this.Name = relativeName;
      this.Type = type;
    }

    protected override RaceHorseBloodTrendAnalyzer GenerateAnalyzer()
    {
      return new RaceHorseBloodTrendAnalyzer(this.Race, this.RaceHorse);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceHorseBloodTrendAnalyzer analyzer)
    {
      // WARNING: 全体の総数が多くないと予想されるのでここでDBからすべて取得し、配分している
      //          間違ってもこれをこのまま他のSelectorクラスにコピペしないように
      if (this._allRaces == null)
      {
        var allRaces = await db.RaceHorses!
          .Where(h => h.Key == this._key)
          .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, })
          .Where(d => d.Race.StartTime < this.Race.StartTime)
          .OrderByDescending(d => d.Race.StartTime)
          .ToArrayAsync();
        var list = new List<RaceHorseAnalyzer>();
        foreach (var race in allRaces)
        {
          var standardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race);
          list.Add(new RaceHorseAnalyzer(race.Race, race.RaceHorse, standardTime));
        }
        this._allRaces = list;
      }

      var query = this._allRaces.Where(r => r.Data.Key == this._key);

      if (keys.Contains(Key.SameCourse))
      {
        query = query.Where(r => r.Race.Course == this.Race.Course);
      }
      if (keys.Contains(Key.NearDistance))
      {
        query = query.Where(r => r.Race.Distance / 200 == this.Race.Distance / 200);
      }
      if (keys.Contains(Key.SameSeason))
      {
        query = query.Where(r => r.Race.StartTime.Month % 12 / 3 == this.Race.StartTime.Month % 12 / 3);
      }
      if (keys.Contains(Key.SameCondition))
      {
        query = query.Where(r => r.Race.TrackCondition == this.Race.TrackCondition);
      }
      if (keys.Contains(Key.SameSubject))
      {
        query = query.Where(r => r.Race.SubjectName == this.Race.SubjectName &&
                                 r.Race.SubjectAge2 == this.Race.SubjectAge2 &&
                                 r.Race.SubjectAge3 == this.Race.SubjectAge3 &&
                                 r.Race.SubjectAge4 == this.Race.SubjectAge4 &&
                                 r.Race.SubjectAge5 == this.Race.SubjectAge5 &&
                                 r.Race.SubjectAgeYounger == this.Race.SubjectAgeYounger);
      }
      if (keys.Contains(Key.SameGrade))
      {
        query = query.Where(r => r.Race.Grade == this.Race.Grade);
      }
      if (keys.Contains(Key.SameWeather))
      {
        query = query.Where(r => r.Race.TrackWeather == this.Race.TrackWeather);
      }

      analyzer.SetSource(query);
    }
  }

  public enum BloodType
  {
    Unknown,

    [Label("父")]
    Father,

    [Label("母")]
    Mother,

    [Label("父父")]
    FatherFather,

    [Label("父母")]
    FatherMother,

    [Label("母父")]
    MotherFather,

    [Label("母母")]
    MotherMother,
  }
}
