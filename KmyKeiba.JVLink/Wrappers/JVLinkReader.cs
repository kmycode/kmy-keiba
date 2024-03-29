﻿using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Wrappers
{
  public interface IJVLinkReader : IDisposable
  {
    int DownloadedCount => 0;

    int DownloadCount => 0;

    int ReadCount => 0;

    int ReadedCount => 0;

    int ReadedEntityCount => 0;

    JVLinkObjectType Type { get; }

    JVLinkReaderData Load(IEnumerable<string>? targetSpecs = null);

    void StopLoading();
  }

  class EmptyJVLinkReader : IJVLinkReader
  {
    private readonly IJVLinkObject link;

    public JVLinkObjectType Type => JVLinkObjectType.Unknown;

    public EmptyJVLinkReader(IJVLinkObject link)
    {
      this.link = link;
    }

    public void Dispose()
    {
      this.link.Close();
      this.link.IsOpen = false;
    }

    public JVLinkReaderData Load(IEnumerable<string>? targetSpecs = null)
    {
      return new JVLinkReaderData();
    }

    public void StopLoading()
    {
    }
  }

  class JVLinkReader : IJVLinkReader
  {
    private readonly IJVLinkObject link;
    private readonly bool isRealTime;
    private bool isCanceled;

    public JVLinkObjectType Type => this.link.Type;

    public int DownloadedCount => this.link.Status();

    public int DownloadCount { get; set; }

    public int ReadCount { get; set; }

    public int ReadedCount { get; set; }

    public int ReadedEntityCount { get; set; }

    public DateTime StartDate { get; }

    public DateTime EndDate { get; }

    static JVLinkReader()
    {
      // SJISを扱う
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public JVLinkReader(IJVLinkObject link, int readCount, int downloadCount, DateTime? start, DateTime? end, bool isRealtime)
    {
      this.ReadCount = readCount;
      this.DownloadCount = downloadCount;

      this.link = link;
      link.IsOpen = true;

      this.isRealTime = isRealtime;

      this.StartDate = start ?? default;
      this.EndDate = end ?? default;
    }

    public JVLinkReaderData Load(IEnumerable<string>? targetSpecs = null)
    {
      var data = new JVLinkReaderData();
      var buffSize = 110000;
      var managedBuff = new byte[buffSize];

      var lastFileName = string.Empty;
      this.ReadedEntityCount = 0;

      while (true)
      {
        if (this.isCanceled)
        {
          return data;
        }

        var buff = Array.Empty<byte>();
        var result = this.link.Gets(ref buff, buffSize, out string fileName);
        if (result < -1)
        {
          switch (result)
          {
            case -402:
            case -403:
            case -502:
            case -503:
            case -203:
              {
                this.link.FileDelete(fileName);
                // this.link.Close();
                continue;
              }
            default:
              {
                throw JVLinkException.GetError((JVLinkReadResult)result);
              }
          }
        }
        else if (result == 0)
        {
          break;
        }

        // 地方競馬の場合、取得するファイルをファイル名で判別する（終端時刻が指定できないため）
        var isSkip = false;
        var fileDateStr = fileName.Substring(4, 6);
        if (this.link.Type == JVLinkObjectType.Local && this.EndDate != DateTime.MinValue && int.TryParse(fileDateStr, out var fileDate))
        {
          var downloadEndDate = this.EndDate.Year * 100 + this.EndDate.Month;
          if (fileDate > downloadEndDate)
          {
            isSkip = true;
          }
        }

        // var managedBuff = new byte[buff.Length];
        Array.Copy(buff, managedBuff, buff.Length);

        // JV、NV-Linkの仕様でメモリ解放が必要（Array.Resizeで解放になるらしい）
        Array.Resize(ref buff, 0);
        /*
        unsafe
        {
          fixed (byte* p = buff)
          {
            Marshal.FreeCoTaskMem((IntPtr)p);
            // CoTaskMemFree((IntPtr)p);
          }
        }
        */

        var d = Encoding.GetEncoding(932).GetString(managedBuff);
        var spec = d[..2];

        void Read<T>(T item, List<T> list, Func<T, T, bool> isEquals, IComparer<T> comparer)
          where T : EntityBase
        {
          if (this.link.Type == JVLinkObjectType.Local && this.EndDate != default)
          {
            // 地方競馬UmaConnでは終端時刻を指定しても最新まですべて読み込んでしまう
            if (item.LastModified != default && (item.LastModified < this.StartDate || item.LastModified > this.EndDate))
            {
              this.ReadedEntityCount--;
              return;
            }
          }

          var oldItem = list.FirstOrDefault((r) => isEquals(r, item));
          if (oldItem != null)
          {
            if (oldItem.LastModified < item.LastModified)
            {
              list.Remove(oldItem);
            }
            if (oldItem.LastModified >= item.LastModified)
            {
              return;
            }
          }

          list.Add(item);
        }

        void ReadDic<KEY, T>(T item, Dictionary<KEY, T> list, KEY key)
          where T : EntityBase where KEY : IComparable
        {
          if (this.link.Type == JVLinkObjectType.Local && this.EndDate != default)
          {
            // 地方競馬UmaConnでは終端時刻を指定しても最新まですべて読み込んでしまう
            // ファイル名ではじくようにしたのでこの処理は不要？
            /*
            if (item.LastModified != default && (item.LastModified < this.StartDate || item.LastModified > this.EndDate))
            {
              this.ReadedEntityCount--;
              return;
            }
            */
          }

          list.TryGetValue(key, out var oldItem);
          if (oldItem != null)
          {
            if (oldItem.LastModified >= item.LastModified)
            {
              return;
            }
          }

          list[key] = item;
        }

        if ((targetSpecs != null && targetSpecs.Any() && !targetSpecs.Contains(spec)) || isSkip)
        {
          if (!this.isRealTime)
          {
            this.link.Skip();
          }
        }
        else
        {
          this.ReadedEntityCount++;
          switch (spec)
          {
            case "RA":
              {
                var a = new JVData_Struct.JV_RA_RACE();
                a.SetDataB(ref d);
                var item = Race.FromJV(a);

                // Read(item, data.Races, (a, b) => a.Key == b.Key, new ComparableComparer<Race>(r => r.Key));
                ReadDic(item, data.Races, item.Key);
                break;
              }
            case "SE":
              {
                var a = new JVData_Struct.JV_SE_RACE_UMA();
                a.SetDataB(ref d);
                var item = RaceHorse.FromJV(a);

                // Read(item, data.RaceHorses, (a, b) => a.RaceKey == b.RaceKey && a.Name == b.Name, new ComparableComparer<RaceHorse>(x => x?.RaceKey + x?.Name));
                ReadDic(item, data.RaceHorses, item.RaceKey + item.Name);
                break;
              }
            case "WH":
              {
                var a = new JVData_Struct.JV_WH_BATAIJYU();
                a.SetDataB(ref d);
                var item = HorseWeight.FromJV(a);

                Read(item, data.HorseWeights, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<HorseWeight>(x => x?.RaceKey));
                break;
              }
            case "WE":
              {
                var a = new JVData_Struct.JV_WE_WEATHER();
                a.SetDataB(ref d);
                var item = CourseWeatherCondition.FromJV(a);

                Read(item, data.CourseWeatherConditions, (a, b) => a.RaceKeyWithoutRaceNum == b.RaceKeyWithoutRaceNum, new ComparableComparer<CourseWeatherCondition>(x => x?.RaceKeyWithoutRaceNum));
                break;
              }
            case "AV":
              {
                var a = new JVData_Struct.JV_AV_INFO();
                a.SetDataB(ref d);
                var item = HorseAbnormality.FromJV(a);

                Read(item, data.HorseAbnormalities, (a, b) => a.RaceKey + a.HorseNumber == b.RaceKey + b.HorseNumber, new ComparableComparer<HorseAbnormality>(x => x?.RaceKey + x?.HorseNumber));
                break;
              }
            case "UM":
              {
                var a = new JVData_Struct.JV_UM_UMA();
                a.SetDataB(ref d);
                var item = Horse.FromJV(a);

                // Read(item, data.Horses, (a, b) => a.Code == b.Code, new ComparableComparer<Horse>(x => x?.Code));
                ReadDic(item, data.Horses, item.Code + item.CentralFlag);
                break;
              }
            case "HN":
              {
                var a = new JVData_Struct.JV_HN_HANSYOKU();
                a.SetDataB(ref d);
                var item = HorseBlood.FromJV(a);

                // Read(item, data.HorseBloods, (a, b) => a.Key == b.Key, new ComparableComparer<HorseBlood>(x => x?.Key));
                ReadDic(item, data.HorseBloods, item.Key);
                break;
              }
            case "SK":
              {
                var a = new JVData_Struct.JV_SK_SANKU();
                a.SetDataB(ref d);
                var item = BornHorse.FromJV(a);

                // Read(item, data.HorseBloods, (a, b) => a.Key == b.Key, new ComparableComparer<HorseBlood>(x => x?.Key));
                ReadDic(item, data.BornHorses, item.Code);
                break;
              }
            case "BT":
              {
                var a = new JVData_Struct.JV_BT_KEITO();
                a.SetDataB(ref d);
                var item = HorseBloodInfo.FromJV(a);

                // Read(item, data.HorseBloods, (a, b) => a.Key == b.Key, new ComparableComparer<HorseBlood>(x => x?.Key));
                ReadDic(item, data.HorseBloodInfos, item.Key);
                break;
              }
            case "JC":
              {
                var a = new JVData_Struct.JV_JC_INFO();
                a.SetDataB(ref d);
                var item = HorseRiderChange.FromJV(a);

                Read(item, data.HorseRiderChanges, (a, b) => a.RaceKey + a.HorseNumber == b.RaceKey + b.HorseNumber, new ComparableComparer<HorseRiderChange>(x => x?.RaceKey + x?.HorseNumber));
                break;
              }
            case "HC":
              {
                var a = new JVData_Struct.JV_HC_HANRO();
                a.SetDataB(ref d);
                var item = Training.FromJV(a);

                // Read(item, data.Trainings, (a, b) => a.HorseKey == b.HorseKey && a.StartTime == b.StartTime, new ComparableComparer<Training>(x => x?.HorseKey + x?.StartTime));
                ReadDic(item, data.Trainings, item.HorseKey + item.StartTime);
                break;
              }
            case "WC":
              {
                var a = new JVData_Struct.JV_WC_WOODTIP();
                a.SetDataB(ref d);
                var item = WoodtipTraining.FromJV(a);

                // Read(item, data.Trainings, (a, b) => a.HorseKey == b.HorseKey && a.StartTime == b.StartTime, new ComparableComparer<Training>(x => x?.HorseKey + x?.StartTime));
                ReadDic(item, data.WoodtipTrainings, item.HorseKey + item.StartTime);
                break;
              }
            case "TC":
              {
                var a = new JVData_Struct.JV_TC_INFO();
                a.SetDataB(ref d);
                var item = RaceStartTimeChange.FromJV(a);

                Read(item, data.RaceStartTimeChanges, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<RaceStartTimeChange>(x => x?.RaceKey));
                break;
              }
            case "CC":
              {
                var a = new JVData_Struct.JV_CC_INFO();
                a.SetDataB(ref d);
                var item = RaceCourseChange.FromJV(a);

                Read(item, data.RaceCourseChanges, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<RaceCourseChange>(x => x?.RaceKey));
                break;
              }
            case "HR":
              {
                var a = new JVData_Struct.JV_HR_PAY();
                a.SetDataB(ref d);
                var item = Refund.FromJV(a);

                // Read(item, data.Refunds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<Refund>(x => x?.RaceKey));
                ReadDic(item, data.Refunds, item.RaceKey);
                break;
              }
            case "O1":
              {
                var a = new JVData_Struct.JV_O1_ODDS_TANFUKUWAKU();
                a.SetDataB(ref d);
                var item = SingleAndDoubleWinOdds.FromJV(a);
                var item2 = FrameNumberOdds.FromJV(a);

                // Read(item, data.SingleAndDoubleWinOdds, (a, b) => a.RaceKey == b.RaceKey && a.Time == b.Time, new ComparableComparer<SingleAndDoubleWinOdds>(x => x?.RaceKey + x?.Time));
                // Read(item2, data.FrameNumberOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<FrameNumberOdds>(x => x?.RaceKey));
                ReadDic(item, data.SingleAndDoubleWinOdds, item.RaceKey + item.Time);
                ReadDic(item2, data.FrameNumberOdds, item2.RaceKey);
                break;
              }
            case "O2":
              {
                var a = new JVData_Struct.JV_O2_ODDS_UMAREN();
                a.SetDataB(ref d);
                var item = QuinellaOdds.FromJV(a);

                // Read(item, data.QuinellaOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<QuinellaOdds>(x => x?.RaceKey));
                ReadDic(item, data.QuinellaOdds, item.RaceKey);
                break;
              }
            case "O3":
              {
                var a = new JVData_Struct.JV_O3_ODDS_WIDE();
                a.SetDataB(ref d);
                var item = QuinellaPlaceOdds.FromJV(a);

                // Read(item, data.QuinellaPlaceOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<QuinellaPlaceOdds>(x => x?.RaceKey));
                ReadDic(item, data.QuinellaPlaceOdds, item.RaceKey);
                break;
              }
            case "O4":
              {
                var a = new JVData_Struct.JV_O4_ODDS_UMATAN();
                a.SetDataB(ref d);
                var item = ExactaOdds.FromJV(a);

                // Read(item, data.ExactaOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<ExactaOdds>(x => x?.RaceKey));
                ReadDic(item, data.ExactaOdds, item.RaceKey);
                break;
              }
            case "O5":
              {
                var a = new JVData_Struct.JV_O5_ODDS_SANREN();
                a.SetDataB(ref d);
                var item = TrioOdds.FromJV(a);

                // Read(item, data.TrioOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<TrioOdds>(x => x?.RaceKey));
                ReadDic(item, data.TrioOdds, item.RaceKey);
                break;
              }
            case "O6":
              {
                var a = new JVData_Struct.JV_O6_ODDS_SANRENTAN();
                a.SetDataB(ref d);
                var item = TrifectaOdds.FromJV(a);

                // Read(item, data.TrifectaOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<TrifectaOdds>(x => x?.RaceKey));
                ReadDic(item, data.TrifectaOdds, item.RaceKey);
                break;
              }
            case "NR":
              {
                var a = new JVData_Struct.JV_NR_NOSI_RACE();
                a.SetDataB(ref d);
                var item = TestRace.FromJV(a);

                ReadDic(item, data.TestRaces, item.Key);
                break;
              }
            case "NS":
              {
                var a = new JVData_Struct.JV_NS_NOSI_UMA();
                a.SetDataB(ref d);
                var item = TestRaceHorse.FromJV(a);

                ReadDic(item, data.TestRaceHorses, item.Key + item.RaceKey);
                break;
              }
            case "KS":
              {
                var a = new JVData_Struct.JV_KS_KISYU();
                a.SetDataB(ref d);
                var item = Rider.FromJV(a);

                ReadDic(item, data.Riders, item.Code + item.CentralFlag);
                break;
              }
            case "CH":
              {
                var a = new JVData_Struct.JV_CH_CHOKYOSI();
                a.SetDataB(ref d);
                var item = Trainer.FromJV(a);

                ReadDic(item, data.Trainers, item.Code + item.CentralFlag);
                break;
              }
            case "DM":
              {
                var a = new JVData_Struct.JV_DM_INFO();
                a.SetDataB(ref d);
                var item = MiningTime.FromJV(a);

                ReadDic(item, data.MiningTimes, item.RaceKey);
                break;
              }
            case "TM":
              {
                var a = new JVData_Struct.JV_TM_INFO();
                a.SetDataB(ref d);
                var item = MiningMatch.FromJV(a);

                ReadDic(item, data.MiningMatches, item.RaceKey);
                break;
              }
            default:
              this.ReadedEntityCount--;
              if (!this.isRealTime)
              {
                this.link.Skip();
              }
              break;
          }
        }

        if (fileName != lastFileName)
        {
          this.ReadedCount++;
          lastFileName = fileName;
        }
      }

      this.ReadedCount = this.ReadCount;

      return data;
    }

    public void StopLoading()
    {
      this.isCanceled = true;
    }

    public void Dispose()
    {
      try
      {
        this.link.Close();
        this.link.IsOpen = false;
      }
      catch
      {

      }
    }

    [DllImport("ole32.dll", SetLastError = true)]
    static extern void CoTaskMemFree(IntPtr lpMem);
  }

  public class JVLinkReaderData
  {
    public Dictionary<string, Race> Races { get; internal set; } = new();

    public Dictionary<string, RaceHorse> RaceHorses { get; internal set; } = new();

    public Dictionary<string, Horse> Horses { get; internal set; } = new();

    public Dictionary<string, HorseBlood> HorseBloods { get; internal set; } = new();

    public Dictionary<string, HorseBloodInfo> HorseBloodInfos { get; internal set; } = new();

    public Dictionary<string, BornHorse> BornHorses { get; internal set; } = new();

    public Dictionary<string, SingleAndDoubleWinOdds> SingleAndDoubleWinOdds { get; internal set; } = new();

    public Dictionary<string, FrameNumberOdds> FrameNumberOdds { get; internal set; } = new();

    public Dictionary<string, QuinellaOdds> QuinellaOdds { get; internal set; } = new();

    public Dictionary<string, QuinellaPlaceOdds> QuinellaPlaceOdds { get; internal set; } = new();

    public Dictionary<string, ExactaOdds> ExactaOdds { get; internal set; } = new();

    public Dictionary<string, TrioOdds> TrioOdds { get; internal set; } = new();

    public Dictionary<string, TrifectaOdds> TrifectaOdds { get; internal set; } = new();

    public Dictionary<string, Refund> Refunds { get; internal set; } = new();

    public Dictionary<string, TestRace> TestRaces { get; internal set; } = new();

    public Dictionary<string, TestRaceHorse> TestRaceHorses { get; internal set; } = new();

    public Dictionary<string, Rider> Riders { get; internal set; } = new();

    public Dictionary<string, Trainer> Trainers { get; internal set; } = new();

    public List<HorseWeight> HorseWeights { get; internal set; } = new();

    public List<CourseWeatherCondition> CourseWeatherConditions { get; internal set; } = new();

    public List<HorseAbnormality> HorseAbnormalities { get; internal set; } = new();

    public List<HorseRiderChange> HorseRiderChanges { get; internal set; } = new();

    public List<RaceCourseChange> RaceCourseChanges { get; internal set; } = new();

    public List<RaceStartTimeChange> RaceStartTimeChanges { get; internal set; } = new();

    public Dictionary<string, Training> Trainings { get; internal set; } = new();

    public Dictionary<string, WoodtipTraining> WoodtipTrainings { get; internal set; } = new();

    public Dictionary<string, MiningMatch> MiningMatches { get; internal set; } = new();

    public Dictionary<string, MiningTime> MiningTimes { get; internal set; } = new();
  }

  class SimpleDistinctComparer<T> : IEqualityComparer<T>
  {
    private readonly Func<T, T, bool> action;

    public SimpleDistinctComparer(Func<T, T, bool> action)
    {
      this.action = action;
    }

    public bool Equals(T? x, T? y)
    {
      if (x != null && y != null)
      {
        return this.action(x, y);
      }
      return false;
    }

    public int GetHashCode([DisallowNull] T obj)
    {
      return obj.GetHashCode();
    }
  }
}
