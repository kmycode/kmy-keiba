using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Memo
{
  internal static class MemoUtil
  {
    public static List<ExpansionMemoConfig> Configs { get; } = new();
    public static List<RaceMemoItem> MemoCaches { get; } = new();
    public static IList<PointLabelConfig> PointLabels => PointLabelModel.Default.Configs;
    private static bool _isInitialized;

    public static async Task InitializeAsync(MyContext db)
    {
      if (!_isInitialized)
      {
        var configs = await db.MemoConfigs!.ToListAsync();
        Configs.AddRange(configs);
        await PointLabelModel.InitializeAsync(db);
        _isInitialized = true;
      }
    }

    public static async Task<(IReadOnlyList<MemoData>, PointLabelData?)> GetRaceListMemosAsync(MyContext? db, IEnumerable<string> raceKeys)
    {
      var isDbNull = false;
      if (db == null)
      {
        isDbNull = true;
        db = new();
      }

      var configs = GetMemoConfigs(MemoTarget.Race, MemoTarget.Unknown, MemoTarget.Unknown)
        .Where(c => c.Style == MemoStyle.Point || c.Style == MemoStyle.MemoAndPoint)
        .Where(c => c.PointLabelId != default && PointLabels.Any(l => l.Data.Id == (uint)c.PointLabelId));
      if (configs.Any())
      {
        var config = configs.OrderBy(c => c.Order).First();
        var memos = await GetMemosAsync(db, config);
        var label = GetPointLabelConfig(config);
        return (memos, label);
      }

      /*
      var memoConfig = Configs!
        .Where(m => m.Target1 == MemoTarget.Race && m.Target2 == MemoTarget.Unknown && m.Target3 == MemoTarget.Unknown && m.Type == MemoType.Race && m.PointLabelId != default)
        .Where(m => m.Style == MemoStyle.Point || m.Style == MemoStyle.MemoAndPoint)
        .Join(db.PointLabels!, m => (uint)m.PointLabelId, p => p.Id, (m, p) => new { Memo = m, PointLabel = p, })
        .OrderBy(m => m.Memo.Order)
        .FirstOrDefault();
      var memos = Array.Empty<MemoData>();
      PointLabelData? pointLabel = null;
      if (memoConfig != null)
      {
        var number = memoConfig.Memo.MemoNumber;
        var memoData = await db.Memos!
          .Where(m => m.Target1 == MemoTarget.Race && m.Target2 == MemoTarget.Unknown && m.Target3 == MemoTarget.Unknown && m.Number == number)
          .Where(m => raceKeys.Contains(m.Key1))
          .ToArrayAsync();
        if (memoData.Any())
        {
          memos = memoData;
          pointLabel = memoConfig.PointLabel;
        }
      }
      */

      if (isDbNull)
      {
        db.Dispose();
      }

      return (Array.Empty<MemoData>(), null);
    }

    public static ExpansionMemoConfig? GetMemoConfig(MemoTarget target1, MemoTarget target2, MemoTarget target3, short number)
    {
      return Configs.FirstOrDefault(c => c.Target1 == target1 && c.Target2 == target2 && c.Target3 == target3 && c.MemoNumber == number);
    }

    public static IReadOnlyList<ExpansionMemoConfig> GetMemoConfigs(MemoTarget target1, MemoTarget target2, MemoTarget target3)
    {
      return Configs.Where(c => c.Target1 == target1 && c.Target2 == target2 && c.Target3 == target3).ToArray();
    }

    public static PointLabelData? GetPointLabelConfig(ExpansionMemoConfig config)
    {
      if (config.PointLabelId != default)
      {
        var label = PointLabels.FirstOrDefault(l => l.Data.Id == (uint)config.PointLabelId);
        if (label != null)
        {
          return label.Data;
        }
      }

      return null;
    }

    public static async Task<IReadOnlyList<MemoData>> GetMemosAsync(MyContext db, ExpansionMemoConfig config)
    {
      var memos = await db.Memos!.Where(m => m.Target1 == config.Target1 && m.Target2 == config.Target2 && m.Target3 == config.Target3 && m.Number == config.MemoNumber).ToArrayAsync();
      var caches = MemoCaches.Select(c => c.Data).Where(m => m.Target1 == config.Target1 && m.Target2 == config.Target2 && m.Target3 == config.Target3 && m.Number == config.MemoNumber);

      // キャッシュを優先して返す（インスタンスの使いまわし対策）
      return memos.GroupJoin(caches, m => m.Id, c => c.Id, (m, cs) => cs.Any() ? cs.First() : m).ToArray();
    }

    public static async Task<MemoData?> GetMemoAsync(MyContext db, RaceData race, ExpansionMemoConfig config, RaceHorseAnalyzer? horse, bool isCache = true)
    {
      return await GetMemoAsync(db, race, db.Memos!, config, horse, isCache);
    }

    public static async Task<MemoData?> GetMemoAsync(MyContext db, RaceData race, IQueryable<MemoData> query, ExpansionMemoConfig config, RaceHorseAnalyzer? horse, bool isCache = true)
    {
      var q = await GetMemoQueryAsync(db, race, MemoCaches.Select(c => c.Data), config, horse);
      var cache = q.FirstOrDefault();
      if (cache == null)
      {
        var qq = await GetMemoQueryAsync(db, race, query, config, horse);
        var data = await qq.FirstOrDefaultAsync();
        if (data != null && isCache)
        {
          MemoCaches.Add(new RaceMemoItem(data, config));
        }
        return data;
      }
      return cache;
    }

    public static async Task<IQueryable<MemoData>> GetMemoQueryAsync(MyContext db, RaceData race, IQueryable<MemoData> query, ExpansionMemoConfig config, RaceHorseAnalyzer? horse)
    {
      query = query.Where(m => m.Target1 == config.Target1 && m.Target2 == config.Target2 && m.Target3 == config.Target3 && m.Number == config.MemoNumber);
      query = query.Where(await GetTargetComparationAsync(db, race, 1, config.Target1, horse));
      if (config.Target2 != MemoTarget.Unknown)
        query = query.Where(await GetTargetComparationAsync(db, race, 2, config.Target2, horse));
      if (config.Target3 != MemoTarget.Unknown)
        query = query.Where(await GetTargetComparationAsync(db, race, 3, config.Target3, horse));
      return query;
    }

    public static async Task<IEnumerable<MemoData>> GetMemoQueryAsync(MyContext db, RaceData race, IEnumerable<MemoData> query, ExpansionMemoConfig config, RaceHorseAnalyzer? horse)
    {
      query = query.Where(m => m.Target1 == config.Target1 && m.Target2 == config.Target2 && m.Target3 == config.Target3 && m.Number == config.MemoNumber);
      query = query.Where((await GetTargetComparationAsync(db, race, 1, config.Target1, horse)).Compile());
      if (config.Target2 != MemoTarget.Unknown)
        query = query.Where((await GetTargetComparationAsync(db, race, 2, config.Target2, horse)).Compile());
      if (config.Target3 != MemoTarget.Unknown)
        query = query.Where((await GetTargetComparationAsync(db, race, 3, config.Target3, horse)).Compile());
      return query;
    }

    public static async Task<IEnumerable<RaceMemoItem>> GetMemoQueryAsync(MyContext db, RaceData race, IEnumerable<RaceMemoItem> query, ExpansionMemoConfig config, RaceHorseAnalyzer? horse)
    {
      var hits = await GetMemoQueryAsync(db, race, query.Select(i => i.Data), config, horse);
      return query.Join(hits.ToArray(), i => i.Data, m => (object)m, (i, m) => i);
    }

    public static async Task<Expression<Func<MemoData, bool>>> GetTargetComparationAsync(MyContext db, RaceData race, int target, MemoTarget targetType, RaceHorseAnalyzer? horse)
    {
      var memo = Expression.Parameter(typeof(MemoData), "x");
      var key = Expression.Property(memo, "Key" + target);

      if (targetType == MemoTarget.Race)
      {
        return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(race.Key)), memo);
      }
      else if (targetType == MemoTarget.Course)
      {
        key = Expression.Property(memo, nameof(MemoData.CourseKey));
        return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(race.Course)), memo);
      }
      else if (targetType == MemoTarget.Day)
      {
        return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(race.Key[..8])), memo);
      }
      else if (targetType == MemoTarget.Unknown)
      {
        return Expression.Lambda<Func<MemoData, bool>>(Expression.Constant(true), memo);
      }
      else if (horse != null)
      {
        if (targetType == MemoTarget.Horse)
        {
          return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(horse.Data.Key)), memo);
        }
        else if (targetType == MemoTarget.Rider)
        {
          return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(horse.Data.RiderCode)), memo);
        }
        else if (targetType == MemoTarget.Trainer)
        {
          return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(horse.Data.TrainerCode)), memo);
        }
        else if (targetType == MemoTarget.Owner)
        {
          return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(horse.Data.OwnerCode)), memo);
        }
        else if (targetType == MemoTarget.Father)
        {
          var father = await HorseBloodUtil.GetBloodCodeAsync(db, horse.Data.Key, BloodType.Father);
          if (!string.IsNullOrEmpty(father))
          {
            return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(father)), memo);
          }
        }
        else if (targetType == MemoTarget.Mother)
        {
          var father = await HorseBloodUtil.GetBloodCodeAsync(db, horse.Data.Key, BloodType.Mother);
          if (!string.IsNullOrEmpty(father))
          {
            return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(father)), memo);
          }
        }
        else if (targetType == MemoTarget.MotherFather)
        {
          var father = await HorseBloodUtil.GetBloodCodeAsync(db, horse.Data.Key, BloodType.MotherFather);
          if (!string.IsNullOrEmpty(father))
          {
            return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(father)), memo);
          }
        }
      }

      return Expression.Lambda<Func<MemoData, bool>>(Expression.Constant(false), memo);
    }

    public static string GetMemoTarget(MemoTarget target)
    {
      return target switch
      {
        MemoTarget.Race => "race",
        MemoTarget.Course => "course",
        MemoTarget.Distance => "distance",
        MemoTarget.Day => "day",
        MemoTarget.Direction => "direction",
        MemoTarget.Grades => "grades",
        MemoTarget.Horse => "horse",
        MemoTarget.Rider => "rider",
        MemoTarget.Trainer => "trainer",
        MemoTarget.Owner => "owner",
        MemoTarget.Father => "f",
        MemoTarget.MotherFather => "mf",
        (MemoTarget)(short)-1 => "point",
        (MemoTarget)(short)-2 => "number",
        _ => string.Empty,
      };
    }

    public static MemoTarget GetMemoTarget(string key)
    {
      var pointTarget = (MemoTarget)(short)-1;
      var numberTarget = (MemoTarget)(short)-2;

      return key switch
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
        "m" => MemoTarget.Mother,
        "mf" => MemoTarget.MotherFather,
        "point" => pointTarget,
        "" => pointTarget,
        "number" => numberTarget,
        _ => MemoTarget.Unknown,
      };
    }
  }
}
