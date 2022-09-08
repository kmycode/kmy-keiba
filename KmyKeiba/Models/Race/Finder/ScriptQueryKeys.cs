using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.ExNumber;
using KmyKeiba.Models.Race.Memo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  abstract class ScriptKeyQuery
  {
    public abstract IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query);

    public abstract IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query);

    public abstract IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query);

    public abstract IQueryable<ExternalNumberData> Apply(MyContext db, IQueryable<ExternalNumberData> query);

    public abstract IQueryable<RaceHorseExtraData> Apply(MyContext db, IQueryable<RaceHorseExtraData> query);

    public abstract IQueryable<JrdbRaceHorseData> Apply(MyContext db, IQueryable<JrdbRaceHorseData> query);

    public abstract IEnumerable<RaceData> Apply(MyContext db, IEnumerable<RaceData> query);

    public abstract IEnumerable<RaceHorseData> Apply(MyContext db, IEnumerable<RaceHorseData> query);

    public abstract IEnumerable<RaceHorseExtraData> Apply(MyContext db, IEnumerable<RaceHorseExtraData> query);

    public abstract IEnumerable<JrdbRaceHorseData> Apply(MyContext db, IEnumerable<JrdbRaceHorseData> query);

    public abstract IEnumerable<MemoData> Apply(MyContext db, IEnumerable<MemoData> query);

    public abstract IEnumerable<ExternalNumberData> Apply(MyContext db, IEnumerable<ExternalNumberData> query);

    private protected string GetScriptKey(QueryKey key)
    {
      var attribute = typeof(QueryKey).GetField(key.ToString())!.GetCustomAttributes(true).OfType<QueryKeyAttribute>().FirstOrDefault();
      return attribute?.ScriptKey ?? string.Empty;
    }
  }

  class SimpleScriptKeyQuery : ScriptKeyQuery
  {
    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      return query;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      return query;
    }

    public override IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query)
    {
      return query;
    }

    public override IQueryable<ExternalNumberData> Apply(MyContext db, IQueryable<ExternalNumberData> query)
    {
      return query;
    }

    public override IQueryable<RaceHorseExtraData> Apply(MyContext db, IQueryable<RaceHorseExtraData> query)
    {
      return query;
    }

    public override IQueryable<JrdbRaceHorseData> Apply(MyContext db, IQueryable<JrdbRaceHorseData> query)
    {
      return query;
    }

    public override IEnumerable<RaceData> Apply(MyContext db, IEnumerable<RaceData> query)
    {
      return query;
    }

    public override IEnumerable<RaceHorseData> Apply(MyContext db, IEnumerable<RaceHorseData> query)
    {
      return query;
    }

    public override IEnumerable<MemoData> Apply(MyContext db, IEnumerable<MemoData> query)
    {
      return query;
    }

    public override IEnumerable<RaceHorseExtraData> Apply(MyContext db, IEnumerable<RaceHorseExtraData> query)
    {
      return query;
    }

    public override IEnumerable<JrdbRaceHorseData> Apply(MyContext db, IEnumerable<JrdbRaceHorseData> query)
    {
      return query;
    }

    public override IEnumerable<ExternalNumberData> Apply(MyContext db, IEnumerable<ExternalNumberData> query)
    {
      return query;
    }
  }

  class RaceLambdaScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly Expression<Func<RaceData, bool>> _where;

    public RaceLambdaScriptKeyQuery(Expression<Func<RaceData, bool>> lambda)
    {
      this._where = lambda;
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      return query.Where(this._where);
    }
  }

  class HorseLambdaScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly Expression<Func<RaceHorseData, bool>> _where;

    public HorseLambdaScriptKeyQuery(Expression<Func<RaceHorseData, bool>> lambda)
    {
      this._where = lambda;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      return query.Where(this._where);
    }
  }

  class SameRaceHorseScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly string _raceKey;
    private readonly short _horseNumber;
    private IReadOnlyList<string>? _riders;

    public SameRaceHorseScriptKeyQuery(string raceKey, short horseNumber)
    {
      this._raceKey = raceKey;
      this._horseNumber = horseNumber;
    }

    private void InitializeCaches(MyContext db)
    {
      if (this._riders != null)
      {
        return;
      }
      var horses = (IQueryable<RaceHorseData>)db.RaceHorses!;
      if (this._horseNumber != default)
      {
        horses = horses.Where(h => h.Number == this._horseNumber);
      }
      this._riders = horses.Where(rh => rh.RaceKey == this._raceKey).Select(rh => rh.Key).ToArray();
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      this.InitializeCaches(db);
      return query.Where(h => this._riders!.Contains(h.Key)).Distinct();
    }
  }

  class SameRaceRiderScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly string _raceKey;
    private readonly short _horseNumber;
    private IReadOnlyList<string>? _riders;

    public SameRaceRiderScriptKeyQuery(string raceKey, short horseNumber)
    {
      this._raceKey = raceKey;
      this._horseNumber = horseNumber;
    }

    private void InitializeCaches(MyContext db)
    {
      if (this._riders != null)
      {
        return;
      }
      var horses = (IQueryable<RaceHorseData>)db.RaceHorses!;
      if (this._horseNumber != default)
      {
        horses = horses.Where(h => h.Number == this._horseNumber);
      }
      this._riders = horses.Where(rh => rh.RaceKey == this._raceKey).Select(rh => rh.RiderCode).ToArray();
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      this.InitializeCaches(db);
      return query.Where(h => this._riders!.Contains(h.RiderCode));
    }
  }

  class SameRaceTrainerScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly string _raceKey;
    private readonly short _horseNumber;
    private IReadOnlyList<string>? _riders;

    public SameRaceTrainerScriptKeyQuery(string raceKey, short horseNumber)
    {
      this._raceKey = raceKey;
      this._horseNumber = horseNumber;
    }

    private void InitializeCaches(MyContext db)
    {
      if (this._riders != null)
      {
        return;
      }
      var horses = (IQueryable<RaceHorseData>)db.RaceHorses!;
      if (this._horseNumber != default)
      {
        horses = horses.Where(h => h.Number == this._horseNumber);
      }
      this._riders = horses.Where(rh => rh.RaceKey == this._raceKey).Select(rh => rh.TrainerCode).ToArray();
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      this.InitializeCaches(db);
      return query.Where(h => this._riders!.Contains(h.TrainerCode));
    }
  }

  class SameRaceOwnerScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly string _raceKey;
    private readonly short _horseNumber;
    private IReadOnlyList<string>? _riders;

    public SameRaceOwnerScriptKeyQuery(string raceKey, short horseNumber)
    {
      this._raceKey = raceKey;
      this._horseNumber = horseNumber;
    }

    private void InitializeCaches(MyContext db)
    {
      if (this._riders != null)
      {
        return;
      }
      var horses = (IQueryable<RaceHorseData>)db.RaceHorses!;
      if (this._horseNumber != default)
      {
        horses = horses.Where(h => h.Number == this._horseNumber);
      }
      this._riders = horses.Where(rh => rh.RaceKey == this._raceKey).Select(rh => rh.OwnerCode).ToArray();
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      this.InitializeCaches(db);
      return query.Where(h => this._riders!.Contains(h.OwnerCode));
    }
  }

  class HorseBelongsScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly IReadOnlyList<HorseBelongs> _belongs;
    private readonly bool _isAlive;

    internal static IReadOnlyList<HorseBelongs> GetBelongs(string value, QueryType type)
    {
      if (value.StartsWith('@'))
      {
        value = value[1..];
      }
      if (value.Contains('-'))
      {
        var d = value.Split('-');
        if (short.TryParse(d[0], out var min) && short.TryParse(d[1], out var max))
        {
          value = string.Join(',', Enumerable.Range(min, max - min + 1));
        }
      }
      var values = value.Split(',').Select(v =>
      {
        short.TryParse(v, out var num);
        return (HorseBelongs)num;
      }).Where(v => v != default);

      if (type == QueryType.NotEquals)
      {
        values = Enum.GetValues<HorseBelongs>().Where(v => v != HorseBelongs.Unknown).Except(values);
      }

      return values.ToArray();
    }

    public HorseBelongsScriptKeyQuery(string value, QueryType type)
    {
      this._isAlive = value.StartsWith('@');
      this._belongs = GetBelongs(value, type);
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      var source = db.Horses!.Where(h => this._belongs.Contains(h.Belongs));
      if (this._isAlive) source = source.Where(h => h.Retired == default);
      return query.Join(source, rh => rh.Key, h => h.Code, (rh, h) => rh);
    }
  }

  class RiderBelongsScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly IReadOnlyList<HorseBelongs> _belongs;

    public RiderBelongsScriptKeyQuery(string value, QueryType type)
    {
      this._belongs = HorseBelongsScriptKeyQuery.GetBelongs(value, type);
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      return query.Join(db.Riders!.Where(h => this._belongs.Contains(h.Belongs)), rh => rh.RiderCode, h => h.Code, (rh, h) => rh);
    }
  }

  class TrainerBelongsScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly IReadOnlyList<HorseBelongs> _belongs;

    public TrainerBelongsScriptKeyQuery(string value, QueryType type)
    {
      this._belongs = HorseBelongsScriptKeyQuery.GetBelongs(value, type);
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      return query.Join(db.Trainers!.Where(h => this._belongs.Contains(h.Belongs)), rh => rh.TrainerCode, h => h.Code, (rh, h) => rh);
    }
  }

  interface IRaceHorseAnalyzerScriptQuery
  {
    IEnumerable<RaceHorseAnalyzer> Apply(MyContext db, IEnumerable<RaceHorseAnalyzer> query);
  }

  class MemoScriptKeyQuery : SimpleScriptKeyQuery, IRaceHorseAnalyzerScriptQuery
  {
    private readonly IReadOnlyList<KeyValuePair<MemoTarget, string>> _data;
    private readonly ScriptKeyQuery _query;
    private readonly short _number;

    public MemoScriptKeyQuery(Dictionary<MemoTarget, string> data, ScriptKeyQuery query)
    {
      this._query = query;

      var pointTarget = (MemoTarget)(short)-1;
      var numberTarget = (MemoTarget)(short)-2;
      if (data.TryGetValue(pointTarget, out _))
      {
        data.Remove(pointTarget);
      }
      if (data.TryGetValue(numberTarget, out _))
      {
        short.TryParse(data[numberTarget], out var num);
        this._number = num;
        data.Remove(numberTarget);
      }

      this._data = data.OrderBy(d => d.Key).ToList();
    }

    private Expression<Func<MemoData, bool>> GetMemoFilter()
    {
      var target1 = this._data.ElementAtOrDefault(0).Key;
      var target2 = this._data.ElementAtOrDefault(1).Key;
      var target3 = this._data.ElementAtOrDefault(2).Key;

      if (this._number != default)
      {
        return m => m.Target1 == target1 && m.Target2 == target2 && m.Target3 == target3 && m.Number == this._number;
      }
      return m => m.Target1 == target1 && m.Target2 == target2 && m.Target3 == target3;
    }

    private ExpansionMemoConfig? GetMemoConfig()
    {
      var target1 = this._data.ElementAtOrDefault(0).Key;
      var target2 = this._data.ElementAtOrDefault(1).Key;
      var target3 = this._data.ElementAtOrDefault(2).Key;

      return MemoUtil.GetMemoConfig(target1, target2, target3, this._number);
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      var memo = Expression.Parameter(typeof(MemoData), "x");

      var memoFilter =
        this._query.Apply(db, db.Memos!).Where(this.GetMemoFilter());

      var i = 1;
      foreach (var key in this._data.Take(3))
      {
        var innerKey = Expression.Lambda<Func<MemoData, string>>(Expression.Property(memo, "Key" + i), memo);
        i++;

        if (key.Key == MemoTarget.Course)
        {
          query = query.Join(memoFilter, r => r.Course, m => m.CourseKey, (r, m) => r);
        }
        else if (key.Key == MemoTarget.Day)
        {
          query = query
            .Select(r => new { Race = r, Day = r.Key.Substring(0, 8), })
            .Join(memoFilter, r => r.Day, innerKey, (r, m) => r.Race);
        }
        else if (key.Key == MemoTarget.Race)
        {
          query = query.Join(memoFilter, r => r.Key, innerKey, (r, m) => r);
        }
      }

      if (i > 2)
      {
        query = query.Distinct();
      }

      return query;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      var memo = Expression.Parameter(typeof(MemoData), "x");

      var memoFilter =
        this._query.Apply(db, db.Memos!).Where(this.GetMemoFilter());

      var i = 1;
      foreach (var key in this._data.Take(3))
      {
        var innerKey = Expression.Lambda<Func<MemoData, string>>(Expression.Property(memo, "Key" + i), memo);
        i++;

        if (key.Key == MemoTarget.Rider)
        {
          query = query.Join(memoFilter, h => h.RiderCode, innerKey, (r, m) => r);
        }
        else if (key.Key == MemoTarget.Trainer)
        {
          query = query.Join(memoFilter, h => h.TrainerCode, innerKey, (r, m) => r);
        }
        else if (key.Key == MemoTarget.Owner)
        {
          query = query.Join(memoFilter, h => h.OwnerCode, innerKey, (r, m) => r);
        }
        else if (key.Key == MemoTarget.Horse)
        {
          query = query.Join(memoFilter, h => h.Key, innerKey, (r, m) => r);
        }
        else if (key.Key == MemoTarget.Father)
        {
          query = query
            .Join(db.BornHorses!, h => h.Key, b => b.Code, (h, b) => new { Horse = h, FatherCode = b.FatherBreedingCode, })
            .Join(memoFilter, h => h.FatherCode, innerKey, (h, m) => h.Horse);
        }
        else if (key.Key == MemoTarget.Mother)
        {
          query = query
            .Join(db.BornHorses!, h => h.Key, b => b.Code, (h, b) => new { Horse = h, MotherCode = b.MotherBreedingCode, })
            .Join(memoFilter, h => h.MotherCode, innerKey, (h, m) => h.Horse);
        }
        else if (key.Key == MemoTarget.MotherFather)
        {
          query = query
            .Join(db.BornHorses!, h => h.Key, b => b.Code, (h, b) => new { Horse = h, MotherFatherCode = b.MFBreedingCode, })
            .Join(memoFilter, h => h.MotherFatherCode, innerKey, (h, m) => h.Horse);
        }
      }

      if (i > 2)
      {
        query = query.Distinct();
      }

      return query;
    }

    public IEnumerable<RaceHorseAnalyzer> Apply(MyContext db, IEnumerable<RaceHorseAnalyzer> query)
    {
      var config = this.GetMemoConfig();

      if (config != null)
      {
        var items = new List<RaceHorseAnalyzer>();
        foreach (var item in query)
        {
          var memo = MemoUtil.GetMemoAsync(db, item.Race, config, item).Result;
          if (memo != null && this._query.Apply(db, new[] { memo, }).Any())
          {
            items.Add(item);
          }
        }
        return items;
      }

      return query;
    }

    public override IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query)
    {
      return query;
    }
  }

  class BloodHorseScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly QueryKey _scriptKey;
    private IList<string> _codes;
    private bool _isSelfCode = false;
    private bool _isCheckedSelfCode = false;
    private readonly int _horseNumber;
    private readonly string? _raceKey;

    public BloodHorseScriptKeyQuery(QueryKey scriptKey, IReadOnlyList<string> codes, bool isSelfCode = false, short horseNumber = 0, string? raceKey = null)
    {
      this._scriptKey = scriptKey;
      this._codes = codes.ToList();
      this._isSelfCode = isSelfCode;
      this._horseNumber = horseNumber;
      this._raceKey = raceKey;
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      return query;
    }

    public override IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query)
    {
      return query;
    }

    private object Apply(MyContext db, IQueryable<RaceHorseData>? query1, IEnumerable<RaceHorseData>? query2)
    {
      var type = this._scriptKey switch
      {
        QueryKey.Father => BloodType.Father,
        QueryKey.FatherFather => BloodType.FatherFather,
        QueryKey.FatherFatherFather => BloodType.FatherFatherFather,
        QueryKey.FatherFatherMother => BloodType.FatherFatherMother,
        QueryKey.FatherMother => BloodType.FatherMother,
        QueryKey.FatherMotherFather => BloodType.FatherMotherFather,
        QueryKey.FatherMotherMother => BloodType.FatherMotherFather,
        QueryKey.Mother => BloodType.Mother,
        QueryKey.MotherFather => BloodType.MotherFather,
        QueryKey.MotherFatherFather => BloodType.MotherFatherFather,
        QueryKey.MotherFatherMother => BloodType.MotherFatherMother,
        QueryKey.MotherMother => BloodType.MotherMother,
        QueryKey.MotherMotherFather => BloodType.MotherMotherFather,
        QueryKey.MotherMotherMother => BloodType.MotherMotherMother,
        _ => default,
      };

      if (this._horseNumber != default && this._raceKey != null && this._codes.Count == 1)
      {
        var horse = db.RaceHorses!.FirstOrDefault(rh => rh.Number == this._horseNumber && rh.RaceKey == this._raceKey);
        if (horse != null)
        {
          var key = horse.Key;
          var code = HorseBloodUtil.GetBloodCodeAsync(db, key, type).Result;
          this._codes = new List<string> { code, };
        }
      }

      if (this._codes.Any())
      {
        // 血統番号を繁殖番号に変換
        if (this._codes.Count == 1 && this._codes[0].Length == 10 && !this._isSelfCode)
        {
          this._codes[0] = HorseBloodUtil.GetBloodCodeAsync(db, this._codes[0], type).Result;
          this._isSelfCode = true;
        }

        if (!this._isSelfCode && !this._isCheckedSelfCode)
        {
          // 自分が父になっている馬を検索
          var list = new List<string>();
          foreach (var code in this._codes)
          {
            var parentCode = HorseBloodUtil.GetBloodCodeFromCodeAsync(db, code, type).Result;
            list.Add(parentCode);
          }
          this._codes = list;
          this._isCheckedSelfCode = true;
        }

        Expression<Func<BornHorseData, bool>>? subject = this._scriptKey switch
        {
          QueryKey.Father => b => this._codes.Contains(b.FatherBreedingCode),
          QueryKey.FatherFather => b => this._codes.Contains(b.FFBreedingCode),
          QueryKey.FatherFatherFather => b => this._codes.Contains(b.FFFBreedingCode),
          QueryKey.FatherFatherMother => b => this._codes.Contains(b.FFMBreedingCode),
          QueryKey.FatherMother => b => this._codes.Contains(b.FMBreedingCode),
          QueryKey.FatherMotherFather => b => this._codes.Contains(b.FMFBreedingCode),
          QueryKey.FatherMotherMother => b => this._codes.Contains(b.FMMBreedingCode),
          QueryKey.Mother => b => this._codes.Contains(b.MotherBreedingCode),
          QueryKey.MotherFather => b => this._codes.Contains(b.MFBreedingCode),
          QueryKey.MotherFatherFather => b => this._codes.Contains(b.MFFBreedingCode),
          QueryKey.MotherFatherMother => b => this._codes.Contains(b.MFMBreedingCode),
          QueryKey.MotherMother => b => this._codes.Contains(b.MMBreedingCode),
          QueryKey.MotherMotherFather => b => this._codes.Contains(b.MMFBreedingCode),
          QueryKey.MotherMotherMother => b => this._codes.Contains(b.MMMBreedingCode),
          _ => null,
        };

        if (subject != null)
        {
          if (query1 != null)
          {
            return query1.Join(db.BornHorses!.Where(subject), h => h.Key, b => b.Code, (h, b) => h);
          }
          else if (query2 != null)
          {
            var ids = query2.Select(q => q.Id).ToArray();
            var a = db.RaceHorses!.Where(rh => ids.Contains(rh.Id))
              .Join(db.BornHorses!.Where(subject), h => h.Key, b => b.Code, (h, b) => h)
              .Where(rh => ids.Contains(rh.Id))
              .ToArray();
            return a;
          }
        }
      }
      else
      {
        // すべてを焼き尽くす
        if (query1 != null)
        {
          return query1.Where(r => false);
        }
        else if (query2 != null)
        {
          return Array.Empty<RaceHorseData>();
        }
      }

      return query1 ?? query2 ?? throw new ArgumentException("query");
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      return (IQueryable<RaceHorseData>)this.Apply(db, query, null);
    }

    public override IEnumerable<RaceHorseData> Apply(MyContext db, IEnumerable<RaceHorseData> query)
    {
      return (IEnumerable<RaceHorseData>)this.Apply(db, null, query);
    }
  }

  class HorseBeforeRacesCountScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly IReadOnlyList<ScriptKeyQuery> _queries;
    private readonly int _count = 0;
    private readonly RaceCountComparationRule _rule;
    private readonly bool _hasJrdbQueries;
    private readonly bool _hasExtraQueries;

    public int Count => this._count;

    public RaceCountComparationRule Rule => this._rule;

    public HorseBeforeRacesCountScriptKeyQuery(IReadOnlyList<ScriptKeyQuery> queries, RaceCountComparationRule rule, int count, bool hasJrdbQueries, bool hasExtraQueries)
    {
      this._hasJrdbQueries = hasJrdbQueries;
      this._hasExtraQueries = hasExtraQueries;
      this._rule = rule;
      this._queries = queries;
      this._count = count;
    }

    internal enum RaceCountComparationRule
    {
      Within,  // 以内
      MorePast,  // より前
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      var races = (IQueryable<RaceData>)db.Races!;
      var horses = (IQueryable<RaceHorseData>)db.RaceHorses!;
      foreach (var q in this._queries)
      {
        races = q.Apply(db, races);
        horses = q.Apply(db, horses);
      }

      if (this._hasJrdbQueries)
      {
        IQueryable<JrdbRaceHorseData> jrdbs = db.JrdbRaceHorses!;
        foreach (var q in this._queries)
        {
          jrdbs = q.Apply(db, jrdbs);
        }
        horses = horses.Join(jrdbs, h => new { h.Key, h.RaceKey, }, j => new { j.Key, j.RaceKey, }, (h, j) => h);
      }
      if (this._hasExtraQueries)
      {
        IQueryable<RaceHorseExtraData> extras = db.RaceHorseExtras!;
        foreach (var q in this._queries)
        {
          extras = q.Apply(db, extras);
        }
        horses = horses.Join(extras, h => new { h.Key, h.RaceKey, }, j => new { j.Key, j.RaceKey, }, (h, j) => h);
      }

      var horsesData = horses
        .Join(races, h => h.RaceKey, r => r.Key, (h, r) => new { h.Key, h.RaceCount, });

      var data = query.Join(horsesData, q => q.Key, h => h.Key, (q, h) => new { Horse = q, BeforeRaceCount = h.RaceCount, });
      data = data.Where(d => d.Horse.RaceCount > d.BeforeRaceCount);

      if (this._rule == RaceCountComparationRule.Within)
      {
        data = data.Where(d => d.BeforeRaceCount >= d.Horse.RaceCount - this._count);
      }
      else if (this._rule == RaceCountComparationRule.MorePast)
      {
        data = data.Where(d => d.BeforeRaceCount <= d.Horse.RaceCount - this._count);
      }

      query = data.Select(d => d.Horse);

      return query;
    }
  }

  class HorseBeforeRacesScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly IReadOnlyList<ScriptKeyQuery> _queries;
    private readonly IReadOnlyList<ExpressionScriptKeyQuery> _diffQueries;
    private readonly IReadOnlyList<ExpressionScriptKeyQuery> _diffQueriesBetweenCurrent;
    private readonly bool _hasJrdbQueries;
    private readonly bool _hasExtraQueries;
    private readonly int _beforeSize;
    private readonly int _compareTargetSize;
    private readonly RaceCountRule _countRule;
    private readonly bool _isDiffQueriesContainsExtraData;

    internal enum RaceCountRule
    {
      All,
      AnywaysRun,
      Completely,
    }

    public HorseBeforeRacesScriptKeyQuery(RaceCountRule countRule, int beforeSize, int compareTargetSize, IReadOnlyList<ScriptKeyQuery> queries, IReadOnlyList<ExpressionScriptKeyQuery> diffQueries, IReadOnlyList<ExpressionScriptKeyQuery> diffQueriesBetweenCurrent, bool hasJrdbQueries, bool hasExtraQueries, bool hasExtraQueriesInDiff)
    {
      this._hasJrdbQueries = hasJrdbQueries;
      this._hasExtraQueries = hasExtraQueries;
      this._countRule = countRule;
      this._beforeSize = beforeSize;
      this._compareTargetSize = compareTargetSize;
      this._queries = queries;
      this._isDiffQueriesContainsExtraData = hasExtraQueriesInDiff;

      if (compareTargetSize == 0)
      {
        // $のついたクエリは現在のレースと比較
        this._diffQueries = Array.Empty<ExpressionScriptKeyQuery>();
        this._diffQueriesBetweenCurrent = diffQueries.Concat(diffQueriesBetweenCurrent).ToArray();
      }
      else
      {
        this._diffQueries = diffQueries;
        this._diffQueriesBetweenCurrent = diffQueriesBetweenCurrent;
      }
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      return query;
    }

    public override IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query)
    {
      return query;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      var races = (IQueryable<RaceData>)db.Races!;
      var horses = (IQueryable<RaceHorseData>)db.RaceHorses!;

      if (!this._queries.Any())
      {
        query = query.Join(horses, q => new { q.Key, q.RaceCount, }, h => new { h.Key, RaceCount = (short)(h.RaceCount + this._beforeSize), }, (q, h) => q);
      }
      else
      {
        foreach (var q in this._queries)
        {
          horses = q.Apply(db, horses);
          races = q.Apply(db, races);
        }

        if (this._hasJrdbQueries)
        {
          IQueryable<JrdbRaceHorseData> jrdbs = db.JrdbRaceHorses!;
          foreach (var q in this._queries)
          {
            jrdbs = q.Apply(db, jrdbs);
          }
          horses = horses.Join(jrdbs, h => new { h.Key, h.RaceKey, }, j => new { j.Key, j.RaceKey, }, (h, j) => h);
        }
        if (this._hasExtraQueries)
        {
          IQueryable<RaceHorseExtraData> extras = db.RaceHorseExtras!;
          foreach (var q in this._queries)
          {
            extras = q.Apply(db, extras);
          }
          horses = horses.Join(extras, h => new { h.Key, h.RaceKey, }, j => new { j.Key, j.RaceKey, }, (h, j) => h);
        }

        var tmpr = horses.Join(races, rh => rh.RaceKey, r => r.Key, (rh, r) => new { Race = r, RaceHorse = rh, });
        var tmpre = horses.Join(races, rh => rh.RaceKey, r => r.Key, (rh, r) => new { Race = r, RaceHorse = rh, })
          .Join(db.RaceHorseExtras!, rh => new { rh.RaceHorse.Key, rh.RaceHorse.RaceKey, }, e => new { e.Key, e.RaceKey, }, (rh, e) => new { rh.RaceHorse, rh.Race, Extra = e, });

        Expression<Func<T, bool>> Compare<T>(IQueryable<T> obj, QueryType type, bool isRace, string propertyName, int value, string propertyPrefix = "Compare", bool isShort = true, bool isEnum = false, string? subPropertyName = null, bool isExtra = false)
        {
          var param = Expression.Parameter(typeof(T), "x");
          var before = Expression.Property(param, "Before");
          var beforeRace = Expression.Property(param, "BeforeRace");
          var current = Expression.Property(param, propertyPrefix);
          var currentRace = Expression.Property(param, propertyPrefix + "Race");

          var beforeObj = isRace ? beforeRace : before;
          var currentObj = isRace ? currentRace : current;

          if (isExtra)
          {
            beforeObj = Expression.Property(param, "BeforeExtra");
            currentObj = Expression.Property(param, propertyPrefix + "Extra");
          }

          Expression valueObj = Expression.Constant(value);
          if (subPropertyName != null)
          {
            valueObj = Expression.Property(valueObj, subPropertyName);
          }
          if (isShort)
          {
            valueObj = Expression.Convert(valueObj, typeof(short));
          }
          Expression beforeProperty = Expression.Property(beforeObj, propertyName);
          Expression currentProperty = Expression.Property(currentObj, propertyName);
          if (isEnum)
          {
            beforeProperty = Expression.Convert(beforeProperty, typeof(short));
            currentProperty = Expression.Convert(currentProperty, typeof(short));
          }

          if (type == QueryType.LessThan || type == QueryType.LessThanOrEqual)
          {
            beforeProperty = Expression.Subtract(beforeProperty, valueObj);
          }
          else
          {
            beforeProperty = Expression.Add(beforeProperty, valueObj);
          }

          switch (type)
          {
            case QueryType.Equals:
              return Expression.Lambda<Func<T, bool>>(Expression.Equal(currentProperty, beforeProperty), param);
            case QueryType.NotEquals:
              return Expression.Lambda<Func<T, bool>>(Expression.NotEqual(currentProperty, beforeProperty), param);
            case QueryType.GreaterThan:
              return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(currentProperty, beforeProperty), param);
            case QueryType.GreaterThanOrEqual:
              return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(currentProperty, beforeProperty), param);
            case QueryType.LessThan:
              return Expression.Lambda<Func<T, bool>>(Expression.LessThan(currentProperty, beforeProperty), param);
            case QueryType.LessThanOrEqual:
              return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(currentProperty, beforeProperty), param);
          }
          return t => true;
        }

        IQueryable<T> ApplyQueries<T>(IQueryable<T> tmpQuery)
        {
          foreach (var q in this._diffQueries.Select(qq => new { Query = qq, IsCurrent = false, })
            .Concat(this._diffQueriesBetweenCurrent.Select(qq => new { Query = qq, IsCurrent = true, })))
          {
            var type = q.Query.Type;
            var value = q.Query.Value;
            var prefix = q.IsCurrent ? "Current" : "Compare";
            switch (q.Query.Key)
            {
              case QueryKey.Distance:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.Distance), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Before3HTime:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.BeforeHaronTime3), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.After3HTime:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.AfterHaronTime3), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Weather:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.TrackWeather), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Condition:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.TrackCondition), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.BaneiMoisture:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.BaneiMoisture), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Course:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.Course), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Month:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.StartTime), value, propertyPrefix: prefix, isShort: false, subPropertyName: nameof(DateTime.Month)));
                break;
              case QueryKey.Year:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.StartTime), value, propertyPrefix: prefix, isShort: false, subPropertyName: nameof(DateTime.Year)));
                break;
              case QueryKey.Nichiji:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.Nichiji), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.RaceNumber:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.CourseRaceNumber), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.RiderWeightRule:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.RiderWeight), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.SexRule:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.Sex), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.AreaRule:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.Area), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.HorsesCount:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.HorsesCount), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.GoalHorsesCount:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.ResultHorsesCount), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Ground:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.TrackGround), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.TrackOption:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.TrackOption), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.TrackType:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.TrackType), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Direction:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.TrackCornerDirection), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Grade:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.Grade), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Age:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Age), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Sex:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Sex), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Color:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Color), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Mark:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Mark), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.RaceCount:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.RaceCount), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.RaceCountAfterLastRest:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.RaceCountAfterLastRest), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.RaceCountWithinRunning:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.RaceCountWithinRunning), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.RaceCountWithinRunningCompletely:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.RaceCountWithinRunningCompletely), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.PreviousRaceDays:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.PreviousRaceDays), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Weight:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Weight), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.WeightDiff:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.WeightDiff), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.RiderWeight:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.RiderWeight), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Odds:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Odds), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.PlaceOddsMax:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.PlaceOddsMax), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.PlaceOddsMin:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.PlaceOddsMin), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.FrameNumber:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.FrameNumber), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.HorseNumber:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Number), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Popular:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Popular), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.ResultTime:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.ResultTimeValue), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.ResultTimeDiff:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.TimeDifference), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.A3HTime:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.AfterThirdHalongTimeValue), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Place:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.ResultOrder), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.GoalPlace:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.GoalOrder), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Abnormal:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.AbnormalResult), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.CornerPlace1:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.FirstCornerOrder), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.CornerPlace2:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.SecondCornerOrder), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.CornerPlace3:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.ThirdCornerOrder), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.CornerPlace4:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.FourthCornerOrder), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.ResultLength:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.ResultLength1), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.RunningStyle:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.RunningStyle), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Pci:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseExtraData.Pci), value, propertyPrefix: prefix, isShort: true, isExtra: true));
                break;
              case QueryKey.Rpci:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseExtraData.Rpci), value, propertyPrefix: prefix, isShort: true, isExtra: true));
                break;
              case QueryKey.Pci3:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseExtraData.Pci3), value, propertyPrefix: prefix, isShort: true, isExtra: true));
                break;
            }
          }

          return tmpQuery;
        }

        if (this._diffQueries.Any() || this._diffQueriesBetweenCurrent.Any())
        {
          if (!this._diffQueries.Any())
          {
            // Compare, CompareRaceがいらない
            switch (this._countRule)
            {
              case RaceCountRule.All:
                {
                  if (!this._isDiffQueriesContainsExtraData)
                  {
                    var tmp = query
                      .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                      .Join(tmpr, q => new { q.RaceHorse.Key, q.RaceHorse.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                  else
                  {
                    var tmp = query
                      .Join(db.RaceHorseExtras!, q => new { q.Key, q.RaceKey, }, e => new { e.Key, e.RaceKey, }, (q, e) => new { RaceHorse = q, Extra = e, })
                      .Join(db.Races!, q => q.RaceHorse.RaceKey, r => r.Key, (q, r) => new { q.RaceHorse, q.Extra, Race = r, })
                      .Join(tmpre, q => new { q.RaceHorse.Key, q.RaceHorse.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._beforeSize), },
                        (q, h) => new { BeforeRace = h.Race, BeforeExtra = h.Extra, Before = h.RaceHorse, CurrentRace = q.Race, CurrentExtra = q.Extra, Current = q.RaceHorse, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                }
                break;
              case RaceCountRule.AnywaysRun:
                {
                  if (!this._isDiffQueriesContainsExtraData)
                  {
                    var tmp = query
                      .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                      .Join(tmpr, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                  else
                  {
                    var tmp = query
                      .Join(db.RaceHorseExtras!, q => new { q.Key, q.RaceKey, }, e => new { e.Key, e.RaceKey, }, (q, e) => new { RaceHorse = q, Extra = e, })
                      .Join(db.Races!, q => q.RaceHorse.RaceKey, r => r.Key, (q, r) => new { q.RaceHorse, q.Extra, Race = r, })
                      .Join(tmpre, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._beforeSize), },
                        (q, h) => new { BeforeRace = h.Race, BeforeExtra = h.Extra, Before = h.RaceHorse, CurrentRace = q.Race, CurrentExtra = q.Extra, Current = q.RaceHorse, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                }
                break;
              case RaceCountRule.Completely:
                {
                  if (!this._isDiffQueriesContainsExtraData)
                  {
                    var tmp = query
                      .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                      .Join(tmpr, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                  else
                  {
                    var tmp = query
                      .Join(db.RaceHorseExtras!, q => new { q.Key, q.RaceKey, }, e => new { e.Key, e.RaceKey, }, (q, e) => new { RaceHorse = q, Extra = e, })
                      .Join(db.Races!, q => q.RaceHorse.RaceKey, r => r.Key, (q, r) => new { q.RaceHorse, q.Extra, Race = r, })
                      .Join(tmpre, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._beforeSize), },
                        (q, h) => new { BeforeRace = h.Race, BeforeExtra = h.Extra, Before = h.RaceHorse, CurrentRace = q.Race, CurrentExtra = q.Extra, Current = q.RaceHorse, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                }
                break;
            }
          }
          else
          {
            switch (this._countRule)
            {
              case RaceCountRule.All:
                {
                  if (!this._isDiffQueriesContainsExtraData)
                  {
                    var tmp = query
                      .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                      .Join(tmpr, q => new { q.RaceHorse.Key, q.RaceHorse.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, })
                      .Join(tmpr, q => new { q.Current.Key, q.Current.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._compareTargetSize), }, (q, h) => new { q.BeforeRace, q.Before, q.CurrentRace, q.Current, Compare = h.RaceHorse, CompareRace = h.Race, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                  else
                  {
                    var tmp = query
                      .Join(db.RaceHorseExtras!, q => new { q.Key, q.RaceKey, }, e => new { e.Key, e.RaceKey, }, (q, e) => new { RaceHorse = q, Extra = e, })
                      .Join(db.Races!, q => q.RaceHorse.RaceKey, r => r.Key, (q, r) => new { q.RaceHorse, q.Extra, Race = r, })
                      .Join(tmpre, q => new { q.RaceHorse.Key, q.RaceHorse.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, BeforeExtra = h.Extra, Before = h.RaceHorse, CurrentRace = q.Race, CurrentExtra = q.Extra, Current = q.RaceHorse, })
                      .Join(tmpre, q => new { q.Current.Key, q.Current.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._compareTargetSize), },
                        (q, h) => new { q.BeforeRace, q.BeforeExtra, q.Before, q.CurrentRace, q.CurrentExtra, q.Current, CompareRace = h.Race, CompareExtra = h.Extra, Compare = h.RaceHorse, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                    var testobj = tmp.First();
                  }
                }
                break;
              case RaceCountRule.AnywaysRun:
                {
                  if (!this._isDiffQueriesContainsExtraData)
                  {
                    var tmp = query
                      .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                      .Join(tmpr, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, })
                      .Join(tmpr, q => new { q.Current.Key, RaceCount = q.Current.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._compareTargetSize), }, (q, h) => new { q.BeforeRace, q.Before, q.CurrentRace, q.Current, Compare = h.RaceHorse, CompareRace = h.Race, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                  else
                  {
                    var tmp = query
                      .Join(db.RaceHorseExtras!, q => new { q.Key, q.RaceKey, }, e => new { e.Key, e.RaceKey, }, (q, e) => new { RaceHorse = q, Extra = e, })
                      .Join(db.Races!, q => q.RaceHorse.RaceKey, r => r.Key, (q, r) => new { q.RaceHorse, q.Extra, Race = r, })
                      .Join(tmpre, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, BeforeExtra = h.Extra, Before = h.RaceHorse, CurrentRace = q.Race, CurrentExtra = q.Extra, Current = q.RaceHorse, })
                      .Join(tmpre, q => new { q.Current.Key, RaceCount = q.Current.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._compareTargetSize), },
                        (q, h) => new { q.BeforeRace, q.BeforeExtra, q.Before, q.CurrentRace, q.CurrentExtra, q.Current, CompareRace = h.Race, CompareExtra = h.Extra, Compare = h.RaceHorse, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                }
                break;
              case RaceCountRule.Completely:
                {
                  if (!this._isDiffQueriesContainsExtraData)
                  {
                    var tmp = query
                      .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                      .Join(tmpr, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, })
                      .Join(tmpr, q => new { q.Current.Key, RaceCount = q.Current.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._compareTargetSize), }, (q, h) => new { q.BeforeRace, q.Before, q.CurrentRace, q.Current, Compare = h.RaceHorse, CompareRace = h.Race, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                  else
                  {
                    var tmp = query
                      .Join(db.RaceHorseExtras!, q => new { q.Key, q.RaceKey, }, e => new { e.Key, e.RaceKey, }, (q, e) => new { RaceHorse = q, Extra = e, })
                      .Join(db.Races!, q => q.RaceHorse.RaceKey, r => r.Key, (q, r) => new { q.RaceHorse, q.Extra, Race = r, })
                      .Join(tmpre, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, BeforeExtra = h.Extra, Before = h.RaceHorse, CurrentRace = q.Race, CurrentExtra = q.Extra, Current = q.RaceHorse, })
                      .Join(tmpre, q => new { q.Current.Key, RaceCount = q.Current.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._compareTargetSize), },
                        (q, h) => new { q.BeforeRace, q.BeforeExtra, q.Before, q.CurrentRace, q.CurrentExtra, q.Current, CompareRace = h.Race, CompareExtra = h.Extra, Compare = h.RaceHorse, });
                    query = ApplyQueries(tmp).Select(t => t.Current);
                  }
                }
                break;
            }
          }
        }
        else
        {
          // 過去レース存在確認または固定値比較
          switch (this._countRule)
          {
            case RaceCountRule.All:
              {
                var tmp = query
                  .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => q)
                  .Join(tmpr, q => new { q.Key, q.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._beforeSize), }, (q, h) => q);
                query = tmp.Select(t => t);
              }
              break;
            case RaceCountRule.AnywaysRun:
              {
                var tmp = query
                  .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => q)
                  .Join(tmpr, q => new { q.Key, RaceCount = q.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._beforeSize), }, (q, h) => q);
                query = tmp.Select(t => t);
              }
              break;
            case RaceCountRule.Completely:
              {
                var tmp = query
                  .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => q)
                  .Join(tmpr, q => new { q.Key, RaceCount = q.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._beforeSize), }, (q, h) => q);
                query = tmp.Select(t => t);
              }
              break;
          }
        }
      }

      return query;
    }
  }

  class TopHorsesScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly IReadOnlyList<ScriptKeyQuery> _queries;
    private readonly bool _hasExtraQueries;
    private readonly bool _hasJrdbQueries;
    private readonly int _minCount;
    private readonly int _maxCount;
    private readonly int _minRate;
    private readonly int _maxRate;

    public TopHorsesScriptKeyQuery(IReadOnlyList<ScriptKeyQuery> queries, bool hasExtraQueries, bool hasJrdbQueries, int minCount, int maxCount, int minRate, int maxRate)
    {
      this._queries = queries;
      this._hasJrdbQueries = hasJrdbQueries;
      this._hasExtraQueries = hasExtraQueries;
      this._minCount = minCount;
      this._maxCount = maxCount;
      this._minRate = minRate;
      this._maxRate = maxRate;
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      IQueryable<RaceHorseData> horses = db.RaceHorses!;
      foreach (var q in this._queries)
      {
        horses = q.Apply(db, horses);
      }

      if (this._hasJrdbQueries)
      {
        IQueryable<JrdbRaceHorseData> jrdbs = db.JrdbRaceHorses!;
        foreach (var q in this._queries)
        {
          jrdbs = q.Apply(db, jrdbs);
        }
        horses = horses.Join(jrdbs, h => new { h.Key, h.RaceKey, }, j => new { j.Key, j.RaceKey, }, (h, j) => h);
      }
      if (this._hasExtraQueries)
      {
        IQueryable<RaceHorseExtraData> extras = db.RaceHorseExtras!;
        foreach (var q in this._queries)
        {
          extras = q.Apply(db, extras);
        }
        horses = horses.Join(extras, h => new { h.Key, h.RaceKey, }, j => new { j.Key, j.RaceKey, }, (h, j) => h);
      }

      if (this._maxCount <= 0 || this._maxRate <= 0)
      {
        var raceKeys = horses.GroupBy(h => h.RaceKey).Select(g => g.Key).ToArray();
        query = query.Where(h => !raceKeys.Contains(h.Key));
      }
      else
      {
        if (this._minCount != 1 || this._maxCount < 18)
        {
          var raceKeys = horses.GroupBy(h => h.RaceKey)
            .Select(g => new { g.Key, Count = g.Count(), })
            .Where(g => g.Count >= this._minCount && g.Count <= this._maxCount)
            .Select(g => g.Key)
            .ToArray();
          //horses = horses.Where(h => raceKeys.Contains(h.RaceKey));
          query = query.Where(r => raceKeys.Contains(r.Key));
        }
        if (this._minRate > 0 || this._maxRate < 100)
        {
          var raceKeys = horses.GroupBy(h => h.RaceKey)
            .Select(g => new { g.Key, Count = g.Count() })
            .Join(db.Races!, g => g.Key, r => r.Key, (g, r) => new { Key = g.Key, g.Count, r.HorsesCount, })
            .Select(g => new { g.Key, Rate = g.Count * 100 / g.HorsesCount, })
            .Where(g => g.Rate >= this._minRate && g.Rate <= this._maxRate)
            .Select(g => g.Key)
            .ToArray();
          //horses = horses.Where(h => raceKeys.Contains(h.RaceKey));
          query = query.Where(r => raceKeys.Contains(r.Key));
        }

        //var raceKeys2 = horses.GroupBy(h => h.RaceKey).Select(h => h.Key).ToArray();
        //query = query.Where(r => raceKeys2.Contains(r.Key));
      }

      return query;
    }
  }

  class HorseExternalNumberScriptKeyQuery : SimpleScriptKeyQuery
  {
    public readonly uint _configId;
    private readonly ScriptKeyQuery _pointQuery;

    public HorseExternalNumberScriptKeyQuery(uint configId, ScriptKeyQuery pointQuery)
    {
      this._configId = configId;
      this._pointQuery = pointQuery;

      if (ExternalNumberUtil.GetConfig(this._configId) == null)
      {
        this._configId = 0;
      }
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      if (this._configId == 0)
      {
        return query;
      }

      var numbers = db.ExternalNumbers!
        .Where(n => n.ConfigId == this._configId);
      numbers = this._pointQuery.Apply(db, numbers);

      query = query.Join(numbers, q => new { q.RaceKey, q.Number, }, n => new { n.RaceKey, Number = n.HorseNumber, }, (q, n) => q);

      return query;
    }

    public override IEnumerable<RaceData> Apply(MyContext db, IEnumerable<RaceData> query)
    {
      return query;
    }

    public override IEnumerable<RaceHorseData> Apply(MyContext db, IEnumerable<RaceHorseData> query)
    {
      var config = ExternalNumberUtil.GetConfig(this._configId);
      if (config != null)
      {
        var result = new List<RaceHorseData>();
        foreach (var horse in query)
        {
          var value = ExternalNumberUtil.GetValueAsync(db, config, horse.RaceKey, horse.Number).Result;
          if (value != null)
          {
            if (this._pointQuery.Apply(db, new[] { value }).Any())
            {
              result.Add(horse);
            }
          }
        }
        return result;
      }
      return query;
    }
  }

  class ExpressionScriptKeyQuery : SimpleScriptKeyQuery
  {
    public QueryKey Key { get; }

    public QueryType Type { get; }

    public int Value { get; private set; }

    public int MaxValue { get; }

    public int[] Values { get; }

    public string? StringValue { get; }

    public RaceData? Race { get; }

    public RaceHorseData? Horse { get; }

    public bool IsCompareCurrentRace { get; }

    public ExpressionScriptKeyQuery(QueryKey key, QueryType type, int value, int maxValue, RaceData? race, RaceHorseData? horse, bool isCompareCurrentRace)
    {
      if (maxValue < value)
      {
        var tmp = maxValue;
        maxValue = value;
        value = tmp;
      }

      this.Key = key;
      this.Type = type;
      this.Value = value;
      this.Values = new int[] { value, };
      this.MaxValue = maxValue;
      this.Race = race;
      this.Horse = horse;
      this.IsCompareCurrentRace = isCompareCurrentRace;
    }

    public ExpressionScriptKeyQuery(QueryKey key, QueryType type, int value, RaceData? race, RaceHorseData? horse, bool isCompareCurrentRace) : this(key, type, value, value, race, horse, isCompareCurrentRace)
    {
    }

    public ExpressionScriptKeyQuery(QueryKey key, QueryType type, int[] values)
    {
      this.Key = key;
      this.Type = type;
      this.Value = values.FirstOrDefault();
      this.Values = values;
      this.MaxValue = default;
    }

    public ExpressionScriptKeyQuery(QueryKey key, QueryType type, string value)
    {
      this.Key = key;
      this.Type = type;
      this.Values = Array.Empty<int>();
      this.StringValue = value;
    }

    private Expression<Func<RaceData, bool>> GenerateRaceFilter()
    {
      switch (this.Key)
      {
        case QueryKey.RaceKey:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            return this.BuildStringQuery<RaceData>(nameof(RaceData.Key));
          }
          break;
        case QueryKey.Weather:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackWeather), isEnum: true);
        case QueryKey.Condition:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackCondition), isEnum: true);
        case QueryKey.BaneiMoisture:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.BaneiMoisture));
        case QueryKey.Course:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.Course), isEnum: true);
        case QueryKey.Ground:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackGround), isEnum: true);
        case QueryKey.Direction:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackCornerDirection), isEnum: true);
        case QueryKey.TrackType:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackType), isEnum: true);
        case QueryKey.TrackOption:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackOption), isEnum: true);
        case QueryKey.Distance:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.Distance));
        case QueryKey.Day:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.StartTime), nameof(DateTime.Day), false);
        case QueryKey.Month:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.StartTime), nameof(DateTime.Month), false);
        case QueryKey.Year:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.StartTime), nameof(DateTime.Year), false);
        case QueryKey.Date:
          return this.BuildDateQuery<RaceData>(nameof(RaceData.StartTime));
        case QueryKey.Hour:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.StartTime), nameof(DateTime.Hour), false);
        case QueryKey.Nichiji:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.Nichiji));
        case QueryKey.RaceNumber:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.CourseRaceNumber));
        case QueryKey.RaceName:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            return this.BuildStringQuery<RaceData>(nameof(RaceData.Name));
          }
          break;
        case QueryKey.GradeId:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.GradeId));
        case QueryKey.SubjectAge2:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.SubjectAge2), isEnum: true);
        case QueryKey.SubjectAge3:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.SubjectAge3), isEnum: true);
        case QueryKey.SubjectAge4:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.SubjectAge4), isEnum: true);
        case QueryKey.SubjectAge5:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.SubjectAge5), isEnum: true);
        case QueryKey.Grade:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.Grade), isEnum: true);
        case QueryKey.RiderWeightRule:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.RiderWeight), isEnum: true);
        case QueryKey.AreaRule:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.Area), isEnum: true);
        case QueryKey.SexRule:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.Sex), isEnum: true);
        case QueryKey.CrossRule:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.Cross), isEnum: true);
        case QueryKey.PrizeMoney1:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.PrizeMoney1), isShort: false);
        case QueryKey.HorsesCount:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.HorsesCount), isShort: true);
        case QueryKey.GoalHorsesCount:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.ResultHorsesCount), isShort: true);
        case QueryKey.Before3HTime:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.BeforeHaronTime3), isShort: true);
        case QueryKey.After3HTime:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.AfterHaronTime3), isShort: true);
        case QueryKey.DataStatus:
          return this.BuildNumericQuery<RaceData>(nameof(RaceData.DataStatus), isShort: true, isEnum: true);
      }

      return r => true;
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      return query.Where(this.GenerateRaceFilter());
    }

    public override IEnumerable<RaceData> Apply(MyContext db, IEnumerable<RaceData> query)
    {
      return query.Where(this.GenerateRaceFilter().Compile());
    }

    private Expression<Func<RaceHorseData, bool>> GenerateRaceHorseFilter()
    {
      switch (this.Key)
      {
        case QueryKey.HorseKey:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            return this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.Key));
          }
          break;
        case QueryKey.Age:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Age));
        case QueryKey.Sex:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Sex), isEnum: true);
        case QueryKey.HorseType:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Type), isEnum: true);
        case QueryKey.Color:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Color), isEnum: true);
        case QueryKey.HorseNumber:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Number));
        case QueryKey.FrameNumber:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.FrameNumber));
        case QueryKey.Mark:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Mark), isEnum: true);
        case QueryKey.RiderCode:
          return this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.RiderCode));
        case QueryKey.TrainerCode:
          return this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.TrainerCode));
        case QueryKey.OwnerCode:
          return this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.OwnerCode));
        case QueryKey.Place:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.ResultOrder));
        case QueryKey.GoalPlace:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.GoalOrder));
        case QueryKey.ResultLength:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.ResultLength1));
        case QueryKey.Abnormal:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.AbnormalResult), isEnum: true);
        case QueryKey.Popular:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Popular));
        case QueryKey.ResultTime:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.ResultTimeValue));
        case QueryKey.ResultTimeDiff:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.TimeDifference));
        case QueryKey.CornerPlace1:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.FirstCornerOrder));
        case QueryKey.CornerPlace2:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.SecondCornerOrder));
        case QueryKey.CornerPlace3:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.ThirdCornerOrder));
        case QueryKey.CornerPlace4:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.FourthCornerOrder));
        case QueryKey.Weight:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Weight));
        case QueryKey.WeightDiff:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.WeightDiff));
        case QueryKey.RiderWeight:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.RiderWeight));
        case QueryKey.Odds:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Odds));
        case QueryKey.PlaceOddsMax:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.PlaceOddsMax));
        case QueryKey.PlaceOddsMin:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.PlaceOddsMin));
        case QueryKey.A3HTime:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.AfterThirdHalongTimeValue));
        case QueryKey.RunningStyle:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.RunningStyle), isEnum: true);
        case QueryKey.PreviousRaceDays:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.PreviousRaceDays));
        case QueryKey.RaceCount:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.RaceCount));
        case QueryKey.RaceCountWithinRunning:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.RaceCountWithinRunning));
        case QueryKey.RaceCountWithinRunningCompletely:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.RaceCountWithinRunningCompletely));
        case QueryKey.RaceCountAfterLastRest:
          return this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.RaceCountAfterLastRest));
        case QueryKey.HorseName:
          return this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.Name));
        case QueryKey.RiderName:
          return this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.RiderName));
        case QueryKey.TrainerName:
          return this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.TrainerName));
        case QueryKey.OwnerName:
          return this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.OwnerName));
      }

      return h => true;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      return query.Where(this.GenerateRaceHorseFilter());
    }

    public override IEnumerable<RaceHorseData> Apply(MyContext db, IEnumerable<RaceHorseData> query)
    {
      return query.Where(this.GenerateRaceHorseFilter().Compile());
    }

    private Expression<Func<JrdbRaceHorseData, bool>> GenerateJrdbRaceHorseFilter()
    {
      switch (this.Key)
      {
        case QueryKey.IdmPoint:
          return this.BuildNumericQuery<JrdbRaceHorseData>(nameof(JrdbRaceHorseData.IdmPoint));
        case QueryKey.RiderPoint:
          return this.BuildNumericQuery<JrdbRaceHorseData>(nameof(JrdbRaceHorseData.RiderPoint));
        case QueryKey.InfoPoint:
          return this.BuildNumericQuery<JrdbRaceHorseData>(nameof(JrdbRaceHorseData.InfoPoint));
        case QueryKey.TotalPoint:
          return this.BuildNumericQuery<JrdbRaceHorseData>(nameof(JrdbRaceHorseData.TotalPoint));
        case QueryKey.HorseClimb:
          return this.BuildNumericQuery<JrdbRaceHorseData>(nameof(JrdbRaceHorseData.Climb), isEnum: true);
      }

      return j => true;
    }

    public override IQueryable<JrdbRaceHorseData> Apply(MyContext db, IQueryable<JrdbRaceHorseData> query)
    {
      return query.Where(this.GenerateJrdbRaceHorseFilter());
    }

    public override IEnumerable<JrdbRaceHorseData> Apply(MyContext db, IEnumerable<JrdbRaceHorseData> query)
    {
      return query.Where(this.GenerateJrdbRaceHorseFilter().Compile());
    }

    private Expression<Func<RaceHorseExtraData, bool>> GenerateRaceHorseExtraFilter()
    {
      switch (this.Key)
      {
        case QueryKey.TrainingCatchupPoint:
          return this.BuildNumericQuery<RaceHorseExtraData>(nameof(RaceHorseExtraData.TrainingCatchupPoint));
        case QueryKey.TrainingFinishPoint:
          return this.BuildNumericQuery<RaceHorseExtraData>(nameof(RaceHorseExtraData.TrainingFinishPoint));

        case QueryKey.Pci:
          return this.BuildNumericQuery<RaceHorseExtraData>(nameof(RaceHorseExtraData.Pci));
        case QueryKey.Pci3:
          return this.BuildNumericQuery<RaceHorseExtraData>(nameof(RaceHorseExtraData.Pci3));
        case QueryKey.Rpci:
          return this.BuildNumericQuery<RaceHorseExtraData>(nameof(RaceHorseExtraData.Rpci));
        case QueryKey.Before3HTimeNormalized:
          return this.BuildNumericQuery<RaceHorseExtraData>(nameof(RaceHorseExtraData.Before3HaronTimeFixed));
      }

      return j => true;
    }

    public override IQueryable<RaceHorseExtraData> Apply(MyContext db, IQueryable<RaceHorseExtraData> query)
    {
      return query.Where(this.GenerateRaceHorseExtraFilter());
    }

    public override IEnumerable<RaceHorseExtraData> Apply(MyContext db, IEnumerable<RaceHorseExtraData> query)
    {
      return query.Where(this.GenerateRaceHorseExtraFilter().Compile());
    }

    public override IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query)
    {
      switch (this.Key)
      {
        case QueryKey.Point:
          query = query.Where(this.BuildNumericQuery<MemoData>(nameof(MemoData.Point)));
          break;
      }
      return query;
    }

    public override IEnumerable<MemoData> Apply(MyContext db, IEnumerable<MemoData> query)
    {
      switch (this.Key)
      {
        case QueryKey.Point:
          query = query.Where(this.BuildNumericQuery<MemoData>(nameof(MemoData.Point)).Compile());
          break;
      }
      return query;
    }

    public override IQueryable<ExternalNumberData> Apply(MyContext db, IQueryable<ExternalNumberData> query)
    {
      switch (this.Key)
      {
        case QueryKey.Point:
          query = query.Where(this.BuildNumericQuery<ExternalNumberData>(nameof(ExternalNumberData.Value), isShort: false));
          break;
      }
      return query;
    }

    public override IEnumerable<ExternalNumberData> Apply(MyContext db, IEnumerable<ExternalNumberData> query)
    {
      switch (this.Key)
      {
        case QueryKey.Point:
          query = query.Where(this.BuildNumericQuery<ExternalNumberData>(nameof(ExternalNumberData.Value), isShort: false).Compile());
          break;
      }
      return query;
    }

    private Expression<Func<T, bool>> BuildNumericQuery<T>(string propertyName, bool isShort = true, bool isDouble = false, bool isEnum = false)
    {
      return this.BuildNumericQuery<T>(propertyName, null, isShort, isDouble, isEnum);
    }

    private Expression<Func<T, bool>> BuildNumericQuery<T>(string propertyName, string? propertyName2, bool isShort = true, bool isDouble = false, bool isEnum = false)
    {
      var param = Expression.Parameter(typeof(T), "x");

      Expression property = Expression.Property(param, propertyName);
      if (propertyName2 != null)
      {
        property = Expression.Property(property, propertyName2);
      }

      Expression EnumToNumeric(Expression property)
      {
        if (isEnum)
        {
          if (isShort)
          {
            property = Expression.Convert(property, typeof(short));
            property = Expression.Convert(property, typeof(int));
          }
          else
          {
            property = Expression.Convert(property, typeof(int));
          }
        }
        else if (isShort)
        {
          property = Expression.Convert(property, typeof(int));
        }
        return property;
      }

      property = EnumToNumeric(property);
      var isCompareCurrentRace = false;
      if (this.IsCompareCurrentRace)
      {
        object? obj = typeof(T) == typeof(RaceData) ? this.Race : this.Horse;
        if (obj != null)
        {
          Expression currentProperty = Expression.Property(Expression.Constant(obj), propertyName);
          if (propertyName2 != null)
          {
            currentProperty = Expression.Property(currentProperty, propertyName2);
          }
          currentProperty = EnumToNumeric(currentProperty);
          property = Expression.Subtract(property, currentProperty);
          isCompareCurrentRace = true;
        }
      }

      Expression value = Expression.Constant(isDouble ? (double)this.Value : (object)this.Value);
      Expression maxValue = Expression.Constant(isDouble ? (double)this.MaxValue : (object)this.MaxValue);
      var values = Expression.Constant(this.Values.ToList());

      if (isCompareCurrentRace)
      {
        if (this.Type == QueryType.LessThan || this.Type == QueryType.LessThanOrEqual)
        {
          value = Expression.Multiply(value, Expression.Constant(-1));
          maxValue = Expression.Multiply(maxValue, Expression.Constant(-1));
        }
      }

      return this.BuildNumericQuery<T>(param, property, value, maxValue, values);
    }

    private Expression<Func<T, bool>> BuildNumericQuery<T>(ParameterExpression param, Expression property, Expression value, Expression maxValue, Expression values)
    {
      return BuildNumericQuery<T>(this.Type, param, property, value, maxValue, values);
    }

    private Expression<Func<T, bool>> BuildNumericQuery<T>(IQueryable<T> test, ParameterExpression param, Expression property, Expression value, Expression maxValue, Expression values)
    {
      return BuildNumericQuery<T>(this.Type, param, property, value, maxValue, values);
    }

    private Expression<Func<T, bool>> BuildNumericQuery<T>(IEnumerable<T> test, ParameterExpression param, Expression property, Expression value, Expression maxValue, Expression values)
    {
      return BuildNumericQuery<T>(this.Type, param, property, value, maxValue, values);
    }

    internal static Expression<Func<T, bool>> BuildNumericQuery<T>(QueryType type, ParameterExpression param, Expression property, Expression value, Expression maxValue, Expression values)
    {
      if (type == QueryType.Equals)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Equal(property, value), param);
      }
      if (type == QueryType.NotEquals)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.NotEqual(property, value), param);
      }
      if (type == QueryType.GreaterThan)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(property, value), param);
      }
      if (type == QueryType.GreaterThanOrEqual)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(property, value), param);
      }
      if (type == QueryType.LessThan)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.LessThan(property, value), param);
      }
      if (type == QueryType.LessThanOrEqual)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(property, value), param);
      }

      if (type == QueryType.Range)
      {
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.GreaterThanOrEqual(property, value), Expression.LessThan(property, maxValue)),
          param);
      }
      if (type == QueryType.RangeOrEqual)
      {
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.GreaterThanOrEqual(property, value), Expression.LessThanOrEqual(property, maxValue)),
          param);
      }
      if (type == QueryType.NotRange)
      {
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.LessThan(property, value), Expression.GreaterThanOrEqual(property, maxValue)),
          param);
      }
      if (type == QueryType.NotRangeOrEqual)
      {
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.LessThan(property, value), Expression.GreaterThan(property, maxValue)),
          param);
      }

      if (type == QueryType.Contains)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Call(values, "Contains", null, property), param);
      }
      if (type == QueryType.Excepts)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Call(values, "Contains", null, property), Expression.Constant(false)), param);
      }

      throw new NotSupportedException();
    }

    private Expression<Func<T, bool>> BuildStringQuery<T>(string propertyName)
    {
      if (string.IsNullOrEmpty(this.StringValue))
      {
        return _ => true;
      }

      var param = Expression.Parameter(typeof(T), "x");
      var property = Expression.Property(param, propertyName);
      var value = Expression.Constant(this.StringValue);

      if (this.Type == QueryType.Equals)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Equal(property, value), param);
      }
      if (this.Type == QueryType.Contains)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Call(property, "Contains", null, value), param);
      }
      if (this.Type == QueryType.StartsWith)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Call(property, "StartsWith", null, value), param);
      }
      if (this.Type == QueryType.EndsWith)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Call(property, "EndsWith", null, value), param);
      }

      throw new NotSupportedException();
    }

    private Expression<Func<T, bool>> BuildDateQuery<T>(string propertyName)
    {
      var param = Expression.Parameter(typeof(T), "x");
      var property = Expression.Property(param, propertyName);
      var property2 = Expression.Add(
        Expression.Add(
          Expression.Multiply(Expression.Property(property, nameof(DateTime.Year)), Expression.Constant(10000)),
          Expression.Multiply(Expression.Property(property, nameof(DateTime.Month)), Expression.Constant(100))),
        Expression.Property(property, nameof(DateTime.Day)));

      var value = Expression.Constant((int)this.Value);
      var maxValue = Expression.Constant((int)this.MaxValue);
      var values = Expression.Constant(this.Values.ToList());

      return this.BuildNumericQuery<T>(param, property2, value, maxValue, values);
    }
  }

  class RaceSubjectScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly IReadOnlyList<RaceSubjectType> _subjects;
    private readonly IReadOnlyList<string> _localSubjects;
    private readonly QueryType _type;

    public RaceSubjectScriptKeyQuery(QueryType type, IReadOnlyList<RaceSubjectType> subjects, IReadOnlyList<string> localSubjects)
    {
      this._type = type;
      this._subjects = subjects;
      this._localSubjects = localSubjects;
    }

    private Expression<Func<RaceData, bool>> GenerateExpression()
    {
      if (this._type == QueryType.Equals || this._type == QueryType.Contains)
      {
        return r => this._subjects.Contains(r.SubjectAge2) || this._subjects.Contains(r.SubjectAge3) ||
          this._subjects.Contains(r.SubjectAge4) || this._subjects.Contains(r.SubjectAge5) ||
          this._subjects.Contains(r.SubjectAgeYounger) || this._localSubjects.Contains(r.SubjectInfo1) ||
          this._localSubjects.Contains(r.SubjectInfo2);
      }
      else
      {
        return r => !this._subjects.Contains(r.SubjectAge2) && !this._subjects.Contains(r.SubjectAge3) &&
          !this._subjects.Contains(r.SubjectAge4) && !this._subjects.Contains(r.SubjectAge5) &&
          !this._subjects.Contains(r.SubjectAgeYounger) && !this._localSubjects.Contains(r.SubjectInfo1) &&
          !this._localSubjects.Contains(r.SubjectInfo2);
      }
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      if (!this._subjects.Any() && !this._localSubjects.Any())
      {
        return query;
      }

      query = query.Where(this.GenerateExpression());

      return query;
    }

    public override IEnumerable<RaceData> Apply(MyContext db, IEnumerable<RaceData> query)
    {
      if (!this._subjects.Any() && !this._localSubjects.Any())
      {
        return query;
      }

      query = query.Where(this.GenerateExpression().Compile());

      return query;
    }

    public override IEnumerable<RaceHorseData> Apply(MyContext db, IEnumerable<RaceHorseData> query)
    {
      return query;
    }
  }

  class RaceAgeScriptKeyQuery : SimpleScriptKeyQuery
  {
    private readonly IReadOnlyList<short> _ages;

    public RaceAgeScriptKeyQuery(IReadOnlyList<short> ages)
    {
      this._ages = ages;
    }

    private Expression<Func<RaceData, bool>> GenerateExpression()
    {
      var unknown = Expression.Constant((short)RaceSubjectType.Unknown);

      for (var i = 2; i <= 5; i++)
      {
        var param = Expression.Parameter(typeof(RaceData), "x");
        var property = Expression.Convert(Expression.Property(param, "SubjectAge" + i), typeof(short));
        Expression result;

        // 指定の年齢が含まれていないレースを除外する
        if (!this._ages.Contains((short)i))
        {
          result = Expression.Equal(property, unknown);
          return Expression.Lambda<Func<RaceData, bool>>(result, param);
        }
      }

      return r => true;
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      if (!this._ages.Any())
      {
        return query;
      }

      query = query.Where(this.GenerateExpression());

      return query;
    }

    public override IEnumerable<RaceData> Apply(MyContext db, IEnumerable<RaceData> query)
    {
      if (!this._ages.Any())
      {
        return query;
      }

      query = query.Where(this.GenerateExpression().Compile());

      return query;
    }

    public override IEnumerable<RaceHorseData> Apply(MyContext db, IEnumerable<RaceHorseData> query)
    {
      return query;
    }
  }

  class DropoutScriptKeyQuery : SimpleScriptKeyQuery
  {
    private QueryType _type;
    private int _value;
    private int _maxValue;
    private int[] _values;
    protected bool IsReverse { get; set; }

    public DropoutScriptKeyQuery(QueryType type, int value, int maxValue, int[]? values)
    {
      this._type = type;
      this._value = value;
      this._maxValue = maxValue;
      this._values = values ?? Array.Empty<int>();
    }

    public static ScriptKeyQuery FromExpressionQuery(ExpressionScriptKeyQuery query)
    {
      return new DropoutScriptKeyQuery(query.Type, query.Value, query.MaxValue, query.Values);
    }

    private Expression<Func<int, bool>> GetExpression(int count)
    {
      var param = Expression.Parameter(typeof(int), "x");
      var property = Expression.Constant(count);
      var value = Expression.Constant(this._value);
      var maxValue = Expression.Constant(this._maxValue);
      var values = Expression.Constant(this._values.ToList());

      var lambda = ExpressionScriptKeyQuery.BuildNumericQuery<int>(this._type, param, property, value, maxValue, values);
      return lambda;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      var lambda = this.GetExpression(query.Count());
      if (lambda.Compile().Invoke(0) == !this.IsReverse)
      {
        return query.Where(q => false);
      }
      return query;
    }

    public override IEnumerable<RaceHorseData> Apply(MyContext db, IEnumerable<RaceHorseData> query)
    {
      var lambda = this.GetExpression(query.Count());
      if (lambda.Compile().Invoke(0) == !this.IsReverse)
      {
        return Enumerable.Empty<RaceHorseData>();
      }
      return query;
    }
  }

  class ResidueScriptKeyQuery : DropoutScriptKeyQuery
  {
    public ResidueScriptKeyQuery(QueryType type, int value, int maxValue, int[]? values) : base(type, value, maxValue, values)
    {
      base.IsReverse = true;
    }

    public static new ScriptKeyQuery FromExpressionQuery(ExpressionScriptKeyQuery query)
    {
      return new ResidueScriptKeyQuery(query.Type, query.Value, query.MaxValue, query.Values);
    }
  }
}
