using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
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
      var queries = new List<ScriptKeyQuery>();
      foreach (var q in this._keys.Split('|'))
      {
        bool AddQuery(string split, QueryType type)
        {
          if (q.Contains(split))
          {
            var data = q.Split(split);
            var query = this.GetQuery(type, data[0], data[1]);
            if (query != null)
            {
              queries!.Add(query);
            }
            return true;
          }
          return false;
        }

        var hr = true;
        hr = hr && !AddQuery("==", QueryType.Equals);
        hr = hr && !AddQuery("!=", QueryType.NotEquals);
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
          var key = this.GetKeyInfo(q);
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
            }
          }
        }
      }

      return queries;
    }

    private (QueryKey, QueryKeyAttribute?) GetKeyInfo(string scriptKey)
    {
      var key = _keyDefs.FirstOrDefault(k => k.Item2?.ScriptKey == scriptKey);
      if (key.Item2 != null)
      {
        return key;
      }
      return (QueryKey.Unknown, null);
    }

    private ScriptKeyQuery? GetQuery(QueryType type, string scriptKey, string value)
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
        if (value.Contains('-'))
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
    public abstract IQueryable<RaceData> Apply(IQueryable<RaceData> query);

    // public abstract IQueryable<RaceHorseData> Apply(IQueryable<RaceHorseData> query);
  }

  class RaceLambdaScriptKeyQuery : ScriptKeyQuery
  {
    private readonly Expression<Func<RaceData, bool>> _where;

    public RaceLambdaScriptKeyQuery(Expression<Func<RaceData, bool>> lambda)
    {
      this._where = lambda;
    }

    public override IQueryable<RaceData> Apply(IQueryable<RaceData> query)
    {
      return query.Where(this._where);
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

    public override IQueryable<RaceData> Apply(IQueryable<RaceData> query)
    {
      if (this.Key == QueryKey.Weather)
      {
        query = query.Where(this.BuildValuesQuery<RaceData, RaceCourseWeather>
          (nameof(RaceData.TrackWeather), this.Values.Select(v => (RaceCourseWeather)v)));
      }
      if (this.Key == QueryKey.Condition)
      {
        query = query.Where(this.BuildValuesQuery<RaceData, RaceCourseCondition>
          (nameof(RaceData.TrackCondition), this.Values.Select(v => (RaceCourseCondition)v)));
      }
      if (this.Key == QueryKey.Course)
      {
        query = query.Where(this.BuildValuesQuery<RaceData, RaceCourse>
          (nameof(RaceData.Course), this.Values.Select(v => (RaceCourse)v)));
      }
      if (this.Key == QueryKey.Ground)
      {
        query = query.Where(this.BuildValuesQuery<RaceData, TrackGround>
          (nameof(RaceData.TrackGround), this.Values.Select(v => (TrackGround)v)));
      }
      if (this.Key == QueryKey.Direction)
      {
        query = query.Where(this.BuildValuesQuery<RaceData, TrackCornerDirection>
          (nameof(RaceData.TrackCornerDirection), this.Values.Select(v => (TrackCornerDirection)v)));
      }
      if (this.Key == QueryKey.Distance)
      {
        query = query.Where(this.BuildNumericQuery<RaceData>(nameof(RaceData.Distance)));
      }
      if (this.Key == QueryKey.Month)
      {
        query = query.Where(r => this.Values.Contains(r.StartTime.Month));
      }
      if (this.Key == QueryKey.RaceName)
      {
        if (!string.IsNullOrEmpty(this.StringValue))
        {
          query = query.Where(this.BuildStringQuery<RaceData>(nameof(RaceData.Name)));
        }
      }
      return query;
    }

    private Expression<Func<T, bool>> BuildValuesQuery<T, V>(string propertyName, IEnumerable<V> values)
    {
      var param = Expression.Parameter(typeof(T), "x");
      var property = Expression.Property(param, propertyName);

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
      var param = Expression.Parameter(typeof(T), "x");
      var property = Expression.Property(param, propertyName);
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
    [EnumQueryKey("month")]
    Month,
    [StringQueryKey("name")]
    RaceName,
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

  class QueryKeyAttribute : Attribute
  {
    public string ScriptKey { get; }

    public QueryKeyAttribute(string key)
    {
      this.ScriptKey = key;
    }
  }

  class NumericQueryKeyAttribute : QueryKeyAttribute { public NumericQueryKeyAttribute(string key) : base(key) { } }

  class EnumQueryKeyAttribute : QueryKeyAttribute { public EnumQueryKeyAttribute(string key) : base(key) { } }

  class StringQueryKeyAttribute : QueryKeyAttribute { public StringQueryKeyAttribute(string key) : base(key) { } }
}
