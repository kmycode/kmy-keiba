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

    public IReadOnlyList<ScriptKeyQuery> GetQueries(RaceData race)
    {
      return GetQueries(this._keys, race);
    }

    public static IReadOnlyList<ScriptKeyQuery> GetQueries(string keys, RaceData race, RaceHorseData? horse = null)
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
          var splits = q!.Split('=');
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

          var bkey = horse?.Key;
          var bcode = splits.ElementAtOrDefault(1);

          if (bkey != null || bcode != null)
          {
            queries.Add(new BloodHorseScriptKeyQuery(key, code: bcode, key: bkey));
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
          if (key.Item1 != QueryKey.Unknown && key.Item2 != null)
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
        else if (value.Contains('-'))
        {
          var data = value.Split('-');
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
        else if (value.Contains('-'))
        {
          var data = value.Split('-');
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
          d.Key == MemoTarget.Race || d.Key == MemoTarget.Course || d.Key == MemoTarget.Day ||
          d.Key == MemoTarget.Distance || d.Key == MemoTarget.Direction || d.Key == MemoTarget.Grades)
        .ToArray();

      if (!targetKeys.Any())
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

    public BloodHorseScriptKeyQuery(QueryKey scriptKey, string? code = null, string? key = null)
    {
      this._scriptKey = scriptKey;
      this._code = code;
      this._key = key;
      this._isCheckedCode = code != null;
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
        var born = db.BornHorses!.FirstOrDefault(h => h.Code == this._key);
        if (born != null)
        {
          this._code = this._scriptKey switch
          {
            QueryKey.Father => born.FatherBreedingCode,
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
              _ => null,
            };
          }
        }

        this._isCheckedCode = true;
      }

      if (this._code != null)
      {
        Expression<Func<BornHorseData, bool>>? subject = this._scriptKey switch
        {
          QueryKey.Father => b => b.FatherBreedingCode == this._code,
          _ => null,
        };

        if (subject != null)
        {
          query = query.Join(db.BornHorses!.Where(subject), h => h.Key, b => b.Code, (h, b) => h);
        }
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

    public int Value { get; }

    public int MaxValue { get; }

    public int[] Values { get; }

    public string? StringValue { get; }

    public ExpressionScriptKeyQuery(QueryKey key, QueryType type, int value, int maxValue)
    {
      this.Key = key;
      this.Type = type;
      this.Value = value;
      this.Values = new int[] { value, };
      this.MaxValue = maxValue;
    }

    public ExpressionScriptKeyQuery(QueryKey key, QueryType type, int value) : this(key, type, value, default(int))
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
      this.Type = QueryType.Equals;
      this.Values = Array.Empty<int>();
      this.StringValue = value;
    }

    public override IQueryable<RaceData> Apply(MyContext db, IQueryable<RaceData> query)
    {
      switch (this.Key)
      {
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
        case QueryKey.Grade:
          query = query.Where(this.BuildEnumValuesQuery<RaceData, RaceGrade>
            (nameof(RaceData.Grade), this.Values.Select(v => (RaceGrade)v)));
          break;
      }
      return query;
    }

    public override IQueryable<RaceHorseData> Apply(MyContext db, IQueryable<RaceHorseData> query)
    {
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

    private Expression<Func<T, bool>> BuildNumericQuery<T>(string propertyName, bool isShort = true)
    {
      return this.BuildNumericQuery<T>(propertyName, null, isShort);
    }

    private Expression<Func<T, bool>> BuildNumericQuery<T>(string propertyName, string? propertyName2, bool isShort = true)
    {
      var param = Expression.Parameter(typeof(T), "x");
      var property = Expression.Property(param, propertyName);
      if (propertyName2 != null)
      {
        property = Expression.Property(property, propertyName2);
      }
      var value = Expression.Constant(isShort ? (object)(short)this.Value : (object)this.Value);

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
        var maxValue = Expression.Constant(isShort ? (object)(short)this.MaxValue : (object)this.MaxValue);
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.GreaterThanOrEqual(property, value), Expression.LessThan(property, maxValue)),
          param);
      }
      if (Type == QueryType.RangeOrEqual)
      {
        var maxValue = Expression.Constant(isShort ? (object)(short)this.MaxValue : (object)this.MaxValue);
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.GreaterThanOrEqual(property, value), Expression.LessThanOrEqual(property, maxValue)),
          param);
      }
      if (Type == QueryType.NotRange)
      {
        var maxValue = Expression.Constant(isShort ? (object)(short)this.MaxValue : (object)this.MaxValue);
        return Expression.Lambda<Func<T, bool>>(
          Expression.And(Expression.LessThan(property, value), Expression.GreaterThanOrEqual(property, maxValue)),
          param);
      }
      if (Type == QueryType.NotRangeOrEqual)
      {
        var maxValue = Expression.Constant(isShort ? (object)(short)this.MaxValue : (object)this.MaxValue);
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
    [EnumQueryKey("grade")]
    Grade,
    [QueryKey("gradeid")]
    GradeId,
    [QueryKey("grades")]
    Grades,

    [EnumQueryKey("place")]
    Place,

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
