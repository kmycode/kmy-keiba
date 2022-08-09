using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.ExNumber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  abstract class ScriptKeyQuery
  {
    public abstract IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query);

    public abstract IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query);

    public abstract IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query);

    public abstract IQueryable<ExternalNumberData> Apply(MyContext db, IQueryable<ExternalNumberData> query);

    private protected string GetScriptKey(QueryKey key)
    {
      var attribute = typeof(QueryKey).GetField(key.ToString())!.GetCustomAttributes(true).OfType<QueryKeyAttribute>().FirstOrDefault();
      return attribute?.ScriptKey ?? string.Empty;
    }
  }

  class DefaultLambdaScriptKeyQuery : ScriptKeyQuery
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
  }

  class RaceLambdaScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

  class HorseLambdaScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

  class SameRaceHorseScriptKeyQuery : DefaultLambdaScriptKeyQuery
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
      return query.Where(h => this._riders!.Contains(h.Key));
    }
  }

  class SameRaceRiderScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

  class SameRaceTrainerScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

  class SameRaceOwnerScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

  class HorseBelongsScriptKeyQuery : DefaultLambdaScriptKeyQuery
  {
    private readonly IReadOnlyList<HorseBelongs> _belongs;
    private readonly bool _isAlive;

    internal static IReadOnlyList<HorseBelongs> GetBelongs(string value, QueryType type)
    {
      if (value.StartsWith('@'))
      {
        value = value[1..];
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

  class RiderBelongsScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

  class TrainerBelongsScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

  class MemoScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      var targetKeys = this._data.Where(d =>
          d.Key == MemoTarget.Race || d.Key == MemoTarget.Course || d.Key == MemoTarget.Day)
        .ToArray();
      var horseKeys = this._data.Where(d =>
          d.Key == MemoTarget.Race || d.Key == MemoTarget.Course || d.Key == MemoTarget.Day ||
          d.Key == MemoTarget.Rider || d.Key == MemoTarget.Trainer || d.Key == MemoTarget.Owner ||
          d.Key == MemoTarget.Horse || d.Key == MemoTarget.Father || d.Key == MemoTarget.Mother ||
          d.Key == MemoTarget.MotherFather)
        .ToArray();

      if (!targetKeys.Any() || targetKeys.Length != horseKeys.Length)
      {
        return query;
      }

      var memo = Expression.Parameter(typeof(MemoData), "x");
      var target1 = targetKeys.ElementAtOrDefault(0).Key;
      var target2 = targetKeys.ElementAtOrDefault(1).Key;
      var target3 = targetKeys.ElementAtOrDefault(2).Key;

      var memoFilter =
        this._query.Apply(db, db.Memos!).Where(m => m.Target1 == target1 && m.Target2 == target2 && m.Target3 == target3);
      if (this._number != default)
      {
        memoFilter = memoFilter.Where(m => m.Number == this._number);
      }

      var i = 1;
      foreach (var key in targetKeys.Take(3))
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

      return query;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      var raceKeys = this._data.Where(d =>
          d.Key == MemoTarget.Race || d.Key == MemoTarget.Course || d.Key == MemoTarget.Day)
        .ToArray();
      var targetKeys = this._data.Where(d =>
          d.Key == MemoTarget.Race || d.Key == MemoTarget.Course || d.Key == MemoTarget.Day ||
          d.Key == MemoTarget.Rider || d.Key == MemoTarget.Trainer || d.Key == MemoTarget.Owner ||
          d.Key == MemoTarget.Horse || d.Key == MemoTarget.Father || d.Key == MemoTarget.Mother ||
          d.Key == MemoTarget.MotherFather)
        .ToArray();

      if (!targetKeys.Any() || raceKeys.Length == targetKeys.Length)
      {
        return query;
      }

      var memo = Expression.Parameter(typeof(MemoData), "x");
      var target1 = targetKeys.ElementAtOrDefault(0).Key;
      var target2 = targetKeys.ElementAtOrDefault(1).Key;
      var target3 = targetKeys.ElementAtOrDefault(2).Key;

      var memoFilter =
        this._query.Apply(db, db.Memos!).Where(m => m.Target1 == target1 && m.Target2 == target2 && m.Target3 == target3);
      if (this._number != default)
      {
        memoFilter = memoFilter.Where(m => m.Number == this._number);
      }

      var i = 1;
      foreach (var key in targetKeys.Take(3))
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
            .Select(r => new { Race = r, Day = r.RaceKey.Substring(0, 8), })
            .Join(memoFilter, r => r.Day, innerKey, (r, m) => r.Race);
        }
        else if (key.Key == MemoTarget.Race)
        {
          query = query.Join(memoFilter, r => r.RaceKey, innerKey, (r, m) => r);
        }
        else if (key.Key == MemoTarget.Rider)
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

      return query;
    }

    public override IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query)
    {
      return query;
    }
  }

  class BloodHorseScriptKeyQuery : DefaultLambdaScriptKeyQuery
  {
    private readonly QueryKey _scriptKey;
    private string? _code;
    private string? _key;
    private bool _isCheckedCode = false;
    private readonly bool _isSelfCode = false;
    private readonly short _horseNumber;
    private readonly string? _raceKey;

    public BloodHorseScriptKeyQuery(QueryKey scriptKey, string? code = null, string? key = null, bool isSelfCode = false, short horseNumber = 0, string? raceKey = null)
    {
      this._scriptKey = scriptKey;
      this._code = code;
      this._key = key;
      this._isCheckedCode = code != null;
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

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      if (this._code == null && !this._isCheckedCode)
      {
        if (this._horseNumber != default && this._raceKey != null)
        {
          var horse = db.RaceHorses!.FirstOrDefault(rh => rh.Number == this._horseNumber && rh.RaceKey == this._raceKey);
          if (horse != null)
          {
            this._key = horse.Key;
          }
        }

        if (!this._isSelfCode)
        {
          // 自分と同じ父を持つ馬を検索
          var born = db.BornHorses!.FirstOrDefault(h => h.Code == this._key);
          if (born != null)
          {
            this._code = this._scriptKey switch
            {
              QueryKey.Father => born.FatherBreedingCode,
              QueryKey.FatherFather => born.FFBreedingCode,
              QueryKey.FatherFatherFather => born.FFFBreedingCode,
              QueryKey.FatherFatherMother => born.FFMBreedingCode,
              QueryKey.FatherMother => born.FMBreedingCode,
              QueryKey.FatherMotherFather => born.FMFBreedingCode,
              QueryKey.FatherMotherMother => born.FMMBreedingCode,
              QueryKey.Mother => born.MotherBreedingCode,
              QueryKey.MotherFather => born.MFBreedingCode,
              QueryKey.MotherFatherFather => born.MFFBreedingCode,
              QueryKey.MotherFatherMother => born.MFMBreedingCode,
              QueryKey.MotherMother => born.MMBreedingCode,
              QueryKey.MotherMotherFather => born.MMFBreedingCode,
              QueryKey.MotherMotherMother => born.MMMBreedingCode,
              _ => null,
            };
          }
          else
          {
            var horse = db.Horses!.FirstOrDefault(h => h.Code == this._key);
            if (horse != null)
            {
              this._code = this._scriptKey switch
              {
                QueryKey.Father => horse.FatherBreedingCode,
                QueryKey.FatherFather => horse.FFBreedingCode,
                QueryKey.FatherFatherFather => horse.FFFBreedingCode,
                QueryKey.FatherFatherMother => horse.FFMBreedingCode,
                QueryKey.FatherMother => horse.FMBreedingCode,
                QueryKey.FatherMotherFather => horse.FMFBreedingCode,
                QueryKey.FatherMotherMother => horse.FMMBreedingCode,
                QueryKey.Mother => horse.MotherBreedingCode,
                QueryKey.MotherFather => horse.MFBreedingCode,
                QueryKey.MotherFatherFather => horse.MFFBreedingCode,
                QueryKey.MotherFatherMother => horse.MFMBreedingCode,
                QueryKey.MotherMother => horse.MMBreedingCode,
                QueryKey.MotherMotherFather => horse.MMFBreedingCode,
                QueryKey.MotherMotherMother => horse.MMMBreedingCode,
                _ => null,
              };
            }
          }
        }
        else
        {
          // 自分が父になっている馬を検索
          var data = db.HorseBloods!.FirstOrDefault(b => b.Code == this._key);
          if (data != null)
          {
            this._code = data.Key;
          }
        }

        this._isCheckedCode = true;
      }

      if (this._code != null)
      {
        Expression<Func<BornHorseData, bool>>? subject = this._scriptKey switch
        {
          QueryKey.Father => b => b.FatherBreedingCode == this._code,
          QueryKey.FatherFather => b => b.FFBreedingCode == this._code,
          QueryKey.FatherFatherFather => b => b.FFFBreedingCode == this._code,
          QueryKey.FatherFatherMother => b => b.FFMBreedingCode == this._code,
          QueryKey.FatherMother => b => b.FMBreedingCode == this._code,
          QueryKey.FatherMotherFather => b => b.FMFBreedingCode == this._code,
          QueryKey.FatherMotherMother => b => b.FMMBreedingCode == this._code,
          QueryKey.Mother => b => b.MotherBreedingCode == this._code,
          QueryKey.MotherFather => b => b.MFBreedingCode == this._code,
          QueryKey.MotherFatherFather => b => b.MFFBreedingCode == this._code,
          QueryKey.MotherFatherMother => b => b.MFMBreedingCode == this._code,
          QueryKey.MotherMother => b => b.MMBreedingCode == this._code,
          QueryKey.MotherMotherFather => b => b.MMFBreedingCode == this._code,
          QueryKey.MotherMotherMother => b => b.MMMBreedingCode == this._code,
          _ => null,
        };

        if (subject != null)
        {
          query = query.Join(db.BornHorses!.Where(subject), h => h.Key, b => b.Code, (h, b) => h);
        }
      }
      else
      {
        // すべてを焼き尽くす
        query = query.Where(r => false);
      }

      return query;
    }
  }

  class HorseBeforeRacesScriptKeyQuery : DefaultLambdaScriptKeyQuery
  {
    private readonly IReadOnlyList<ScriptKeyQuery> _queries;
    private readonly IReadOnlyList<ExpressionScriptKeyQuery> _diffQueries;
    private readonly IReadOnlyList<ExpressionScriptKeyQuery> _diffQueriesBetweenCurrent;
    private readonly int _beforeSize;
    private readonly int _compareTargetSize;
    private readonly RaceCountRule _countRule;

    internal enum RaceCountRule
    {
      All,
      AnywaysRun,
      Completely,
    }

    public HorseBeforeRacesScriptKeyQuery(RaceCountRule countRule, int beforeSize, int compareTargetSize, IReadOnlyList<ScriptKeyQuery> queries, IReadOnlyList<ExpressionScriptKeyQuery> diffQueries, IReadOnlyList<ExpressionScriptKeyQuery> diffQueriesBetweenCurrent)
    {
      this._countRule = countRule;
      this._beforeSize = beforeSize;
      this._compareTargetSize = compareTargetSize;
      this._queries = queries;

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
        var tmpr = horses.Join(races, rh => rh.RaceKey, r => r.Key, (rh, r) => new { Race = r, RaceHorse = rh, });

        Expression<Func<T, bool>> Compare<T>(IQueryable<T> obj, QueryType type, bool isRace, string propertyName, int value, string propertyPrefix = "Compare", bool isShort = true, bool isEnum = false)
        {
          var param = Expression.Parameter(typeof(T), "x");
          var before = Expression.Property(param, "Before");
          var beforeRace = Expression.Property(param, "BeforeRace");
          var current = Expression.Property(param, propertyPrefix);
          var currentRace = Expression.Property(param, propertyPrefix + "Race");

          var beforeObj = isRace ? beforeRace : before;
          var currentObj = isRace ? currentRace : current;
          Expression valueObj = Expression.Constant(value);
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
          beforeProperty = Expression.Add(beforeProperty, valueObj);

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
              case QueryKey.Weather:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.TrackWeather), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Condition:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.TrackCondition), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Course:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.Course), value, propertyPrefix: prefix, isShort: true, isEnum: true));
                break;
              case QueryKey.Ground:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, true, nameof(RaceData.TrackGround), value, propertyPrefix: prefix, isShort: true, isEnum: true));
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
              case QueryKey.Weight:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Weight), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.WeightDiff:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.WeightDiff), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.RiderWeight:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.RiderWeight), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Popular:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.Popular), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.Place:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.ResultOrder), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.ResultLength:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.ResultLength1), value, propertyPrefix: prefix, isShort: true));
                break;
              case QueryKey.RunningStyle:
                tmpQuery = tmpQuery.Where(Compare(tmpQuery, type, false, nameof(RaceHorseData.RunningStyle), value, propertyPrefix: prefix, isShort: true, isEnum: true));
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
                  var tmp = query
                    .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                    .Join(tmpr, q => new { q.RaceHorse.Key, q.RaceHorse.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, });
                  query = ApplyQueries(tmp).Select(t => t.Current);
                }
                break;
              case RaceCountRule.AnywaysRun:
                {
                  var tmp = query
                    .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                    .Join(tmpr, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, });
                  query = ApplyQueries(tmp).Select(t => t.Current);
                }
                break;
              case RaceCountRule.Completely:
                {
                  var tmp = query
                    .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                    .Join(tmpr, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, });
                  query = ApplyQueries(tmp).Select(t => t.Current);
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
                  var tmp = query
                    .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                    .Join(tmpr, q => new { q.RaceHorse.Key, q.RaceHorse.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, })
                    .Join(tmpr, q => new { q.Current.Key, q.Current.RaceCount, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCount + this._compareTargetSize), }, (q, h) => new { q.BeforeRace, q.Before, q.CurrentRace, q.Current, Compare = h.RaceHorse, CompareRace = h.Race, });
                  query = ApplyQueries(tmp).Select(t => t.Current);
                }
                break;
              case RaceCountRule.AnywaysRun:
                {
                  var tmp = query
                    .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                    .Join(tmpr, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, })
                    .Join(tmpr, q => new { q.Current.Key, RaceCount = q.Current.RaceCountWithinRunning, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunning + this._compareTargetSize), }, (q, h) => new { q.BeforeRace, q.Before, q.CurrentRace, q.Current, Compare = h.RaceHorse, CompareRace = h.Race, });
                  query = ApplyQueries(tmp).Select(t => t.Current);
                }
                break;
              case RaceCountRule.Completely:
                {
                  var tmp = query
                    .Join(db.Races!, q => q.RaceKey, r => r.Key, (q, r) => new { RaceHorse = q, Race = r, })
                    .Join(tmpr, q => new { q.RaceHorse.Key, RaceCount = q.RaceHorse.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._beforeSize), }, (q, h) => new { BeforeRace = h.Race, Before = h.RaceHorse, CurrentRace = q.Race, Current = q.RaceHorse, })
                    .Join(tmpr, q => new { q.Current.Key, RaceCount = q.Current.RaceCountWithinRunningCompletely, }, h => new { h.RaceHorse.Key, RaceCount = (short)(h.RaceHorse.RaceCountWithinRunningCompletely + this._compareTargetSize), }, (q, h) => new { q.BeforeRace, q.Before, q.CurrentRace, q.Current, Compare = h.RaceHorse, CompareRace = h.Race, });
                  query = ApplyQueries(tmp).Select(t => t.Current);
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

    public override string ToString()
    {
      var rule = this._countRule switch
      {
        RaceCountRule.AnywaysRun => "run",
        RaceCountRule.Completely => "complete",
        _ => "all",
      };
      var queries = this._queries.Select(q => q.ToString())
        .Concat(this._diffQueries.Select(q => q.ToString("$")))
        .Concat(this._diffQueriesBetweenCurrent.Select(q => q.ToString("$$")));
      return $"(before<{rule}>:{this._beforeSize},{this._compareTargetSize}){string.Join(';', queries)}";
    }
  }

  class TopHorsesScriptKeyQuery : DefaultLambdaScriptKeyQuery
  {
    private readonly IReadOnlyList<ScriptKeyQuery> _queries;

    public TopHorsesScriptKeyQuery(IReadOnlyList<ScriptKeyQuery> queries)
    {
      this._queries = queries;
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      IQueryable<RaceHorseData> horses = db.RaceHorses!;
      foreach (var q in this._queries)
      {
        horses = q.Apply(db, horses);
      }

      query = query.Join(horses, r => r.Key, rh => rh.RaceKey, (r, rh) => r);

      return query;
    }

    public override string ToString()
    {
      return $"(race){string.Join(';', this._queries)}";
    }
  }

  class HorseExternalNumberScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

    public override string ToString()
    {
      return $"ext:{this._configId}/:{this._pointQuery}";
    }
  }

  class ExpressionScriptKeyQuery : DefaultLambdaScriptKeyQuery
  {
    public QueryKey Key { get; }

    public QueryType Type { get; }

    public int Value { get; private set; }

    public int MaxValue { get; }

    public int[] Values { get; }

    public string? StringValue { get; }

    public ExpressionScriptKeyQuery(QueryKey key, QueryType type, int value, int maxValue)
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
    }

    public ExpressionScriptKeyQuery(QueryKey key, QueryType type, int value) : this(key, type, value, value)
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

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      switch (this.Key)
      {
        case QueryKey.RaceKey:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            query = query.Where(this.BuildStringQuery<RaceData>(nameof(RaceData.Key)));
          }
          break;
        case QueryKey.Weather:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackWeather), isEnum: true));
          break;
        case QueryKey.Condition:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackCondition), isEnum: true));
          break;
        case QueryKey.Course:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.Course), isEnum: true));
          break;
        case QueryKey.Ground:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackGround), isEnum: true));
          break;
        case QueryKey.Direction:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackCornerDirection), isEnum: true));
          break;
        case QueryKey.TrackType:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.TrackType), isEnum: true));
          break;
        case QueryKey.Distance:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.Distance)));
          break;
        case QueryKey.Day:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.StartTime), nameof(DateTime.Day), false));
          break;
        case QueryKey.Month:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.StartTime), nameof(DateTime.Month), false));
          break;
        case QueryKey.Year:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.StartTime), nameof(DateTime.Year), false));
          break;
        case QueryKey.Date:
          query = query.Where(this.BuildDateQuery<RaceData>(nameof(RaceData.StartTime)));
          break;
        case QueryKey.Hour:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.StartTime), nameof(DateTime.Hour), false));
          break;
        case QueryKey.Nichiji:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.Nichiji)));
          break;
        case QueryKey.RaceNumber:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.CourseRaceNumber)));
          break;
        case QueryKey.RaceName:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            query = query.Where(this.BuildStringQuery<RaceData>(nameof(RaceData.Name)));
          }
          break;
        case QueryKey.GradeId:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.GradeId)));
          break;
        case QueryKey.SubjectAge2:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.SubjectAge2), isEnum: true));
          break;
        case QueryKey.SubjectAge3:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.SubjectAge3), isEnum: true));
          break;
        case QueryKey.SubjectAge4:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.SubjectAge4), isEnum: true));
          break;
        case QueryKey.SubjectAge5:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.SubjectAge5), isEnum: true));
          break;
        case QueryKey.Grade:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.Grade), isEnum: true));
          break;
        case QueryKey.RiderWeightRule:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.RiderWeight), isEnum: true));
          break;
        case QueryKey.AreaRule:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.Area), isEnum: true));
          break;
        case QueryKey.SexRule:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.Sex), isEnum: true));
          break;
        case QueryKey.CrossRule:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.Cross), isEnum: true));
          break;
        case QueryKey.PrizeMoney1:
          query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.PrizeMoney1), isShort: false));
          break;
      }
      return query;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      switch (this.Key)
      {
        case QueryKey.HorseKey:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            query = query.Where(this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.Key)));
          }
          break;
        case QueryKey.Age:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Age)));
          break;
        case QueryKey.Sex:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Sex), isEnum: true));
          break;
        case QueryKey.HorseType:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Type), isEnum: true));
          break;
        case QueryKey.Color:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Color), isEnum: true));
          break;
        case QueryKey.HorseNumber:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Number)));
          break;
        case QueryKey.FrameNumber:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.FrameNumber)));
          break;
        case QueryKey.RiderCode:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            query = query.Where(this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.RiderCode)));
          }
          break;
        case QueryKey.TrainerCode:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            query = query.Where(this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.TrainerCode)));
          }
          break;
        case QueryKey.OwnerCode:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            query = query.Where(this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.OwnerCode)));
          }
          break;
        case QueryKey.Place:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.ResultOrder)));
          break;
        case QueryKey.ResultLength:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.ResultLength1)));
          break;
        case QueryKey.Abnormal:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.AbnormalResult), isEnum: true));
          break;
        case QueryKey.Popular:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Popular)));
          break;
        case QueryKey.ResultTime:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.ResultTimeValue)));
          break;
        case QueryKey.CornerPlace1:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.FirstCornerOrder)));
          break;
        case QueryKey.CornerPlace2:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.SecondCornerOrder)));
          break;
        case QueryKey.CornerPlace3:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.ThirdCornerOrder)));
          break;
        case QueryKey.CornerPlace4:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.FourthCornerOrder)));
          break;
        case QueryKey.Weight:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Weight)));
          break;
        case QueryKey.WeightDiff:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.WeightDiff)));
          break;
        case QueryKey.RiderWeight:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.RiderWeight)));
          break;
        case QueryKey.Odds:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.Odds)));
          break;
        case QueryKey.PlaceOddsMax:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.PlaceOddsMax)));
          break;
        case QueryKey.PlaceOddsMin:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.PlaceOddsMin)));
          break;
        case QueryKey.A3HTime:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.AfterThirdHalongTimeValue)));
          break;
        case QueryKey.RunningStyle:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.RunningStyle), isEnum: true));
          break;
        case QueryKey.PreviousRaceDays:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.PreviousRaceDays)));
          break;
        case QueryKey.HorseName:
          query = query.Where(this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.Name)));
          break;
        case QueryKey.RiderName:
          query = query.Where(this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.RiderName)));
          break;
        case QueryKey.TrainerName:
          query = query.Where(this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.TrainerName)));
          break;
        case QueryKey.OwnerName:
          query = query.Where(this.BuildStringQuery<RaceHorseData>(nameof(RaceHorseData.OwnerName)));
          break;
      }
      return query;
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

    [Obsolete]
    private Expression<Func<T, bool>> BuildEnumValuesQuery<T, V>(string propertyName, IEnumerable<V> values)
    {
      return this.BuildEnumValuesQuery<T, V>(propertyName, null, values);
    }

    [Obsolete]
    private Expression<Func<T, bool>> BuildEnumValuesQuery<T, V>(string propertyName, string? propertyName2, IEnumerable<V> values)
    {
      var param = Expression.Parameter(typeof(T), "x");
      var property = Expression.Property(param, propertyName);
      if (propertyName2 != null)
      {
        property = Expression.Property(property, propertyName2);
      }

      var valuesExp = Expression.Constant(values.ToList());

      if (values.Any())
      {
        if (values.Skip(1).Any())
        {
          if (this.Type == QueryType.Contains)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.Call(valuesExp, "Contains", null, property), param);
          }
          if (this.Type == QueryType.Excepts)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Call(valuesExp, "Contains", null, property), Expression.Constant(false)), param);
          }
        }
        else
        {
          var valueExp = Expression.Constant(values.First());
          if (this.Type == QueryType.Contains || this.Type == QueryType.Equals)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.Equal(property, valueExp), param);
          }
          if (this.Type == QueryType.Excepts || this.Type == QueryType.NotEquals)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.NotEqual(property, valueExp), param);
          }
          if (this.Type == QueryType.GreaterThan)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(
              Expression.Convert(property, typeof(short)), Expression.Convert(valueExp, typeof(short))), param);
          }
          if (this.Type == QueryType.GreaterThanOrEqual)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(
              Expression.Convert(property, typeof(short)), Expression.Convert(valueExp, typeof(short))), param);
          }
          if (this.Type == QueryType.LessThan)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.LessThan(
              Expression.Convert(property, typeof(short)), Expression.Convert(valueExp, typeof(short))), param);
          }
          if (this.Type == QueryType.LessThanOrEqual)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(
              Expression.Convert(property, typeof(short)), Expression.Convert(valueExp, typeof(short))), param);
          }
        }
      }

      return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), param);
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
      if (isEnum)
      {
        property = Expression.Convert(property, isShort ? typeof(short) : typeof(int));
      }
      var value = Expression.Constant(isDouble ? (double)this.Value : isShort ? (short)this.Value : (object)this.Value);
      var maxValue = Expression.Constant(isDouble ? (double)this.MaxValue : isShort ? (short)this.MaxValue : (object)this.MaxValue);

      var values = Expression.Constant(isShort ? this.Values.Select(v => (short)v).ToList() : this.Values.ToList());

      return this.BuildNumericQuery<T>(param, property, value, maxValue, values);
    }

    private Expression<Func<T, bool>> BuildNumericQuery<T>(ParameterExpression param, Expression property, Expression value, Expression maxValue, Expression values)
    {
      if (this.Type == QueryType.Equals)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Equal(property, value), param);
      }
      if (this.Type == QueryType.NotEquals)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.NotEqual(property, value), param);
      }
      if (this.Type == QueryType.GreaterThan)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(property, value), param);
      }
      if (this.Type == QueryType.GreaterThanOrEqual)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(property, value), param);
      }
      if (this.Type == QueryType.LessThan)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.LessThan(property, value), param);
      }
      if (this.Type == QueryType.LessThanOrEqual)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(property, value), param);
      }

      if (Type == QueryType.Range)
      {
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.GreaterThanOrEqual(property, value), Expression.LessThan(property, maxValue)),
          param);
      }
      if (Type == QueryType.RangeOrEqual)
      {
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.GreaterThanOrEqual(property, value), Expression.LessThanOrEqual(property, maxValue)),
          param);
      }
      if (Type == QueryType.NotRange)
      {
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.LessThan(property, value), Expression.GreaterThanOrEqual(property, maxValue)),
          param);
      }
      if (Type == QueryType.NotRangeOrEqual)
      {
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.LessThan(property, value), Expression.GreaterThan(property, maxValue)),
          param);
      }

      if (this.Type == QueryType.Contains)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Call(values, "Contains", null, property), param);
      }
      if (this.Type == QueryType.Excepts)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Call(values, "Contains", null, property), Expression.Constant(false)), param);
      }

      throw new NotSupportedException();
    }

    private Expression<Func<T, bool>> BuildStringQuery<T>(string propertyName)
    {
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

    public override string ToString()
    {
      return this.ToString(string.Empty);
    }

    public string ToString(string rightPrefix)
    {
      var split = this.Type switch
      {
        QueryType.NotEquals => "<>",
        QueryType.LessThanOrEqual => "<=",
        QueryType.GreaterThanOrEqual => ">=",
        QueryType.Contains => "@=",
        QueryType.StartsWith => "@<",
        QueryType.EndsWith => "@>",
        QueryType.Equals => "=",
        QueryType.LessThan => "<",
        QueryType.GreaterThan => ">",
        _ => string.Empty,
      };
      var left = base.GetScriptKey(this.Key);
      string right;

      if (this.Values.Length > 1)
      {
        var max = this.Values.Max();
        var min = this.Values.Min();
        var isConsective = true;
        for (var i = min; i <= max; i++)
        {
          if (!this.Values.Contains(i))
          {
            isConsective = false;
            break;
          }
        }
        if (isConsective)
        {
          right = $"{min}-{max}";
        }
        else
        {
          right = string.Join(',', this.Values);
        }
      }
      else
      {
        if (this.StringValue != null)
        {
          right = this.StringValue;
        }
        else
        {
          right = this.Value.ToString();
        }
      }

      return $"{left}{split}{rightPrefix}{right}";
    }
  }

  class RaceSubjectScriptKeyQuery : DefaultLambdaScriptKeyQuery
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

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      if (!this._subjects.Any() && !this._localSubjects.Any())
      {
        return query;
      }

      if (this._type == QueryType.Equals || this._type == QueryType.Contains)
      {
        query = query.Where(r => this._subjects.Contains(r.SubjectAge2) || this._subjects.Contains(r.SubjectAge3) ||
          this._subjects.Contains(r.SubjectAge4) || this._subjects.Contains(r.SubjectAge5) ||
          this._subjects.Contains(r.SubjectAgeYounger) || this._localSubjects.Contains(r.SubjectInfo1) ||
          this._localSubjects.Contains(r.SubjectInfo2));
      }
      else
      {
        query = query.Where(r => !this._subjects.Contains(r.SubjectAge2) && !this._subjects.Contains(r.SubjectAge3) &&
          !this._subjects.Contains(r.SubjectAge4) && !this._subjects.Contains(r.SubjectAge5) &&
          !this._subjects.Contains(r.SubjectAgeYounger) && !this._localSubjects.Contains(r.SubjectInfo1) &&
          !this._localSubjects.Contains(r.SubjectInfo2));
      }
      return query;
    }
  }
}
