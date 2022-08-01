using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Generic
{
  class ScriptKeysReader
  {
    private readonly string _keys;

    private static readonly IReadOnlyList<(QueryKey, QueryKeyAttribute?)> _keyDefs = Enum.GetValues(typeof(QueryKey))
        .OfType<QueryKey>()
        .Select(k => (k, typeof(QueryKey).GetField(k.ToString())!.GetCustomAttributes(true).OfType<QueryKeyAttribute>().FirstOrDefault()))
        .ToArray();

    public ScriptKeysReader(string keys)
    {
      this._keys = keys;
    }

    public IReadOnlyList<ScriptKeyQuery> GetQueries(RaceData? race, RaceHorseData? horse = null)
    {
      return GetQueries(this._keys, race, horse);
    }

    public static IReadOnlyList<ScriptKeyQuery> GetQueries(string keys, RaceData? race = null, RaceHorseData? horse = null)
    {
      var queries = new List<ScriptKeyQuery>();
      foreach (var q in keys.Split('|'))
      {
        bool AddQuery(string split, QueryType type)
        {
          if (q.Contains(split))
          {
            var data = q.Split(split);
            var query = GetQuery(type, data[0], data[1]);
            if (query != null)
            {
              queries!.Add(query);
            }
            return true;
          }
          return false;
        }

        bool AddMemoQuery()
        {
          if (!q!.StartsWith("memo"))
          {
            return false;
          }

          var pointTarget = (MemoTarget)(short)-1;
          var numberTarget = (MemoTarget)(short)-2;

          var parameters = q.Split('/')
            .Skip(1)
            .Select(d => d.Split(':'))
            .Select(d =>
            {
              var key = d[0] switch
              {
                "race" => MemoTarget.Race,
                "course" => MemoTarget.Course,
                "distance" => MemoTarget.Distance,
                "day" => MemoTarget.Day,
                "direction" => MemoTarget.Direction,
                "grades" => MemoTarget.Grades,
                "horse" => MemoTarget.Horse,
                "rider" => MemoTarget.Rider,
                "trainer" => MemoTarget.Trainer,
                "owner" => MemoTarget.Owner,
                "f" => MemoTarget.Father,
                "point" => pointTarget,
                "" => pointTarget,
                "number" => numberTarget,
                _ => MemoTarget.Unknown,
              };
              return (Key: key, Value: d.ElementAtOrDefault(1) ?? string.Empty);
            })
            .Where(d => d.Key != MemoTarget.Unknown)
            .OrderBy(d => d.Key)
            .ToDictionary(d => d.Key, d => d.Value);

          if (!parameters.TryGetValue(pointTarget, out _) || parameters.Count < 2)
          {
            return false;
          }

          var query = GetQueries(parameters[pointTarget], race);
          if (query.Any())
          {
            queries.Add(new MemoScriptKeyQuery(parameters, query.First()));
            return true;
          }

          return false;
        }

        bool AddBloodQuery()
        {
          var splits = q!.Split(':');
          var key = splits[0] switch
          {
            "f" => QueryKey.Father,
            "ff" => QueryKey.FatherFather,
            "fff" => QueryKey.FatherFatherFather,
            "ffm" => QueryKey.FatherFatherMother,
            "fm" => QueryKey.FatherMother,
            "fmf" => QueryKey.FatherMotherFather,
            "fmm" => QueryKey.FatherMotherMother,
            "m" => QueryKey.Mother,
            "mf" => QueryKey.MotherFather,
            "mff" => QueryKey.MotherFatherFather,
            "mfm" => QueryKey.MotherFatherMother,
            "mm" => QueryKey.MotherMother,
            "mmf" => QueryKey.MotherMotherFather,
            "mmm" => QueryKey.MotherMotherMother,
            _ => QueryKey.Unknown,
          };
          if (key == QueryKey.Unknown)
          {
            return false;
          }

          var value = splits.ElementAtOrDefault(1) ?? horse?.Key;
          if (value == null)
          {
            return false;
          }

          var isSelfMode = false;
          if (value.StartsWith('@'))
          {
            isSelfMode = true;
            value = value[1..];
            if (string.IsNullOrEmpty(value))
            {
              // f:@ という描き方を想定
              value = horse?.Key;
            }
          }

          string? bkey = value;
          string? bcode = value;

          if (bkey != null || bcode != null)
          {
            if (bkey?.Length != 10) bkey = null;
            if (bcode?.Length != 8) bcode = null;
          }

          if (bkey != null || bcode != null)
          {
            queries.Add(new BloodHorseScriptKeyQuery(key, key: bkey, code: bcode, isSelfCode: isSelfMode));
            return true;
          }

          // 処理済として以降の処理をスキップする
          if (horse == null)
          {
            return true;
          }

          return true;
        }

        bool AddPlaceHorseQuery()
        {
          if (!q!.StartsWith("(place"))
          {
            return false;
          }

          var endIndex = q.IndexOf(')');
          if (endIndex < 0)
          {
            return false;
          }

          var placeScriptKey = q[1..endIndex];
          if (string.IsNullOrEmpty(placeScriptKey))
          {
            return false;
          }

          var placeQueries = GetQueries(placeScriptKey, race, horse);
          if (placeQueries == null || !placeQueries.Any())
          {
            return false;
          }

          var keys = q.Substring(endIndex + 1).Replace(';', '|');
          var qs = GetQueries(keys, race, horse);

          queries.Add(new TopHorsesScriptKeyQuery(placeQueries[0], qs));
          return true;
        }

        var hr = true;
        hr = hr && !AddMemoQuery();
        hr = hr && !AddBloodQuery();
        hr = hr && !AddPlaceHorseQuery();
        hr = hr && !AddQuery("<>", QueryType.NotEquals);
        hr = hr && !AddQuery("<=", QueryType.LessThanOrEqual);
        hr = hr && !AddQuery(">=", QueryType.GreaterThanOrEqual);
        hr = hr && !AddQuery("<", QueryType.LessThan);
        hr = hr && !AddQuery(">", QueryType.GreaterThan);
        hr = hr && !AddQuery("=", QueryType.Equals);
        hr = hr && !AddQuery("@<", QueryType.Contains);
        hr = hr && !AddQuery("@:", QueryType.StartsWith);
        hr = hr && !AddQuery("@;", QueryType.EndsWith);

        if (hr)
        {
          var key = GetKeyInfo(q);

          // 条件式指定がないときのデフォルト値を指定
          if (key.Item1 != QueryKey.Unknown && key.Item2 != null)
          {
            if (race != null)
            {
              switch (key.Item1)
              {
                case QueryKey.Weather:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.TrackWeather == race.TrackWeather));
                  break;
                case QueryKey.Condition:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.TrackCondition == race.TrackCondition));
                  break;
                case QueryKey.Distance:
                  var diff = race.Course <= RaceCourse.CentralMaxValue ?
                    ApplicationConfiguration.Current.Value.NearDistanceDiffCentral :
                    ApplicationConfiguration.Current.Value.NearDistanceDiffLocal;
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Distance >= race.Distance - diff && r.Distance <= race.Distance + diff));
                  break;
                case QueryKey.Direction:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.TrackCornerDirection == race.TrackCornerDirection));
                  break;
                case QueryKey.Day:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.StartTime.Day == race.StartTime.Day));
                  break;
                case QueryKey.Month:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.StartTime.Month == race.StartTime.Month));
                  break;
                case QueryKey.Year:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.StartTime.Year == race.StartTime.Year));
                  break;
                case QueryKey.Subject:
                  if (race.Course <= RaceCourse.CentralMaxValue)
                  {
                    queries.Add(new RaceLambdaScriptKeyQuery(r =>
                                             r.SubjectName == race.SubjectName &&
                                             r.SubjectAge2 == race.SubjectAge2 &&
                                             r.SubjectAge3 == race.SubjectAge3 &&
                                             r.SubjectAge4 == race.SubjectAge4 &&
                                             r.SubjectAge5 == race.SubjectAge5 &&
                                             r.SubjectAgeYounger == race.SubjectAgeYounger));
                  }
                  else if (race.Course >= RaceCourse.LocalMinValue && !string.IsNullOrEmpty(race.SubjectDisplayInfo))
                  {
                    queries.Add(new RaceLambdaScriptKeyQuery(r => r.SubjectDisplayInfo == race.SubjectDisplayInfo));
                  }
                  break;
                case QueryKey.GradeId:
                  if (race.GradeId != default)
                  {
                    queries.Add(new RaceLambdaScriptKeyQuery(r => r.GradeId == race.GradeId));
                  }
                  break;
                case QueryKey.Grades:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => race.Grade == RaceGrade.Grade1 || race.Grade == RaceGrade.Grade2 || race.Grade == RaceGrade.Grade3 ||
                                   race.Grade == RaceGrade.LocalGrade1 || race.Grade == RaceGrade.LocalGrade2 || race.Grade == RaceGrade.LocalGrade3));
                  break;
                case QueryKey.Course:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Course == race.Course));
                  break;
                case QueryKey.RaceKey:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Key == race.Key));
                  break;
              }
            }

            if (horse != null)
            {
              switch (key.Item1)
              {
                case QueryKey.Color:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Color == horse.Color));
                  break;
                case QueryKey.Age:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Age == horse.Age));
                  break;
                case QueryKey.Sex:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Sex == horse.Sex));
                  break;
                case QueryKey.HorseType:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Type == horse.Type));
                  break;
                case QueryKey.HorseNumber:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Number == horse.Number));
                  break;
                case QueryKey.FrameNumber:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.FrameNumber == horse.FrameNumber));
                  break;
                case QueryKey.HorseKey:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Key == horse.Key));
                  break;
                case QueryKey.Place:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.ResultOrder == horse.ResultOrder));
                  break;
                case QueryKey.RiderCode:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.RiderCode == horse.RiderCode));
                  break;
                case QueryKey.TrainerCode:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.TrainerCode == horse.TrainerCode));
                  break;
                case QueryKey.OwnerCode:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.OwnerCode == horse.OwnerCode));
                  break;
                case QueryKey.Abnormal:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.AbnormalResult == horse.AbnormalResult));
                  break;
                case QueryKey.Odds:
                  {
                    var (min, max) = AnalysisUtil.GetOddsRange(horse.Odds);
                    queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Odds >= min && rh.Odds < max));
                  }
                  break;
                case QueryKey.PlaceOddsMax:
                  {
                    var (min, max) = AnalysisUtil.GetOddsRange(horse.Odds);
                    queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.PlaceOddsMax >= min && rh.PlaceOddsMax < max));
                  }
                  break;
                case QueryKey.PlaceOddsMin:
                  {
                    var (min, max) = AnalysisUtil.GetOddsRange(horse.Odds);
                    queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.PlaceOddsMin >= min && rh.PlaceOddsMin < max));
                  }
                  break;
              }
            }
          }
        }
      }

      return queries;
    }

    private static (QueryKey, QueryKeyAttribute?) GetKeyInfo(string scriptKey)
    {
      var key = _keyDefs.FirstOrDefault(k => k.Item2?.ScriptKey == scriptKey);
      if (key.Item2 != null)
      {
        return key;
      }
      return (QueryKey.Unknown, null);
    }

    private static ScriptKeyQuery? GetQuery(QueryType type, string scriptKey, string value)
    {
      var key = GetKeyInfo(scriptKey);
      if (key.Item1 == QueryKey.Unknown)
      {
        return null;
      }

      if (key.Item2 is EnumQueryKeyAttribute)
      {
        if (value.Contains(','))
        {
          var values = value.Split(',').Select(v =>
          {
            if (int.TryParse(v, out var val))
              return val;
            return -3000;
          }).Where(v => v != -3000).ToArray();
          if (values.Length > 0)
          {
            // 1,2,3
            return new ExpressionScriptKeyQuery(key.Item1, type == QueryType.Equals ? QueryType.Contains : QueryType.Excepts, values);
          }
        }
        else if (value.IndexOf('-', 1) > 0)  // マイナス記号で始まっていた場合にそなえて
        {
          var spliter = value.IndexOf('-', 1);
          var data = new string[] { value[..spliter], value[(spliter + 1)..], };
          if (int.TryParse(data[0], out var min) && int.TryParse(data[1], out var max))
          {
            // 2-4
            return new ExpressionScriptKeyQuery(key.Item1, type == QueryType.Equals ? QueryType.Contains : QueryType.Excepts, Enumerable.Range(min, max - min + 1).ToArray());
          }
        }
        else
        {
          if (int.TryParse(value, out var val))
          {
            return new ExpressionScriptKeyQuery(key.Item1, type, val);
          }
        }
      }

      if (key.Item2 is NumericQueryKeyAttribute)
      {
        if (value.Contains(','))
        {
          var values = value.Split(',').Select(v =>
          {
            if (int.TryParse(v, out var val))
              return val;
            return -12445;
          }).Where(v => v != -12445).ToArray();
          if (values.Length > 0)
          {
            // 1,2,3
            return new ExpressionScriptKeyQuery(key.Item1, type == QueryType.Equals ? QueryType.Contains : QueryType.Excepts, values);
          }
        }
        else if (value.IndexOf('-', 1) > 0)  // マイナス記号で始まっていた場合にそなえて
        {
          // テストケース
          // 30-50    -20-40    -3--1
          var spliter = value.IndexOf('-', 1);
          var data = new string[] { value[..spliter], value[(spliter + 1)..], };
          if (int.TryParse(data[0], out var min) && int.TryParse(data[1], out var max))
          {
            // 2-4
            return new ExpressionScriptKeyQuery(key.Item1, type == QueryType.Equals ? QueryType.RangeOrEqual : QueryType.NotRangeOrEqual, min, max);
          }
        }
        else
        {
          if (int.TryParse(value, out var val))
          {
            return new ExpressionScriptKeyQuery(key.Item1, type, val);
          }
        }
      }

      if (key.Item2 is StringQueryKeyAttribute)
      {
        return new ExpressionScriptKeyQuery(key.Item1, type, value);
      }

      return null;
    }
  }

  abstract class ScriptKeyQuery
  {
    public abstract IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query);

    public abstract IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query);

    public abstract IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query);
  }

  abstract class DefaultLambdaScriptKeyQuery : ScriptKeyQuery
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

  class MemoScriptKeyQuery : ScriptKeyQuery
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
          d.Key == MemoTarget.Horse || d.Key == MemoTarget.Father)
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
          d.Key == MemoTarget.Horse || d.Key == MemoTarget.Father)
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
      }

      return query;
    }

    public override IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query)
    {
      return query;
    }
  }

  class BloodHorseScriptKeyQuery : ScriptKeyQuery
  {
    private readonly QueryKey _scriptKey;
    private string? _code;
    private readonly string? _key;
    private bool _isCheckedCode = false;
    private readonly bool _isSelfCode = false;

    public BloodHorseScriptKeyQuery(QueryKey scriptKey, string? code = null, string? key = null, bool isSelfCode = false)
    {
      this._scriptKey = scriptKey;
      this._code = code;
      this._key = key;
      this._isCheckedCode = code != null;
      this._isSelfCode = isSelfCode;
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

  class TopHorsesScriptKeyQuery : ScriptKeyQuery
  {
    private readonly ScriptKeyQuery _placeQuery;
    private readonly IReadOnlyList<ScriptKeyQuery> _queries;

    public TopHorsesScriptKeyQuery(ScriptKeyQuery placeQuery, IReadOnlyList<ScriptKeyQuery> queries)
    {
      this._placeQuery = placeQuery;
      this._queries = queries;
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      IQueryable<RaceHorseData> horses = db.RaceHorses!;
      horses = this._placeQuery.Apply(db, horses);
      foreach (var q in this._queries)
      {
        horses = q.Apply(db, horses);
      }

      query = query.Join(horses, r => r.Key, rh => rh.RaceKey, (r, rh) => r);

      /*
      query = query.GroupJoin(horses, r => r.Key, rh => rh.RaceKey, (r, rhs) => new { Race = r, Horses = rhs, })
        .Where(d => d.Horses.Any())
        .Select(d => d.Race);
      */

      return query;
    }

    public override IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query)
    {
      return query;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
      return query;
    }
  }

  class ExpressionScriptKeyQuery : ScriptKeyQuery
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
      this.NormalizeData();
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
      this.NormalizeData();
    }

    public ExpressionScriptKeyQuery(QueryKey key, QueryType type, string value)
    {
      this.Key = key;
      this.Type = QueryType.Equals;
      this.Values = Array.Empty<int>();
      this.StringValue = value;
      this.NormalizeData();
    }

    private void NormalizeData()
    {
      // ミリ秒と比較をおこなうので
      if (this.Key == QueryKey.ResultTime || this.Key == QueryKey.A3HTime)
      {
        this.Value *= 100;
        if (this.Values != null)
        {
          for (var i = 0; i < this.Values.Length; i++)
          {
            this.Values[i] = this.Values[i] * 100;
          }
        }
      }
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
          query = query.Where(this.BuildEnumValuesQuery<RaceData, RaceCourseWeather>
            (nameof(RaceData.TrackWeather), this.Values.Select(v => (RaceCourseWeather)v)));
          break;
        case QueryKey.Condition:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, RaceCourseCondition>
            (nameof(RaceData.TrackCondition), this.Values.Select(v => (RaceCourseCondition)v)));
          break;
        case QueryKey.Course:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, RaceCourse>
            (nameof(RaceData.Course), this.Values.Select(v => (RaceCourse)v)));
          break;
        case QueryKey.Ground:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, TrackGround>
            (nameof(RaceData.TrackGround), this.Values.Select(v => (TrackGround)v)));
          break;
        case QueryKey.Direction:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, TrackCornerDirection>
            (nameof(RaceData.TrackCornerDirection), this.Values.Select(v => (TrackCornerDirection)v)));
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
        case QueryKey.RaceName:
          if (!string.IsNullOrEmpty(this.StringValue))
          {
            query = query.Where(this.BuildStringQuery<RaceData>(nameof(RaceData.Name)));
          }
          break;
        case QueryKey.GradeId:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, short>
            (nameof(RaceData.GradeId), this.Values.Select(v => (short)v)));
          break;
        case QueryKey.SubjectAge2:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, RaceSubjectType>
            (nameof(RaceData.SubjectAge2), this.Values.Select(v => (RaceSubjectType)v)));
          break;
        case QueryKey.SubjectAge3:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, RaceSubjectType>
            (nameof(RaceData.SubjectAge3), this.Values.Select(v => (RaceSubjectType)v)));
          break;
        case QueryKey.SubjectAge4:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, RaceSubjectType>
            (nameof(RaceData.SubjectAge4), this.Values.Select(v => (RaceSubjectType)v)));
          break;
        case QueryKey.SubjectAge5:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, RaceSubjectType>
            (nameof(RaceData.SubjectAge5), this.Values.Select(v => (RaceSubjectType)v)));
          break;
        case QueryKey.Grade:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, RaceGrade>
            (nameof(RaceData.Grade), this.Values.Select(v => (RaceGrade)v)));
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
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>(nameof(RaceHorseData.Age), this.Values.Select(v => (short)v)));
          break;
        case QueryKey.Sex:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, HorseSex>
            (nameof(RaceHorseData.Sex), this.Values.Select(v => (HorseSex)v)));
          break;
        case QueryKey.HorseType:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, HorseType>
            (nameof(RaceHorseData.Type), this.Values.Select(v => (HorseType)v)));
          break;
        case QueryKey.Color:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, HorseBodyColor>
            (nameof(RaceHorseData.Color), this.Values.Select(v => (HorseBodyColor)v)));
          break;
        case QueryKey.HorseNumber:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>(nameof(RaceHorseData.Number), this.Values.Select(v => (short)v)));
          break;
        case QueryKey.FrameNumber:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>(nameof(RaceHorseData.FrameNumber), this.Values.Select(v => (short)v)));
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
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>(nameof(RaceHorseData.ResultOrder), this.Values.Select(v => (short)v)));
          break;
        case QueryKey.ResultLength:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>
            (nameof(RaceHorseData.ResultLength1), this.Values.Select(v => (short)v)));
          break;
        case QueryKey.Abnormal:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, RaceAbnormality>
            (nameof(RaceHorseData.AbnormalResult), this.Values.Select(v => (RaceAbnormality)v)));
          break;
        case QueryKey.Popular:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>(nameof(RaceHorseData.Popular), this.Values.Select(v => (short)v)));
          break;
        case QueryKey.ResultTime:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.ResultTime), nameof(TimeSpan.TotalMilliseconds), isDouble: true));
          break;
        case QueryKey.CornerPlace1:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>(nameof(RaceHorseData.FirstCornerOrder), this.Values.Select(v => (short)v)));
          break;
        case QueryKey.CornerPlace2:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>(nameof(RaceHorseData.SecondCornerOrder), this.Values.Select(v => (short)v)));
          break;
        case QueryKey.CornerPlace3:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>(nameof(RaceHorseData.ThirdCornerOrder), this.Values.Select(v => (short)v)));
          break;
        case QueryKey.CornerPlace4:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, short>(nameof(RaceHorseData.FourthCornerOrder), this.Values.Select(v => (short)v)));
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
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.AfterThirdHalongTime), nameof(TimeSpan.TotalMilliseconds), isDouble: true));
          break;
        case QueryKey.RunningStyle:
          query = query.Where(this.BuildEnumValuesQuery<RaceHorseData, RunningStyle>
            (nameof(RaceHorseData.RunningStyle), this.Values.Select(v => (RunningStyle)v)));
          break;
        case QueryKey.PreviousRaceDays:
          query = query.Where(this.BuildNumericQuery<RaceHorseData>(nameof(RaceHorseData.PreviousRaceDays)));
          break;
      }
      return query;
    }

    public override IQueryable<MemoData> Apply(MyContext db, IQueryable<MemoData> query)
    {
      switch (this.Key)
      {
        case QueryKey.Point:
          query = query.Where(this.BuildEnumValuesQuery<MemoData, short>(nameof(MemoData.Point), this.Values.Select(v => (short)v)));
          break;
      }
      return query;
    }

    private Expression<Func<T, bool>> BuildEnumValuesQuery<T, V>(string propertyName, IEnumerable<V> values)
    {
      return this.BuildEnumValuesQuery<T, V>(propertyName, null, values);
    }

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
            return Expression.Lambda<Func<T, bool>>(Expression.IsFalse(Expression.Call(valuesExp, "Contains", null, property)), param);
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
            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(property, valueExp), param);
          }
          if (this.Type == QueryType.GreaterThanOrEqual)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(property, valueExp), param);
          }
          if (this.Type == QueryType.LessThan)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.LessThan(property, valueExp), param);
          }
          if (this.Type == QueryType.LessThanOrEqual)
          {
            return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(property, valueExp), param);
          }
        }
      }

      return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), param);
    }

    private Expression<Func<T, bool>> BuildNumericQuery<T>(string propertyName, bool isShort = true, bool isDouble = false)
    {
      return this.BuildNumericQuery<T>(propertyName, null, isShort, isDouble);
    }

    private Expression<Func<T, bool>> BuildNumericQuery<T>(string propertyName, string? propertyName2, bool isShort = true, bool isDouble = false)
    {
      var param = Expression.Parameter(typeof(T), "x");
      var property = Expression.Property(param, propertyName);
      if (propertyName2 != null)
      {
        property = Expression.Property(property, propertyName2);
      }
      var value = Expression.Constant(isDouble ? (object)(double)this.Value : isShort ? (object)(short)this.Value : (object)this.Value);

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

      var maxValue = Expression.Constant(isDouble ? (object)(double)this.MaxValue : isShort ? (object)(short)this.MaxValue : (object)this.MaxValue);
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

      var valuesExp = Expression.Constant(this.Values.ToList());
      if (this.Type == QueryType.Contains)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Call(valuesExp, "Contains", null, property), param);
      }
      if (this.Type == QueryType.Excepts)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.IsFalse(Expression.Call(valuesExp, "Contains", null, property)), param);
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
        return Expression.Lambda<Func<T, bool>>(Expression.Call(value, "Contains", null, property), param);
      }
      if (this.Type == QueryType.StartsWith)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Call(value, "StartsWith", null, property), param);
      }
      if (this.Type == QueryType.EndsWith)
      {
        return Expression.Lambda<Func<T, bool>>(Expression.Call(value, "EndsWith", null, property), param);
      }

      throw new NotSupportedException();
    }
  }

  enum QueryKey
  {
    Unknown,
    [StringQueryKey("race")]
    RaceKey,
    [EnumQueryKey("weather")]
    Weather,
    [EnumQueryKey("condition")]
    Condition,
    [EnumQueryKey("course")]
    Course,
    [EnumQueryKey("ground")]
    Ground,
    [NumericQueryKey("distance")]
    Distance,
    [EnumQueryKey("direction")]
    Direction,
    [NumericQueryKey("day")]
    Day,
    [NumericQueryKey("month")]
    Month,
    [NumericQueryKey("year")]
    Year,
    [StringQueryKey("name")]
    RaceName,
    [QueryKey("subject")]
    Subject,
    [EnumQueryKey("subjectage2")]
    SubjectAge2,
    [EnumQueryKey("subjectage3")]
    SubjectAge3,
    [EnumQueryKey("subjectage4")]
    SubjectAge4,
    [EnumQueryKey("subjectage5")]
    SubjectAge5,
    [EnumQueryKey("grade")]
    Grade,
    [QueryKey("gradeid")]
    GradeId,
    [QueryKey("grades")]
    Grades,

    [StringQueryKey("horse")]
    HorseKey,
    [EnumQueryKey("age")]
    Age,
    [EnumQueryKey("sex")]
    Sex,
    [EnumQueryKey("horsetype")]
    HorseType,
    [EnumQueryKey("color")]
    Color,
    [EnumQueryKey("horsenumber")]
    HorseNumber,
    [EnumQueryKey("framenumber")]
    FrameNumber,
    [StringQueryKey("rider")]
    RiderCode,
    [EnumQueryKey("place")]
    Place,
    [EnumQueryKey("resultlength")]
    ResultLength,
    [EnumQueryKey("abnormal")]
    Abnormal,
    [EnumQueryKey("popular")]
    Popular,
    [NumericQueryKey("resulttime")]
    ResultTime,
    [EnumQueryKey("corner1")]
    CornerPlace1,
    [EnumQueryKey("corner2")]
    CornerPlace2,
    [EnumQueryKey("corner3")]
    CornerPlace3,
    [EnumQueryKey("corner4")]
    CornerPlace4,
    [NumericQueryKey("weight")]
    Weight,
    [NumericQueryKey("weightdiff")]
    WeightDiff,
    [NumericQueryKey("riderweight")]
    RiderWeight,
    [StringQueryKey("trainer")]
    TrainerCode,
    [StringQueryKey("owner")]
    OwnerCode,
    [NumericQueryKey("odds")]
    Odds,
    [NumericQueryKey("placeoddsmin")]
    PlaceOddsMin,
    [NumericQueryKey("placeoddsmax")]
    PlaceOddsMax,
    [NumericQueryKey("a3htime")]
    A3HTime,
    [EnumQueryKey("runningstyle")]
    RunningStyle,
    [NumericQueryKey("prevdays")]
    PreviousRaceDays,

    [StringQueryKey("f")]
    Father,
    [StringQueryKey("ff")]
    FatherFather,
    [StringQueryKey("fff")]
    FatherFatherFather,
    [StringQueryKey("ffm")]
    FatherFatherMother,
    [StringQueryKey("fm")]
    FatherMother,
    [StringQueryKey("fmf")]
    FatherMotherFather,
    [StringQueryKey("fmm")]
    FatherMotherMother,
    [StringQueryKey("m")]
    Mother,
    [StringQueryKey("mf")]
    MotherFather,
    [StringQueryKey("mff")]
    MotherFatherFather,
    [StringQueryKey("mfm")]
    MotherFatherMother,
    [StringQueryKey("mm")]
    MotherMother,
    [StringQueryKey("mmf")]
    MotherMotherFather,
    [StringQueryKey("mmm")]
    MotherMotherMother,

    [EnumQueryKey("point")]
    Point,
  }

  enum QueryType
  {
    Contains,
    Excepts,
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Range,
    RangeOrEqual,
    NotRange,
    NotRangeOrEqual,
    StartsWith,
    EndsWith,
  }

  enum QueryTarget
  {
    Unknown,
    Race,
    RaceHorse,
  }

  class QueryKeyAttribute : Attribute
  {
    public string ScriptKey { get; }

    public QueryTarget Target { get; }

    public QueryKeyAttribute(string key)
    {
      this.ScriptKey = key;
    }

    public QueryKeyAttribute(string key, QueryTarget target)
    {
      this.ScriptKey = key;
      this.Target = target;
    }
  }

  class NumericQueryKeyAttribute : QueryKeyAttribute { public NumericQueryKeyAttribute(string key) : base(key) { } public NumericQueryKeyAttribute(string key, QueryTarget target) : base(key, target) { } }

  class EnumQueryKeyAttribute : QueryKeyAttribute { public EnumQueryKeyAttribute(string key) : base(key) { } public EnumQueryKeyAttribute(string key, QueryTarget target) : base(key, target) { } }

  class StringQueryKeyAttribute : QueryKeyAttribute { public StringQueryKeyAttribute(string key) : base(key) { } public StringQueryKeyAttribute(string key, QueryTarget target) : base(key, target) { } }
}
