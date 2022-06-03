using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
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

    public void Dispose()
    {
      this._disposables.Dispose();
      this.MenuItems.Dispose();
      GC.SuppressFinalize(this);
    }

    public bool IsExistsHorseName(string name)
    {
      return this.MenuItems.Any(i => i.Selector.Name == name);
    }

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

        var born = await db.BornHorses!.FirstOrDefaultAsync(h => h.Code == this.RaceHorse.Key);
        if (born != null)
        {
          this._bloodCode[BloodType.Father] = born.FatherBreedingCode;
          this._bloodCode[BloodType.Mother] = born.MotherBreedingCode;
          this._bloodCode[BloodType.FatherFather] = born.FFBreedingCode;
          this._bloodCode[BloodType.FatherMother] = born.FMBreedingCode;
          this._bloodCode[BloodType.MotherFather] = born.MFBreedingCode;
          this._bloodCode[BloodType.MotherMother] = born.MMBreedingCode;
          this._bloodCode[BloodType.FatherFatherFather] = born.FFFBreedingCode;
          this._bloodCode[BloodType.FatherFatherMother] = born.FFMBreedingCode;
          this._bloodCode[BloodType.FatherMotherFather] = born.FMFBreedingCode;
          this._bloodCode[BloodType.FatherMotherMother] = born.FMMBreedingCode;
          this._bloodCode[BloodType.MotherFatherFather] = born.MFFBreedingCode;
          this._bloodCode[BloodType.MotherFatherMother] = born.MFMBreedingCode;
          this._bloodCode[BloodType.MotherMotherFather] = born.MMFBreedingCode;
          this._bloodCode[BloodType.MotherMotherMother] = born.MMMBreedingCode;
        }
        else
        {
          var horse = await db.Horses!.FirstOrDefaultAsync(h => h.Code == this.RaceHorse.Key);
          if (horse != null)
          {
            this._bloodCode[BloodType.Father] = horse.FatherBreedingCode;
            this._bloodCode[BloodType.Mother] = horse.MotherBreedingCode;
            this._bloodCode[BloodType.FatherFather] = horse.FFBreedingCode;
            this._bloodCode[BloodType.FatherMother] = horse.FMBreedingCode;
            this._bloodCode[BloodType.MotherFather] = horse.MFBreedingCode;
            this._bloodCode[BloodType.MotherMother] = horse.MMBreedingCode;
            this._bloodCode[BloodType.FatherFatherFather] = horse.FFFBreedingCode;
            this._bloodCode[BloodType.FatherFatherMother] = horse.FFMBreedingCode;
            this._bloodCode[BloodType.FatherMotherFather] = horse.FMFBreedingCode;
            this._bloodCode[BloodType.FatherMotherMother] = horse.FMMBreedingCode;
            this._bloodCode[BloodType.MotherFatherFather] = horse.MFFBreedingCode;
            this._bloodCode[BloodType.MotherFatherMother] = horse.MFMBreedingCode;
            this._bloodCode[BloodType.MotherMotherFather] = horse.MMFBreedingCode;
            this._bloodCode[BloodType.MotherMotherMother] = horse.MMMBreedingCode;
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
                if (ff != null)
                {
                  var (fff, ffm) = await GetParentsAsync(ff);
                  if (fff != null) this._bloodCode[BloodType.FatherFatherFather] = fff;
                  if (ffm != null) this._bloodCode[BloodType.FatherFatherMother] = ffm;
                }
                if (fm != null)
                {
                  var (fmf, fmm) = await GetParentsAsync(fm);
                  if (fmf != null) this._bloodCode[BloodType.FatherMotherFather] = fmf;
                  if (fmm != null) this._bloodCode[BloodType.FatherMotherMother] = fmm;
                }
              }
              if (blood.MotherKey != null)
              {
                var (mf, mm) = await GetParentsAsync(blood.MotherKey);
                if (mf != null) this._bloodCode[BloodType.MotherFather] = mf;
                if (mm != null) this._bloodCode[BloodType.MotherMother] = mm;
                if (mf != null)
                {
                  var (mff, mfm) = await GetParentsAsync(mf);
                  if (mff != null) this._bloodCode[BloodType.MotherFatherFather] = mff;
                  if (mfm != null) this._bloodCode[BloodType.MotherFatherMother] = mfm;
                }
                if (mm != null)
                {
                  var (mmf, mmm) = await GetParentsAsync(mm);
                  if (mmf != null) this._bloodCode[BloodType.MotherMotherFather] = mmf;
                  if (mmm != null) this._bloodCode[BloodType.MotherMotherMother] = mmm;
                }
              }
            }
          }
        }

        var bloodKeys = this._bloodCode.Select(p => p.Value).ToArray();

        // 血統番号と繁殖番号が結びついてるのはこのテーブルだけ
        var horses = await db.HorseBloods!.Where(h => bloodKeys.Contains(h.Key)).Select(h => new { h.Code, h.Key, h.Name }).ToArrayAsync();
        var items = new List<MenuItem>();
        foreach (var d in this._bloodCode
          .Join(horses, b => b.Value, h => h.Key, (b, h) => new { Type = b.Key, Key = h.Code, h.Name, BloodKey = h.Key, })
          .Where(h => !string.IsNullOrEmpty(h.Name)))
        {
          this._horseKey.Add(d.Type, d.Key);
          items.Add(new MenuItem(new RaceHorseBloodTrendAnalysisSelector(this, this.Race, this.RaceHorse, d.Key, d.Name, d.Type, d.BloodKey))
          {
            Type = d.Type,
            IsEnabled = !d.Key.All(k => k == '0'),
          });
        }

        ThreadUtil.InvokeOnUiThread(() =>
        {
          this.MenuItems.AddRangeOnScheduler(items.OrderBy(i => i.Type));

          var firstItem = items.FirstOrDefault(i => i.Type == BloodType.MotherFather) ?? items.FirstOrDefault();
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

      public bool IsEnabled { get; set; }

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
      [IgnoreKey]
      BloodHorseSelf,      // 血統馬自身を指定するために内部でAnalyzerを管理する便宜上のキー

      [Label("コース")]
      [ScriptParameterKey("course")]
      SameCourse,

      [Label("地面")]
      [ScriptParameterKey("ground")]
      SameGround,

      [Label("馬場状態")]
      [ScriptParameterKey("condition")]
      SameCondition,

      [Label("天気")]
      [ScriptParameterKey("weather")]
      SameWeather,

      [Label("条件")]
      [ScriptParameterKey("subject")]
      SameSubject,

      [Label("格")]
      [ScriptParameterKey("grade")]
      SameGrade,

      [Label("季節")]
      [ScriptParameterKey("season")]
      [IgnoreKey]
      SameSeason,

      [Label("距離")]
      [ScriptParameterKey("distance")]
      NearDistance,

      [Label("向き")]
      [ScriptParameterKey("direction")]
      SameDirection,

      [Label("複勝")]
      [ScriptParameterKey("placebits")]
      [GroupName("ResultOrder")]
      PlaceBets,

      [Label("着外")]
      [ScriptParameterKey("losed")]
      [GroupName("ResultOrder")]
      Losed,

      [Label("間隔")]
      [ScriptParameterKey("interval")]
      NearInterval,

      [Label("年齢")]
      [ScriptParameterKey("age")]
      SameAge,

      [Label("重賞")]
      [ScriptParameterKey("grades")]
      Grades,
    }

    private readonly string _key;
    private readonly string _bloodKey;
    private IReadOnlyList<RaceHorseAnalyzer>? _allRaces;
    private readonly CompositeDisposable _disposables = new();

    public override string Name { get; }

    public RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    public RaceHorseBloodTrendAnalysisSelectorMenu Menu { get; }

    public BloodType Type { get; }

    public ReactiveProperty<bool> IsSameChildren { get; } = new(true);

    protected override bool IsAutoLoad => this._allRaces != null;

    public RaceHorseBloodTrendAnalysisSelector(RaceHorseBloodTrendAnalysisSelectorMenu menu, RaceData race, RaceHorseData horse, string relativeKey, string relativeName, BloodType type, string bloodKey) : base(typeof(Key))
    {
      this.Menu = menu;
      this.Race = race;
      this.RaceHorse = horse;
      this._key = relativeKey;
      this.Name = relativeName;
      this.Type = type;
      this._bloodKey = bloodKey;

      this.IsSameChildren.Subscribe(isChecked =>
      {
        var item = this.IgnoreKeys.FirstOrDefault(k => k.Key == Key.BloodHorseSelf);
        if (item != null)
        {
          item.IsChecked.Value = !isChecked;
        }
      }).AddTo(this._disposables);

      base.OnFinishedInitialization();
    }

    protected override RaceHorseBloodTrendAnalyzer GenerateAnalyzer()
    {
      return new RaceHorseBloodTrendAnalyzer(this.Race, this.RaceHorse);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceHorseBloodTrendAnalyzer analyzer, int count, int offset, bool isLoadSameHorses)
    {
      if (keys.Contains(Key.BloodHorseSelf))
      {
        await this.InitializeBloodAnalyzerAsync(db, keys, analyzer, count, offset, isLoadSameHorses);
        return;
      }

      /*
      var query = db.Races!
        .Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType)
        .Join(db.RaceHorses!, r => r.Key, rh => rh.RaceKey, (r, rh) => new { Race = r, RaceHorse = rh, })
        .Where(d => d.RaceHorse.RiderCode == this.RaceHorse.RiderCode);
      */
      var query = db.RaceHorses!
        .Join(db.Races!.Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType),
          rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, });

      IQueryable<HorseData> q1 = db.Horses!.Where(h => h.Id == 0);
      IQueryable<BornHorseData> q2 = db.BornHorses!.Where(h => h.Id == 0);
      
      if (this.Type == BloodType.Father)
      {
        q1 = db.Horses!.Where(h => h.FatherBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.FatherBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.FatherFather)
      {
        q1 = db.Horses!.Where(h => h.FFBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.FFBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.FatherFatherFather)
      {
        q1 = db.Horses!.Where(h => h.FFFBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.FFFBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.FatherFatherMother)
      {
        q1 = db.Horses!.Where(h => h.FFMBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.FFMBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.FatherMother)
      {
        q1 = db.Horses!.Where(h => h.FMBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.FMBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.FatherMotherFather)
      {
        q1 = db.Horses!.Where(h => h.FMFBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.FMFBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.FatherMotherMother)
      {
        q1 = db.Horses!.Where(h => h.FMMBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.FMMBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.Mother)
      {
        q1 = db.Horses!.Where(h => h.MotherBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.MotherBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.MotherFather)
      {
        q1 = db.Horses!.Where(h => h.MFBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.MFBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.MotherFatherFather)
      {
        q1 = db.Horses!.Where(h => h.MFFBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.MFFBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.MotherFatherMother)
      {
        q1 = db.Horses!.Where(h => h.MFMBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.MFMBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.MotherMother)
      {
        q1 = db.Horses!.Where(h => h.MMBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.MMBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.MotherMotherFather)
      {
        q1 = db.Horses!.Where(h => h.MMFBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.MMFBreedingCode == this._bloodKey);
      }
      else if (this.Type == BloodType.MotherMotherMother)
      {
        q1 = db.Horses!.Where(h => h.MMMBreedingCode == this._bloodKey);
        q2 = db.BornHorses!.Where(h => h.MMMBreedingCode == this._bloodKey);
      }
      else
      {

      }

      if (keys.Contains(Key.SameCourse))
      {
        query = query.Where(r => r.Race.Course == this.Race.Course);
      }
      if (keys.Contains(Key.SameGround))
      {
        query = query.Where(r => r.Race.TrackGround == this.Race.TrackGround);
      }
      if (keys.Contains(Key.NearDistance))
      {
        query = query.Where(r => r.Race.Distance >= this.Race.Distance - 100 && r.Race.Distance <= this.Race.Distance + 100);
      }
      if (keys.Contains(Key.SameDirection))
      {
        query = query.Where(r => r.Race.TrackCornerDirection == this.Race.TrackCornerDirection);
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
      if (keys.Contains(Key.PlaceBets))
      {
        query = query.Where(r => r.RaceHorse.ResultOrder >= 1 && r.RaceHorse.ResultOrder <= 3);
      }
      if (keys.Contains(Key.Losed))
      {
        query = query.Where(r => r.RaceHorse.ResultOrder > 5);
      }
      if (keys.Contains(Key.SameAge))
      {
        query = query.Where(r => r.RaceHorse.Age == this.RaceHorse.Age);
      }
      if (keys.Contains(Key.Grades))
      {
        query = query.Where(r => r.Race.Grade == RaceGrade.Grade1 || r.Race.Grade == RaceGrade.Grade2 || r.Race.Grade == RaceGrade.Grade3 ||
                                 r.Race.Grade == RaceGrade.LocalGrade1 || r.Race.Grade == RaceGrade.LocalGrade2 || r.Race.Grade == RaceGrade.LocalGrade3);
      }
      if (keys.Contains(Key.NearInterval))
      {
        var (min, max) = AnalysisUtil.GetIntervalRange(this.RaceHorse.PreviousRaceDays);
        query = query.Where(r => r.RaceHorse.PreviousRaceDays >= min && r.RaceHorse.PreviousRaceDays <= max);
      }

      var r0 = query
        .Join(db.HorseBloods!, h => h.RaceHorse.Key, h => h.Code, (d, h) => new { d.Race, d.RaceHorse, BloodKey = h.Code, });
      //var r1 = r0.Join(q1, d => d.BloodKey, q => q.Code, (d, q) => new { d.Race, d.RaceHorse, });
      var r2 = r0.Join(q2, d => d.BloodKey, q => q.Code, (d, q) => new { d.Race, d.RaceHorse, });

      try
      {
        var races = await r2 //.Concat(r1)
          .OrderByDescending(r => r.Race.StartTime)
          .Skip(offset)
          .Take(count)
          .ToArrayAsync();
        var raceHorses = Array.Empty<RaceHorseData>();
        if (isLoadSameHorses)
        {
          var raceKeys = races.Select(r => r.Race.Key).ToArray();
          raceHorses = await db.RaceHorses!
            .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
            .ToArrayAsync();
        }

        var list = new List<RaceHorseAnalyzer>();
        foreach (var race in races)
        {
          list.Add(
            new RaceHorseAnalyzer(
              race.Race,
              race.RaceHorse,
              raceHorses.Where(rh => rh.RaceKey == race.Race.Key).ToArray(),
              await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race)));
        }
        analyzer.SetSource(list);
      }
      catch
      {

      }
    }

    private async Task InitializeBloodAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceHorseBloodTrendAnalyzer analyzer, int count, int offset, bool isLoadSameHorses)
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
      if (keys.Contains(Key.SameGround))
      {
        query = query.Where(r => r.Race.TrackGround == this.Race.TrackGround);
      }
      if (keys.Contains(Key.NearDistance))
      {
        query = query.Where(r => r.Race.Distance / 200 == this.Race.Distance / 200);
      }
      if (keys.Contains(Key.SameDirection))
      {
        query = query.Where(r => r.Race.TrackCornerDirection == this.Race.TrackCornerDirection);
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
      if (keys.Contains(Key.PlaceBets))
      {
        query = query.Where(r => r.Data.ResultOrder >= 1 && r.Data.ResultOrder <= 3);
      }
      if (keys.Contains(Key.Losed))
      {
        query = query.Where(r => r.Data.ResultOrder > 5);
      }
      if (keys.Contains(Key.NearInterval))
      {
        var (min, max) = AnalysisUtil.GetIntervalRange(this.RaceHorse.PreviousRaceDays);
        query = query.Where(r => r.Data.PreviousRaceDays >= min && r.Data.PreviousRaceDays <= max);
      }

      analyzer.SetSource(query);
    }

    public override void Dispose()
    {
      base.Dispose();
      this._disposables.Dispose();
    }
  }

  public enum BloodType
  {
    Unknown,

    [Label("父")]
    Father,

    [Label("父父")]
    FatherFather,

    [Label("父父父")]
    FatherFatherFather,

    [Label("父父母")]
    FatherFatherMother,

    [Label("父母")]
    FatherMother,

    [Label("父母父")]
    FatherMotherFather,

    [Label("父母母")]
    FatherMotherMother,

    [Label("母")]
    Mother,

    [Label("母父")]
    MotherFather,

    [Label("母父父")]
    MotherFatherFather,

    [Label("母父母")]
    MotherFatherMother,

    [Label("母母")]
    MotherMother,

    [Label("母母父")]
    MotherMotherFather,

    [Label("母母母")]
    MotherMotherMother,
  }
}
