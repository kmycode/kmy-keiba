using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.ExNumber;
using KmyKeiba.Models.Race.Memo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static KmyKeiba.Models.Race.Finder.HorseBeforeRacesCountScriptKeyQuery;

namespace KmyKeiba.Models.Race.Finder
{
  class ScriptKeysParseResult
  {
    public IReadOnlyList<ScriptKeyQuery> Queries { get; }

    public IReadOnlyList<ExpressionScriptKeyQuery> DiffQueries { get; }

    public IReadOnlyList<ExpressionScriptKeyQuery> DiffQueriesBetweenCurrent { get; }

    public QueryKey GroupKey { get; }

    public int Limit { get; }

    public int Offset { get; }

    public bool IsContainsFutureRaces { get; }

    public bool IsCurrentRaceOnly { get; }

    public bool IsRealtimeResult { get; }

    public bool IsExpandedResult { get; }

    public ScriptKeysMemoGroupInfo? MemoGroupInfo { get; set; }

    public bool HasJrdbQuery { get; init; }

    public bool HasExtraQuery { get; init; }

    public ScriptKeysParseResult(IReadOnlyList<ScriptKeyQuery> queries, QueryKey groupKey = QueryKey.Unknown, int limit = 0, int offset = 0, IReadOnlyList<ExpressionScriptKeyQuery>? diffQueries = null, IReadOnlyList<ExpressionScriptKeyQuery>? diffQueriesBetweenCurrent = null, bool isContainsFutureRaces = false, bool isCurrentOnly = false, bool isRealtimeResult = false, bool isExpandedResult = false)
    {
      this.Queries = queries;
      this.GroupKey = groupKey;
      this.Limit = limit;
      this.Offset = offset;
      this.DiffQueries = diffQueries ?? Array.Empty<ExpressionScriptKeyQuery>();
      this.DiffQueriesBetweenCurrent = diffQueriesBetweenCurrent ?? Array.Empty<ExpressionScriptKeyQuery>();
      this.IsContainsFutureRaces = isContainsFutureRaces;
      this.IsCurrentRaceOnly = isCurrentOnly;
      this.IsRealtimeResult = isRealtimeResult;
      this.IsExpandedResult = isExpandedResult;
    }
  }

  class ScriptKeysMemoGroupInfo
  {
    public MemoTarget Target1 { get; set; }

    public MemoTarget Target2 { get; set; }

    public MemoTarget Target3 { get; set; }

    public short MemoNumber { get; set; }
  }

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

    public ScriptKeysParseResult GetQueries(RaceData? race, RaceHorseData? horse = null, RaceHorseAnalyzer? horseAnalyzer = null)
    {
      return GetQueries(this._keys, race, horse, horseAnalyzer);
    }

    public static ScriptKeysParseResult GetQueries(string keys, RaceData? race = null, RaceHorseData? horse = null, RaceHorseAnalyzer? horseAnalyzer = null)
    {
      var groupKey = QueryKey.Unknown;
      var limit = 0;
      var offset = 0;
      var isContainsFutureRaces = false;
      var isCurrentRaceOnly = false;
      var isRealtimeResult = false;
      var isExpandedResult = false;
      ScriptKeysMemoGroupInfo? memoGroupInfo = null;
      var hasJrdbKeys = false;
      var hasExtraKeys = false;

      var queries = new List<ScriptKeyQuery>();
      var diffQueries = new List<ExpressionScriptKeyQuery>();
      var diffQueriesBetweenCurrent = new List<ExpressionScriptKeyQuery>();
      foreach (var qu in keys.Split('|'))
      {
        var q = qu;

        bool AddQuery(string split, QueryType type)
        {
          if (q.Contains(split))
          {
            var data = q.Split(split);
            (ScriptKeyQuery?, QueryKeyAttribute?) queryRaw;

            if (data[1].StartsWith("$$"))
            {
              data[1] = data[1][2..];
              queryRaw = GetQuery(type, data[0], data[1], race, horse);
              var query = queryRaw.Item1;
              if (query != null && query is ExpressionScriptKeyQuery exp)
              {
                diffQueriesBetweenCurrent!.Add(exp);
              }
            }
            else if (data[1].StartsWith('$'))
            {
              data[1] = data[1][1..];
              queryRaw = GetQuery(type, data[0], data[1], race, horse);
              var query = queryRaw.Item1;
              if (query != null && query is ExpressionScriptKeyQuery exp)
              {
                diffQueries!.Add(exp);
              }
            }
            else
            {
              queryRaw = GetQuery(type, data[0], data[1], race, horse);
              var query = queryRaw.Item1;
              if (query is ExpressionScriptKeyQuery exp)
              {
                var key = GetKeyInfo(data[0]);
                if (key.Item1 == QueryKey.Dropout)
                {
                  query = DropoutScriptKeyQuery.FromExpressionQuery(exp);
                }
                else if (key.Item1 == QueryKey.Residue)
                {
                  query = ResidueScriptKeyQuery.FromExpressionQuery(exp);
                }
              }
              if (query != null)
              {
                queries!.Add(query);
              }
            }

            if (queryRaw.Item2?.Target == QueryTarget.JrdbRaceHorse)
            {
              hasJrdbKeys = true;
            }
            if (queryRaw.Item2?.Target == QueryTarget.RaceHorseExtra)
            {
              hasExtraKeys = true;
            }

            return true;
          }
          return false;
        }

        bool AddMemoQuery()
        {
          var isGroup = false;
          if (q.StartsWith("[group]memo"))
          {
            q = q[7..];
            isGroup = true;
          }

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
              var key = MemoUtil.GetMemoTarget(d[0]);
              return (Key: key, Value: d.ElementAtOrDefault(1) ?? string.Empty);
            })
            .Where(d => d.Key != MemoTarget.Unknown)
            .OrderBy(d => d.Key)
            .ToDictionary(d => d.Key, d => d.Value);

          if (!isGroup && parameters.Count < 2)
          {
            return false;
          }

          if (isGroup)
          {
            var ks = parameters.Select(p => p.Key).Where(k => (int)k > 0).OrderBy(k => k).ToArray();
            parameters.TryGetValue(numberTarget, out var numVal);
            short.TryParse(numVal, out var num);
            memoGroupInfo = new ScriptKeysMemoGroupInfo
            {
              Target1 = ks.ElementAtOrDefault(0),
              Target2 = ks.ElementAtOrDefault(1),
              Target3 = ks.ElementAtOrDefault(2),
              MemoNumber = num,
            };
            return true;
          }

          if (parameters.ContainsKey(pointTarget))
          {
            var query = GetQueries(parameters[pointTarget], race);
            if (query.Queries.Any())
            {
              queries.Add(new MemoScriptKeyQuery(parameters, query.Queries.First()));
              return true;
            }
          }
          else
          {
            // メモのあるレースのみを検索（ポイントは比較しない）
            queries.Add(new MemoScriptKeyQuery(parameters, new SimpleScriptKeyQuery()));
            return true;
          }

          return false;
        }

        bool AddExternalNumberQuery()
        {
          if (!q!.StartsWith("ext:"))
          {
            return false;
          }

          var endIndex = q.IndexOf('/');
          if (endIndex <= 4)
          {
            return false;
          }

          if (!uint.TryParse(q.Substring(4, endIndex - 4), out var configId))
          {
            return false;
          }

          var pointQuery = q.Split('/')[1];
          if (!pointQuery.StartsWith(":point"))
          {
            return false;
          }
          pointQuery = pointQuery[1..];

          var query = GetQueries(pointQuery, race);
          if (query.Queries.Any())
          {
            queries!.Add(new HorseExternalNumberScriptKeyQuery(configId, query.Queries.First()));
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
              value = horse?.Key ?? string.Empty;
            }
          }

          var horseNumber = default(short);
          if (value.StartsWith('#'))
          {
            if (value != "#")
            {
              var num = value.Split('#')[1];
              short.TryParse(num, out horseNumber);
            }
            else
            {
              // f:#の場合、現在の馬
              value = horse?.Key ?? string.Empty;
            }
          }

          var values = value.Split(',');

          if (values.Any())
          {
            queries.Add(new BloodHorseScriptKeyQuery(key, codes: values, isSelfCode: isSelfMode, horseNumber: horseNumber, raceKey: race?.Key));
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
          if (!q!.StartsWith("(race"))
          {
            return false;
          }

          var prefixEnd = q.IndexOf(')');
          if (prefixEnd < 5)
          {
            return false;
          }

          var min = 0;
          var max = 100;
          var minRate = 0;
          var maxRate = 100;

          if (prefixEnd > 6 && q.ElementAtOrDefault(5) == ':')
          {
            var optionsRaw = q.Substring(6, prefixEnd - 6);
            var options = optionsRaw.Split('<');
            foreach (var option in options.Skip(1).Select(o => o.Split('>')).Where(o => o.Length == 2))
            {
              if (option[0] == "eq")
              {
                int.TryParse(option[1], out min);
                max = min;
              }
              else if (option[0] == "min")
              {
                int.TryParse(option[1], out min);
              }
              else if (option[0] == "max")
              {
                int.TryParse(option[1], out max);
              }
              else if (option[0] == "minr")
              {
                int.TryParse(option[1], out minRate);
              }
              else if (option[0] == "maxr")
              {
                int.TryParse(option[1], out maxRate);
              }
            }
          }

          var keys = q.Substring(prefixEnd + 1).Replace(';', '|');
          var qs = GetQueries(keys, race, horse);

          queries.Add(new TopHorsesScriptKeyQuery(qs.Queries, qs.HasExtraQuery, qs.HasJrdbQuery, min, max, minRate, maxRate));
          return true;
        }

        bool AddBeforeRacesQuery()
        {
          if (!q!.StartsWith("(before"))
          {
            return false;
          }

          var endIndex = q.IndexOf(')');
          if (endIndex < 0)
          {
            return false;
          }

          var tagEndIndex = q[..endIndex].IndexOf('>');

          var countRule = HorseBeforeRacesScriptKeyQuery.RaceCountRule.All;
          if (endIndex > 8 && q.StartsWith("(before<"))
          {
            if (tagEndIndex < 0)
            {
              return false;
            }
            var countRuleStr = q.Substring(8, tagEndIndex - 8);
            countRule = countRuleStr switch
            {
              "run" => HorseBeforeRacesScriptKeyQuery.RaceCountRule.AnywaysRun,
              "complete" => HorseBeforeRacesScriptKeyQuery.RaceCountRule.Completely,
              _ => HorseBeforeRacesScriptKeyQuery.RaceCountRule.All,
            };
          }
          else
          {
            tagEndIndex = -1;
          }

          var beforeSize = 1;
          var compareTargetSize = 0;
          var isCountQuery = false;
          var countQueryCount = 0;
          var countQueryRule = RaceCountComparationRule.Within;
          if (endIndex > 8 && (tagEndIndex > 0 || q.StartsWith("(before:")))
          {
            var paramStartIndex = tagEndIndex > 0 ? tagEndIndex + 2 : 8;
            if (endIndex > paramStartIndex)
            {
              var beforeSizeStr = q.Substring(paramStartIndex, endIndex - paramStartIndex);
              if (beforeSizeStr.Contains(','))
              {
                if (!int.TryParse(beforeSizeStr.Split(',')[0], out beforeSize)) beforeSize = 1;
                if (!int.TryParse(beforeSizeStr.Split(',')[1], out compareTargetSize)) compareTargetSize = 0;

                var countQueryRaw = beforeSizeStr.Split(',')[1];
                if (countQueryRaw.StartsWith(":count"))
                {
                  isCountQuery = true;
                  if (countQueryRaw.StartsWith(":count>="))
                  {
                    countQueryRule = RaceCountComparationRule.MorePast;
                  }
                  int.TryParse(countQueryRaw[8..], out countQueryCount);
                }
              }
              else
              {
                if (!int.TryParse(beforeSizeStr, out beforeSize))
                {
                  beforeSize = 1;
                }
              }
            }
          }

          var keys = q.Substring(endIndex + 1).Replace(';', '|').Replace('\\', '\\');
          var qs = GetQueries(keys, race, horse, horseAnalyzer);

          if (isCountQuery)
          {
            queries!.Add(new HorseBeforeRacesCountScriptKeyQuery(qs.Queries, countQueryRule, countQueryCount, qs.HasJrdbQuery, qs.HasExtraQuery));
          }
          else
          {
            queries!.Add(new HorseBeforeRacesScriptKeyQuery(countRule, beforeSize, compareTargetSize, qs.Queries, qs.DiffQueries, qs.DiffQueriesBetweenCurrent, qs.HasJrdbQuery, qs.HasExtraQuery));
          }
          return true;
        }

        bool CheckAttributes()
        {
          if (q.StartsWith("[group]"))
          {
            if (q.StartsWith("[group]memo"))
            {
              return false;
            }
            q = q[7..];
            var key = GetKeyInfo(q);
            groupKey = key.Item1;
            return true;
          }
          if (q.StartsWith("[limit]") && int.TryParse(q[7..], out var lim))
          {
            limit = lim;
            return true;
          }
          if (q.StartsWith("[offset]") && int.TryParse(q[7..], out var off))
          {
            offset = off;
            return true;
          }
          if (q.StartsWith("[future]"))
          {
            isContainsFutureRaces = true;
            return true;
          }
          if (q.StartsWith("[expand]"))
          {
            isExpandedResult = true;
            return true;
          }
          if (q.StartsWith("[currentonly]"))
          {
            isCurrentRaceOnly = true;
            return true;
          }

          return false;
        }

        bool CheckForCurrentRaceItems()
        {
          if (q.Contains("#") && race != null)
          {
            var horseNumberStr = q.Split('#').ElementAtOrDefault(1) ?? string.Empty;
            var r = short.TryParse(horseNumberStr, out var horseNumber);

            if (!r && !q.EndsWith('#'))
            {
              return false;
            }

            var key = GetKeyInfo(q.Split('#')[0]);
            switch (key.Item1)
            {
              case QueryKey.RiderCode:
              case QueryKey.RiderName:
                queries!.Add(new SameRaceRiderScriptKeyQuery(race.Key, horseNumber));
                break;
              case QueryKey.TrainerCode:
              case QueryKey.TrainerName:
                queries!.Add(new SameRaceTrainerScriptKeyQuery(race.Key, horseNumber));
                break;
              case QueryKey.OwnerCode:
              case QueryKey.OwnerName:
                queries!.Add(new SameRaceOwnerScriptKeyQuery(race.Key, horseNumber));
                break;
              case QueryKey.HorseKey:
              case QueryKey.HorseName:
                queries!.Add(new SameRaceHorseScriptKeyQuery(race.Key, horseNumber));
                break;
              default:
                return false;
            }
            return true;
          }

          return false;
        }

        bool AddBelongsQuery()
        {
          QueryType type;
          string spliter;
          if (q!.Contains('='))
          {
            type = QueryType.Equals;
            spliter = "=";
          }
          else if (q!.Contains("<>"))
          {
            type = QueryType.NotEquals;
            spliter = "<>";
          }
          else
          {
            return false;
          }
          var value = q!.Split(spliter)[1];

          var key = GetKeyInfo(q!.Split(spliter)[0]);
          if (key.Item1 == QueryKey.HorseBelongs)
          {
            queries!.Add(new HorseBelongsScriptKeyQuery(value, type));
            return true;
          }
          else if (key.Item1 == QueryKey.RiderBelongs)
          {
            queries!.Add(new RiderBelongsScriptKeyQuery(value, type));
            return true;
          }
          else if (key.Item1 == QueryKey.TrainerBelongs)
          {
            queries!.Add(new TrainerBelongsScriptKeyQuery(value, type));
            return true;
          }
          return false;
        }

        bool AddRaceSubjectQuery()
        {
          if (!q!.StartsWith("subject"))
          {
            return false;
          }

          var type = QueryType.Equals;
          var split = "=";
          if (q.Contains("<>"))
          {
            type = QueryType.NotEquals;
            split = "<>";
          }
          else if (!q.Contains('='))
          {
            return false;
          }

          var value = q.Split(split)[1];
          var values = value.Split(',');

          var centrals = new List<RaceSubjectType>();
          var locals = new List<string>();
          foreach (var v in values)
          {
            if (short.TryParse(v, out var num))
            {
              centrals.Add((RaceSubjectType)num);
            }
            else
            {
              locals.Add(v);
            }
          }

          if (centrals.Any() || locals.Any())
          {
            queries!.Add(new RaceSubjectScriptKeyQuery(type, centrals, locals));
            return true;
          }

          return false;
        }

        bool AddRaceAgeSubjectQuery()
        {
          if (!q!.StartsWith("subjectage"))
          {
            return false;
          }

          var type = QueryType.Equals;
          var split = "=";
          if (q.Contains("<>"))
          {
            type = QueryType.NotEquals;
            split = "<>";
          }
          else if (!q.Contains('='))
          {
            return false;
          }

          var value = q.Split(split)[1];
          var values = value.Split(',');

          var ages = new List<short>();
          foreach (var v in values)
          {
            if (short.TryParse(v, out var num))
            {
              ages.Add(num);
            }
          }

          if (ages.Any())
          {
            if (type == QueryType.NotEquals)
            {
              ages = Enumerable.Range(2, 4).Select(v => (short)v).Except(ages).ToList();
            }
            queries!.Add(new RaceAgeScriptKeyQuery(ages));
            return true;
          }

          return false;
        }

        var hr = true;
        hr = hr && !CheckAttributes();
        hr = hr && !CheckForCurrentRaceItems();
        hr = hr && !AddMemoQuery();
        hr = hr && !AddExternalNumberQuery();
        hr = hr && !AddBloodQuery();
        hr = hr && !AddPlaceHorseQuery();
        hr = hr && !AddBeforeRacesQuery();
        hr = hr && !AddBelongsQuery();
        hr = hr && !AddRaceAgeSubjectQuery();
        hr = hr && !AddRaceSubjectQuery();
        hr = hr && !AddQuery("<>", QueryType.NotEquals);
        hr = hr && !AddQuery("<=", QueryType.LessThanOrEqual);
        hr = hr && !AddQuery(">=", QueryType.GreaterThanOrEqual);
        hr = hr && !AddQuery("@=", QueryType.Contains);
        hr = hr && !AddQuery("@<", QueryType.StartsWith);
        hr = hr && !AddQuery("@>", QueryType.EndsWith);
        hr = hr && !AddQuery("=", QueryType.Equals);
        hr = hr && !AddQuery("<", QueryType.LessThan);
        hr = hr && !AddQuery(">", QueryType.GreaterThan);

        if (hr)
        {
          if (q.EndsWith('#'))
          {
            q = q[..(q.Length - 1)];
          }

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
                case QueryKey.TrackType:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.TrackType == race.TrackType));
                  break;
                case QueryKey.Direction:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.TrackCornerDirection == race.TrackCornerDirection));
                  break;
                case QueryKey.Ground:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.TrackGround == race.TrackGround));
                  break;
                case QueryKey.TrackOption:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.TrackOption == race.TrackOption));
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
                case QueryKey.Nichiji:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Nichiji == race.Nichiji));
                  break;
                case QueryKey.RaceNumber:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.CourseRaceNumber == race.CourseRaceNumber));
                  break;
                case QueryKey.Subject:
                  if (race.Course <= RaceCourse.CentralMaxValue)
                  {
                    queries.Add(new RaceLambdaScriptKeyQuery(r =>
                                             r.SubjectDisplayInfo == race.SubjectDisplayInfo &&
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
                case QueryKey.Grade:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Grade == race.Grade));
                  break;
                case QueryKey.Grades:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => race.Grade == RaceGrade.Grade1 || race.Grade == RaceGrade.Grade2 || race.Grade == RaceGrade.Grade3 ||
                                   race.Grade == RaceGrade.LocalGrade1 || race.Grade == RaceGrade.LocalGrade2 || race.Grade == RaceGrade.LocalGrade3));
                  break;
                case QueryKey.SexRule:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Sex == race.Sex));
                  break;
                case QueryKey.RiderWeightRule:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.RiderWeight == race.RiderWeight));
                  break;
                case QueryKey.AreaRule:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Area == race.Area));
                  break;
                case QueryKey.CrossRule:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Cross == race.Cross));
                  break;
                case QueryKey.Course:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Course == race.Course));
                  break;
                case QueryKey.RaceKey:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Key == race.Key));
                  break;
                case QueryKey.RaceName:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.Name == race.Name));
                  break;
                case QueryKey.Date:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.StartTime.Date == DateTime.Today));
                  break;
                case QueryKey.Hour:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.StartTime.Hour == DateTime.Now.Hour));
                  break;
                case QueryKey.PrizeMoney1:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.PrizeMoney1 == race.PrizeMoney1));
                  break;
                case QueryKey.HorsesCount:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.HorsesCount == race.HorsesCount));
                  break;
                case QueryKey.GoalHorsesCount:
                  queries.Add(new RaceLambdaScriptKeyQuery(r => r.ResultHorsesCount == race.ResultHorsesCount));
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
                case QueryKey.Popular:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Popular == horse.Popular));
                  isRealtimeResult = true;
                  break;
                case QueryKey.HorseKey:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Key == horse.Key));
                  break;
                case QueryKey.Place:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.ResultOrder == horse.ResultOrder));
                  break;
                case QueryKey.GoalPlace:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.GoalOrder == horse.GoalOrder));
                  break;
                case QueryKey.RunningStyle:
                  var rs = horseAnalyzer?.History?.RunningStyle ?? horse.RunningStyle;
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.RunningStyle == rs));
                  break;
                case QueryKey.CornerPlace1:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.FirstCornerOrder == horse.FirstCornerOrder));
                  break;
                case QueryKey.CornerPlace2:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.SecondCornerOrder == horse.SecondCornerOrder));
                  break;
                case QueryKey.CornerPlace3:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.ThirdCornerOrder == horse.ThirdCornerOrder));
                  break;
                case QueryKey.CornerPlace4:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.FourthCornerOrder == horse.FourthCornerOrder));
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
                  isRealtimeResult = true;
                  break;
                case QueryKey.Odds:
                  {
                    var (min, max) = AnalysisUtil.GetOddsRange(horse.Odds);
                    queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Odds >= min && rh.Odds < max));
                    isRealtimeResult = true;
                  }
                  break;
                case QueryKey.PlaceOddsMax:
                  {
                    var (min, max) = AnalysisUtil.GetOddsRange(horse.Odds);
                    queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.PlaceOddsMax >= min && rh.PlaceOddsMax < max));
                    isRealtimeResult = true;
                  }
                  break;
                case QueryKey.PlaceOddsMin:
                  {
                    var (min, max) = AnalysisUtil.GetOddsRange(horse.Odds);
                    queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.PlaceOddsMin >= min && rh.PlaceOddsMin < max));
                    isRealtimeResult = true;
                  }
                  break;
                case QueryKey.RaceCount:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.RaceCount == horse.RaceCount));
                  break;
                case QueryKey.RaceCountAfterLastRest:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.RaceCountAfterLastRest == horse.RaceCountAfterLastRest));
                  break;
                case QueryKey.RaceCountWithinRunning:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.RaceCountWithinRunning == horse.RaceCountWithinRunning));
                  break;
                case QueryKey.RaceCountWithinRunningCompletely:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.RaceCountWithinRunningCompletely == horse.RaceCountWithinRunningCompletely));
                  break;
                case QueryKey.PreviousRaceDays:
                  {
                    var days = horse.PreviousRaceDays / 14;
                    queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.PreviousRaceDays / 14 == days));
                  }
                  break;
                case QueryKey.Weight:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.Weight == horse.Weight));
                  if (horse.Weight == default)
                  {
                    isRealtimeResult = true;
                  }
                  break;
                case QueryKey.WeightDiff:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.WeightDiff == horse.WeightDiff));
                  if (horse.Weight == default)
                  {
                    isRealtimeResult = true;
                  }
                  break;
                case QueryKey.RiderWeight:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.RiderWeight == horse.RiderWeight));
                  break;
                case QueryKey.ResultTime:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.ResultTimeValue == horse.ResultTimeValue));
                  break;
                case QueryKey.ResultTimeDiff:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.TimeDifference == horse.TimeDifference));
                  break;
                case QueryKey.A3HTime:
                  queries.Add(new HorseLambdaScriptKeyQuery(rh => rh.AfterThirdHalongTimeValue == horse.AfterThirdHalongTimeValue));
                  break;
              }
            }
          }
        }
      }

      if (!queries.Any())
      {
        queries.Add(new SimpleScriptKeyQuery());
      }

      if (race != null && race.DataStatus >= RaceDataStatus.PreliminaryGradeFull)
      {
        isRealtimeResult = false;
      }

      return new ScriptKeysParseResult(queries, groupKey, limit, offset, diffQueries: diffQueries, diffQueriesBetweenCurrent: diffQueriesBetweenCurrent, isContainsFutureRaces: isContainsFutureRaces, isCurrentOnly: isCurrentRaceOnly, isRealtimeResult: isRealtimeResult, isExpandedResult: isExpandedResult)
      {
        MemoGroupInfo = memoGroupInfo,
        HasJrdbQuery = hasJrdbKeys,
        HasExtraQuery = hasExtraKeys,
      };
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

    private static (ScriptKeyQuery?, QueryKeyAttribute?) GetQuery(QueryType type, string scriptKey, string value, RaceData? race, RaceHorseData? horse)
    {
      var key = GetKeyInfo(scriptKey);
      if (key.Item1 == QueryKey.Unknown)
      {
        return default;
      }

      var isCompareCurrentRace = false;
      if (value.StartsWith(':'))
      {
        isCompareCurrentRace = true;
        value = value[1..];
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
            return (new ExpressionScriptKeyQuery(key.Item1, type == QueryType.Equals ? QueryType.Contains : QueryType.Excepts, values), key.Item2);
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
            return (new ExpressionScriptKeyQuery(key.Item1, type == QueryType.Equals ? QueryType.RangeOrEqual : QueryType.NotRangeOrEqual, min, max, race, horse, isCompareCurrentRace), key.Item2);
          }
        }
        else
        {
          if (int.TryParse(value, out var val))
          {
            return (new ExpressionScriptKeyQuery(key.Item1, type, val, race, horse, isCompareCurrentRace), key.Item2);
          }
        }
      }

      if (key.Item2 is StringQueryKeyAttribute)
      {
        return (new ExpressionScriptKeyQuery(key.Item1, type, value), key.Item2);
      }

      return default;
    }
  }

  public enum QueryKey
  {
    Unknown,
    [StringQueryKey("race")]
    RaceKey,
    [StringQueryKey("racename")]
    RaceName,
    [NumericQueryKey("nichiji")]
    Nichiji,
    [NumericQueryKey("racenumber")]
    RaceNumber,
    [NumericQueryKey("weather")]
    Weather,
    [NumericQueryKey("condition")]
    Condition,
    [NumericQueryKey("baneimoisture")]
    BaneiMoisture,
    [NumericQueryKey("course")]
    Course,
    [NumericQueryKey("ground")]
    Ground,
    [NumericQueryKey("distance")]
    Distance,
    [NumericQueryKey("direction")]
    Direction,
    [NumericQueryKey("tracktype")]
    TrackType,
    [NumericQueryKey("trackoption")]
    TrackOption,
    [NumericQueryKey("riderweightrule")]
    RiderWeightRule,
    [NumericQueryKey("arearule")]
    AreaRule,
    [NumericQueryKey("sexrule")]
    SexRule,
    [NumericQueryKey("crossrule")]
    CrossRule,
    [NumericQueryKey("day")]
    Day,
    [NumericQueryKey("month")]
    Month,
    [NumericQueryKey("year")]
    Year,
    [NumericQueryKey("date")]
    Date,
    [NumericQueryKey("hour")]
    Hour,
    [QueryKey("subject")]
    Subject,
    [NumericQueryKey("subjectage2")]
    SubjectAge2,
    [NumericQueryKey("subjectage3")]
    SubjectAge3,
    [NumericQueryKey("subjectage4")]
    SubjectAge4,
    [NumericQueryKey("subjectage5")]
    SubjectAge5,
    [NumericQueryKey("grade")]
    Grade,
    [NumericQueryKey("gradeid")]
    GradeId,
    [QueryKey("grades")]
    Grades,
    [NumericQueryKey("prize1")]
    PrizeMoney1,
    [NumericQueryKey("horsescount")]
    HorsesCount,
    [NumericQueryKey("goalhorsescount")]
    GoalHorsesCount,
    [NumericQueryKey("datastatus")]
    DataStatus,
    [NumericQueryKey("racebefore3h")]
    Before3HTime,
    [NumericQueryKey("raceafter3h")]
    After3HTime,

    [StringQueryKey("horse")]
    HorseKey,
    [StringQueryKey("horsename")]
    HorseName,
    [NumericQueryKey("age")]
    Age,
    [NumericQueryKey("sex")]
    Sex,
    [NumericQueryKey("horsetype")]
    HorseType,
    [NumericQueryKey("color")]
    Color,
    [NumericQueryKey("horsenumber")]
    HorseNumber,
    [NumericQueryKey("framenumber")]
    FrameNumber,
    [StringQueryKey("rider")]
    RiderCode,
    [StringQueryKey("ridername")]
    RiderName,
    [NumericQueryKey("place")]
    Place,
    [NumericQueryKey("goalplace")]
    GoalPlace,
    [NumericQueryKey("resultlength")]
    ResultLength,
    [NumericQueryKey("abnormal")]
    Abnormal,
    [NumericQueryKey("popular")]
    Popular,
    [NumericQueryKey("resulttime")]
    ResultTime,
    [NumericQueryKey("resulttimediff")]
    ResultTimeDiff,
    [NumericQueryKey("corner1")]
    CornerPlace1,
    [NumericQueryKey("corner2")]
    CornerPlace2,
    [NumericQueryKey("corner3")]
    CornerPlace3,
    [NumericQueryKey("corner4")]
    CornerPlace4,
    [NumericQueryKey("weight")]
    Weight,
    [NumericQueryKey("weightdiff")]
    WeightDiff,
    [NumericQueryKey("riderweight")]
    RiderWeight,
    [StringQueryKey("trainer")]
    TrainerCode,
    [StringQueryKey("trainername")]
    TrainerName,
    [StringQueryKey("owner")]
    OwnerCode,
    [StringQueryKey("ownername")]
    OwnerName,
    [NumericQueryKey("odds")]
    Odds,
    [NumericQueryKey("placeoddsmin")]
    PlaceOddsMin,
    [NumericQueryKey("placeoddsmax")]
    PlaceOddsMax,
    [NumericQueryKey("a3htime")]
    A3HTime,
    [NumericQueryKey("runningstyle")]
    RunningStyle,
    [NumericQueryKey("prevdays")]
    PreviousRaceDays,
    [NumericQueryKey("racecount")]
    RaceCount,
    [NumericQueryKey("racecount_run")]
    RaceCountWithinRunning,
    [NumericQueryKey("racecount_complete")]
    RaceCountWithinRunningCompletely,
    [NumericQueryKey("racecount_rest")]
    RaceCountAfterLastRest,
    [QueryKey("horsebelongs")]
    HorseBelongs,
    [QueryKey("riderbelongs")]
    RiderBelongs,
    [QueryKey("trainerbelongs")]
    TrainerBelongs,
    [NumericQueryKey("mark")]
    Mark,

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

    [NumericQueryKey("point")]
    Point,
    [NumericQueryKey("dropout")]
    Dropout,
    [NumericQueryKey("residue")]
    Residue,  // dropout の反対の働きをする
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
    JrdbRaceHorse,
    RaceHorseExtra,
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

  class StringQueryKeyAttribute : QueryKeyAttribute { public StringQueryKeyAttribute(string key) : base(key) { } public StringQueryKeyAttribute(string key, QueryTarget target) : base(key, target) { } }
}
