using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.ExNumber
{
  internal static class ExternalNumberUtil
  {
    public static List<ExternalNumberConfig> Configs { get; } = new();

    private static bool _isInitialized;
    private static readonly Dictionary<(uint, string, short), ExternalNumberData?> _cache = new();

    public static async Task InitializeAsync(MyContext db)
    {
      if (!_isInitialized)
      {
        var configs = await db.ExternalNumberConfigs!.ToArrayAsync();
        foreach (var config in configs)
        {
          Configs.Add(config);
        }

        ExternalNumberConfigModel.Default.Initialize();

        _isInitialized = true;
      }
    }

    public static async Task<ExternalNumberData?> GetValueAsync(MyContext db, ExternalNumberConfig config, string raceKey, short horseNumber)
    {
      if (_cache.TryGetValue((config.Id, raceKey, horseNumber), out var cache))
      {
        return cache;
      }

      var number = await db.ExternalNumbers!.Where(n => n.ConfigId == config.Id && n.RaceKey == raceKey && n.HorseNumber == horseNumber).FirstOrDefaultAsync();
      _cache[(config.Id, raceKey, horseNumber)] = number;
      return number;
    }

    public static async Task<IReadOnlyList<ExternalNumberData>> GetValuesAsync(MyContext db, string raceKey)
    {
      return await db.ExternalNumbers!.Where(n => n.RaceKey == raceKey).ToArrayAsync();
    }

    public static ExternalNumberConfig? GetConfig(uint id)
    {
      return Configs.FirstOrDefault(c => c.Id == id);
    }

    private static string TryReplace(this string str, string a, Func<string> b)
    {
      if (str.Contains(a))
      {
        return str.Replace(a, b());
      }
      return str;
    }

    public static async Task SaveRangeAsync(MyContext db, ExternalNumberConfig config, DateTime start, DateTime end, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      progress ??= new ReactiveProperty<int>();
      progressMax ??= new ReactiveProperty<int>();

      var races = await db.Races!.Where(r => r.StartTime >= start && r.StartTime <= end).ToArrayAsync();
      progressMax.Value = races.Length;
      progress.Value = 0;

      var count = 0;
      var validCount = 0;
      foreach (var race in races)
      {
        var list = ReadRaceHorseValues(db, config, race);
        if (list.Any())
        {
          var olds = (IEnumerable<ExternalNumberData>)await db.ExternalNumbers!.Where(n => n.RaceKey == race.Key).ToArrayAsync();
          var compare = list.GroupJoin(olds, n => n.HorseNumber, o => o.HorseNumber, (n, os) => new { Old = os.FirstOrDefault(), New = n, });

          var targetItems = compare
            .Where(d => d.Old == null || (d.Old.Value != d.New.Value || d.Old.Order != d.New.Order));
          db.ExternalNumbers!.RemoveRange(targetItems.Where(i => i.Old != null).Select(i => i.Old!));
          await db.ExternalNumbers!.AddRangeAsync(targetItems.Select(i => i.New));

          validCount++;
        }

        count++;
        if (validCount >= 1000)
        {
          await db.SaveChangesAsync();
          await db.CommitAsync();
          validCount = 0;
        }
        if (count >= 1000)
        {
          progress.Value += count;
          count = 0;
        }
      }

      await db.SaveChangesAsync();
      await db.CommitAsync();
    }

    private static IReadOnlyList<ExternalNumberData> ReadRaceHorseValues(MyContext db, ExternalNumberConfig config, RaceData race)
    {
      var fileName = GetFileName(config.FileNamePattern, race);
      if (!File.Exists(fileName))
      {
        return Array.Empty<ExternalNumberData>();
      }

      var lines = File.ReadAllLines(fileName);
      if (!lines.Any())
      {
        return Array.Empty<ExternalNumberData>();
      }

      var raceIdFormat = config.RaceIdFormat;
      if (raceIdFormat == ExternalNumberRaceIdFormat.Auto && (config.FileFormat == ExternalNumberFileFormat.RaceCsv || config.FileFormat == ExternalNumberFileFormat.RaceHorseCsv))
      {
        var raceKeyExample = lines[0].Split(',')[0];
        var len = raceKeyExample.Length;
        raceIdFormat = (len == 16 || len == 18) ? ExternalNumberRaceIdFormat.CurrentRule :
          (len == 8 || len == 10) ? ExternalNumberRaceIdFormat.OldRule :
          (len == 12 || len == 14) ? ExternalNumberRaceIdFormat.ThirdRule : ExternalNumberRaceIdFormat.Auto;
      }

      if (raceIdFormat == ExternalNumberRaceIdFormat.Auto)
      {
        return Array.Empty<ExternalNumberData>();
      }

      var raceId = race.Key;
      if (raceIdFormat == ExternalNumberRaceIdFormat.OldRule)
      {
        raceId = ((short)race.Course).ToString("00") + race.StartTime.ToString("yy") +
          race.Kaiji.ToString("X") + race.Nichiji.ToString("X") + race.CourseRaceNumber.ToString("00");
      }
      else if (raceIdFormat == ExternalNumberRaceIdFormat.ThirdRule)
      {
        raceId = race.StartTime.ToString("yyyyMMdd") + ((short)race.Course).ToString("00") +
          race.CourseRaceNumber.ToString("00");
      }

      int ValueToShort(string v)
      {
        var mul = 100;
        if (v.Contains('.'))
        {
          var d = v.Length - v.IndexOf('.') - 1;
          mul = d == 1 ? 10 : d == 2 ? 1 : 100;
        }
        int.TryParse(v.Replace(".", string.Empty), out var num);
        return num * mul;
      }

      ExternalNumberDotFormat GetDotFormat(string v)
      {
        if (v.Contains('.'))
        {
          var d = v.Length - v.IndexOf('.') - 1;
          return d == 1 ? ExternalNumberDotFormat.Real1 : d == 2 ? ExternalNumberDotFormat.Real2 : ExternalNumberDotFormat.Auto;
        }
        return ExternalNumberDotFormat.IntegerWithSign;
      }

      // ファイルの中身をデータに変換
      var items = new List<ExternalNumberData>();
      foreach (var line in lines.Where(l => l.StartsWith(raceId)))
      {
        int[] values;
        string key;
        string sampleValue;
        if (config.FileFormat == ExternalNumberFileFormat.RaceHorseCsv || config.FileFormat == ExternalNumberFileFormat.RaceCsv)
        {
          key = line.Split(',')[0];
          values = line.Split(',')
            .Skip(1)
            .Select(v => ValueToShort(v))
            .ToArray();
          sampleValue = line.Split(',').Skip(1).FirstOrDefault() ?? string.Empty;
        }
        else
        {
          var vals = new List<int>();
          var idLen = config.FileFormat == ExternalNumberFileFormat.RaceFixedLength ? raceId.Length : raceId.Length + 2;
          key = line[0..idLen];
          sampleValue = line.Substring(idLen, 6);

          if (config.ValuesFormat == ExternalNumberValuesFormat.NumberOnly)
          {
            for (var i = 0; i < line.Length / 6; i++)
            {
              vals.Add(ValueToShort(line.Substring(idLen + i * 6, 6)));
            }
          }
          else
          {
            for (var i = 0; i < line.Length / 8; i++)
            {
              vals.Add(ValueToShort(line.Substring(idLen + i * 8, 6)));
              vals.Add(ValueToShort(line.Substring(idLen + i * 8 + 6, 2)));
            }
          }
          values = vals.ToArray();
        }

        // 小数点の位置を保存
        var dotFormat = GetDotFormat(sampleValue);
        if (config.DotFormat != dotFormat)
        {
          try
          {
            db.ExternalNumberConfigs!.Attach(config);
            config.DotFormat = dotFormat;
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
            db.SaveChanges();
#pragma warning restore CS0618 // 型またはメンバーが旧型式です
          }
          catch (Exception ex)
          {
            // TODO
          }
        }

        // リストに登録
        if (config.FileFormat == ExternalNumberFileFormat.RaceHorseFixedLength || config.FileFormat == ExternalNumberFileFormat.RaceHorseCsv)
        {
          // keyに馬番が入ってる
          var raceKey = key.Substring(0, key.Length - 2);
          var horseNum = key.Substring(key.Length - 2, 2);
          short.TryParse(horseNum, out var horseNumber);

          if (config.ValuesFormat == ExternalNumberValuesFormat.NumberOnly)
          {
            items.Add(new ExternalNumberData
            {
              ConfigId = config.Id,
              HorseNumber = horseNumber,
              RaceKey = race.Key,
              Value = values.ElementAtOrDefault(0),
            });
          }
          else
          {
            items.Add(new ExternalNumberData
            {
              ConfigId = config.Id,
              HorseNumber = horseNumber,
              RaceKey = race.Key,
              Value = values.ElementAtOrDefault(0),
              Order = (short)values.ElementAtOrDefault(1),
            });
          }
        }
        else
        {
          // keyはレースのみ
          var raceKey = key;

          if (config.ValuesFormat == ExternalNumberValuesFormat.NumberOnly)
          {
            for (var i = 0; i < values.Length; i++)
            {
              items.Add(new ExternalNumberData
              {
                ConfigId = config.Id,
                HorseNumber = (short)(i + 1),
                RaceKey = race.Key,
                Value = values[i],
              });
            }
          }
          else
          {
            for (var i = 0; i < values.Length / 2; i++)
            {
              items.Add(new ExternalNumberData
              {
                ConfigId = config.Id,
                HorseNumber = (short)(i + 1),
                RaceKey = race.Key,
                Value = values[i * 2],
                Order = (short)values[i * 2 + 1],
              });
            }
          }
        }

        // 順番を登録
        if (config.ValuesFormat == ExternalNumberValuesFormat.NumberOnly)
        {
          var sorted = config.SortRule switch
          {
            ExternalNumberSortRule.Smaller => items.OrderBy(i => i.Value),
            ExternalNumberSortRule.Larger => items.OrderByDescending(i => i.Value),
            ExternalNumberSortRule.SmallerWithoutZero => items.Where(i => i.Value != default).OrderBy(i => i.Value),
            _ => Enumerable.Empty<ExternalNumberData>(),
          };
          var order = 1;
          foreach (var item in sorted)
          {
            item.Order = (short)order++;
          }
          if (config.SortRule == ExternalNumberSortRule.SmallerWithoutZero)
          {
            foreach (var item in items.Where(i => i.Value == default))
            {
              item.Order = default;
            }
          }
        }
      }

      // キャッシュをクリア
      var oldCaches = new List<(uint, string, short)>();
      foreach (var cache in _cache)
      {
        if (cache.Key.Item1 == config.Id && cache.Key.Item2 == race.Key)
        {
          oldCaches.Add(cache.Key);
        }
      }
      foreach (var key in oldCaches)
      {
        _cache.Remove(key);
      }

      return items;
    }

    public static string GetFileName(string pattern, RaceData race)
    {
      string ToZenkaku(string number)
      {
        return number
          .Replace('0', '０')
          .Replace('1', '１')
          .Replace('2', '２')
          .Replace('3', '３')
          .Replace('4', '４')
          .Replace('5', '５')
          .Replace('6', '６')
          .Replace('7', '７')
          .Replace('8', '８')
          .Replace('9', '９')
          .Replace('A', 'Ａ')
          .Replace('B', 'Ｂ')
          .Replace('C', 'Ｃ')
          .Replace('D', 'Ｄ')
          .Replace('E', 'Ｅ')
          .Replace('F', 'Ｆ')
          .Replace('A', 'ａ')
          .Replace('B', 'ｂ')
          .Replace('C', 'ｃ')
          .Replace('D', 'ｄ')
          .Replace('E', 'ｅ')
          .Replace('F', 'ｆ');
      }

      int GetKolCode(RaceCourse course)
      {
        return course switch
        {
          RaceCourse.Kyoto => 0,
          RaceCourse.Hanshin => 1,
          RaceCourse.Chukyo => 2,
          RaceCourse.Kokura => 3,
          RaceCourse.Tokyo => 4,
          RaceCourse.Nakayama => 5,
          RaceCourse.Fukushima => 6,
          RaceCourse.Niigata => 7,
          RaceCourse.Sapporo => 8,
          RaceCourse.Hakodate => 9,
          RaceCourse.Oi => 10,
          RaceCourse.Kawazaki => 11,
          RaceCourse.Funabashi => 12,
          RaceCourse.Urawa => 13,
          RaceCourse.Iwamizawa => 14,
          RaceCourse.Asahikawa => 15,
          RaceCourse.ObihiroBannei => 16,
          // 野田：非対応
          RaceCourse.Kitami => 18,
          RaceCourse.Kasamatsu => 19,
          RaceCourse.Kanazawa => 20,
          RaceCourse.Arao => 21,
          RaceCourse.Utsunomiya => 22,
          RaceCourse.Saga => 23,
          RaceCourse.Nakatsu => 24,
          RaceCourse.Takasaki => 25,
          RaceCourse.Kochi => 26,
          RaceCourse.Ashikaka => 27,
          RaceCourse.Kaminoyama => 28,
          RaceCourse.Mizusawa => 29,
          RaceCourse.Sanjo => 30,
          RaceCourse.Kimidera => 31,     // KOL仕様書では「紀井」と書かれている
          RaceCourse.Masuda => 32,
          RaceCourse.Morioka => 33,
          // 旧名古屋競馬場：非対応
          RaceCourse.SapporoLocal => 35,
          RaceCourse.HakodateLocal => 36,
          RaceCourse.Sonoda => 37,
          RaceCourse.Fukuyama => 38,
          RaceCourse.Himeji => 39,
          RaceCourse.ChukyoLocal => 40,
          RaceCourse.NiigataLocal => 41,
          RaceCourse.Mombetsu => 42,
          RaceCourse.Nagoya => 43,
          // 小林：非対応
          // 西脇：非対応
          // 境町：非対応
          // 小向：非対応
          // 48、49　欠番
          // 栗東：非対応
          // 美浦南：非対応
          // 美浦北：非対応
          // 白井：非対応
          // 道営：非対応
          // 岩手：非対応
          // 岩見（トレセン？）：非対応
          // 旭川（トレセン？）：非対応
          // 帯広（トレセン？）：非対応
          // 北見（トレセン？）：非対応
          RaceCourse.HongKong => 60,
          RaceCourse.Usa => 61,
          RaceCourse.Uk => 62,
          RaceCourse.France => 63,
          RaceCourse.Ireland => 64,
          RaceCourse.Arab => 65,
          RaceCourse.Canada => 66,
          RaceCourse.Italy => 67,
          RaceCourse.Germany => 68,
          RaceCourse.Australia => 69,
          RaceCourse.WestGermany => 70,
          RaceCourse.NewZealand => 71,
          RaceCourse.Chile => 72,
          RaceCourse.Argentina => 73,
          RaceCourse.Brazil => 74,
          RaceCourse.Singapore => 75,
          RaceCourse.Sweden => 76,
          RaceCourse.Spain => 77,
          RaceCourse.Swizerland => 78,
          RaceCourse.Belgium => 79,
          RaceCourse.Macau => 80,
          RaceCourse.Austria => 81,
          RaceCourse.Turkey => 82,
          RaceCourse.Qatar => 83,
          // 84: 欠番
          RaceCourse.Korea => 85,
          // ソウル：非対応
          // プサン：非対応
          RaceCourse.Peru => 88,
          RaceCourse.SaudiArabia => 89,
          _ => default,
        };
      }

      var kaiji = race.Kaiji;
      var nichiji = race.Nichiji;
      var kaisai = race.Course != RaceCourse.Unknown ? (race.Course <= RaceCourse.Chukyo ? 1 : race.Course <= RaceCourse.CentralMaxValue ? 2 : 3) : 0;

      return pattern
        .Replace("%FP", @"c:\TFJV")
        .Replace("%FD", @"c:\TFJV")
        .Replace("%Y1", race.StartTime.ToString("yy"))
        .TryReplace("%Y2", () => ToZenkaku(race.StartTime.ToString("yy")))
        .Replace("%Y3", race.StartTime.ToString("yyyy"))
        .TryReplace("%Y4", () => ToZenkaku(race.StartTime.ToString("yyyy")))
        .Replace("%M1", race.StartTime.ToString("MM"))
        .TryReplace("%M2", () => ToZenkaku(race.StartTime.ToString("MM")))
        .Replace("%M3", race.StartTime.ToString("M"))
        .Replace("%D1", race.StartTime.ToString("dd"))
        .TryReplace("%D2", () => ToZenkaku(race.StartTime.ToString("dd")))
        .Replace("%D3", race.StartTime.ToString("d"))
        .TryReplace("%P1", () =>
        {
          var name = race.Course.GetName();
          if (name == "中京")
          {
            return "名";
          }
          if (name.Length >= 1)
          {
            return name[..1];
          }
          return name;
        })
        .Replace("%P2", race.Course.GetName())
        .Replace("%P3", ((short)race.Course).ToString())
        .Replace("%P4", ((short)race.Course).ToString("00"))
        .Replace("%P5", ((short)race.Course).ToString("x"))
        .Replace("%P4", ((short)race.Course).ToString("00"))
        .Replace("%P5", ((short)race.Course).ToString("x"))
        .TryReplace("%P8", () => (race.Course switch
        {
          RaceCourse.Mombetsu => 36,
          RaceCourse.Asahikawa => 7,
          RaceCourse.Morioka => 10,
          RaceCourse.Mizusawa => 11,
          RaceCourse.Urawa => 18,
          RaceCourse.Funabashi => 19,
          RaceCourse.Oi => 20,
          RaceCourse.Kawazaki => 21,
          RaceCourse.Kanazawa => 22,
          RaceCourse.Kasamatsu => 23,
          RaceCourse.Nagoya => 24,
          RaceCourse.Sonoda => 27,
          RaceCourse.Himeji => 28,
          RaceCourse.Fukuyama => 30,
          RaceCourse.Kochi => 31,
          RaceCourse.Saga => 32,
          RaceCourse.Arao => 33,
          RaceCourse.SapporoLocal => 8,
          _ => 0,
        }).ToString())
        .TryReplace("%PA", () => race.Course switch
        {
          RaceCourse.Sapporo => "sap",
          RaceCourse.Hakodate => "hak",
          RaceCourse.Fukushima => "fuk",
          RaceCourse.Niigata => "nii",
          RaceCourse.Tokyo => "tok",
          RaceCourse.Nakayama => "nak",
          RaceCourse.Chukyo => "chu",
          RaceCourse.Kyoto => "kyo",
          RaceCourse.Hanshin => "han",
          RaceCourse.Kokura => "kok",
          _ => string.Empty,
        })
        .TryReplace("%PN", () => race.Course switch
        {
          RaceCourse.Sapporo => "sapporo",
          RaceCourse.Hakodate => "hakodate",
          RaceCourse.Fukushima => "fukushima",
          RaceCourse.Niigata => "niigata",
          RaceCourse.Tokyo => "tokyo",
          RaceCourse.Nakayama => "nakayama",
          RaceCourse.Chukyo => "chukyo",
          RaceCourse.Kyoto => "kyoto",
          RaceCourse.Hanshin => "hanshin",
          RaceCourse.Kokura => "kokura",
          _ => race.Course.ToString().ToLower(),
        })
        .Replace("%P9", kaisai.ToString())
        .Replace("%PB", kaisai == 1 ? "e" : kaisai == 2 ? "w" : kaisai == 3 ? "1" : string.Empty)
        .Replace("%PC", kaisai == 1 ? "E" : kaisai == 2 ? "W" : kaisai == 3 ? "L" : string.Empty)
        .Replace("%K1", kaiji.ToString("x"))
        .TryReplace("%K2", () => ToZenkaku(kaiji.ToString("x")))
        .Replace("%K3", kaiji.ToString("00"))
        .Replace("%K4", kaiji.ToString())
        .Replace("%N1", nichiji.ToString("x"))
        .TryReplace("%N2", () => ToZenkaku(nichiji.ToString("x")))
        .Replace("%N3", nichiji.ToString("00"))
        .Replace("%N4", nichiji.ToString())
        .Replace("%N5", (nichiji % 2 == 1) ? "sat" : "sun")
        .Replace("%R1", race.CourseRaceNumber.ToString("00"))
        .Replace("%R2", ToZenkaku(race.CourseRaceNumber.ToString("00")))
        .Replace("%R3", race.CourseRaceNumber.ToString("x"))
        .Replace("%KY", race.Distance.ToString("0000"))
        .Replace("%H1", race.StartTime.Month <= 6 ? "1" : "2")
        .Replace("%W2", "%W1")
        .Replace("%W1", new CultureInfo("ja-JP").Calendar.GetWeekOfYear(race.StartTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday).ToString("00"))
        .Replace("%W5", race.StartTime.DayOfWeek.ToString().ToLower().Substring(0, 3))
        .TryReplace("%P6", () => GetKolCode(race.Course).ToString("00"))
        .TryReplace("%P7", () => GetKolCode(race.Course).ToString())
        .Replace("%TC", race.TrackCode.ToString("00"));
    }
  }
}
