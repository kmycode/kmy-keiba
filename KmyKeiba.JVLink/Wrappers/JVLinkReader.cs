using KmyKeiba.JVLink.Entities;
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

    JVLinkReaderData Load(IEnumerable<string>? targetSpecs = null);
  }

  class EmptyJVLinkReader : IJVLinkReader
  {
    private readonly IJVLinkObject link;

    public EmptyJVLinkReader(IJVLinkObject link)
    {
      this.link = link;
    }

    public void Dispose()
    {
      // this.link.Close();
      this.link.IsOpen = false;
    }

    public JVLinkReaderData Load(IEnumerable<string>? targetSpecs = null)
    {
      return new JVLinkReaderData();
    }
  }

  class JVLinkReader : IJVLinkReader
  {
    private readonly IJVLinkObject link;

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

    public JVLinkReader(IJVLinkObject link, int readCount, int downloadCount, DateTime? start, DateTime? end)
    {
      this.ReadCount = readCount;
      this.DownloadCount = downloadCount;

      this.link = link;
      link.IsOpen = true;

      this.StartDate = start ?? default;
      this.EndDate = end ?? default;
    }

    public JVLinkReaderData Load(IEnumerable<string>? targetSpecs = null)
    {
      var data = new JVLinkReaderData();
      var buffSize = 110000;
      var buff = new byte[buffSize];

      var lastFileName = string.Empty;
      this.ReadedEntityCount = 0;

      while (true)
      {
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

        var d = Encoding.GetEncoding(932).GetString(buff);
        var spec = d.Substring(0, 2);

        void Read<T>(T item, List<T> list, Func<T, T, bool> isEquals, IComparer<T> comparer)
          where T : EntityBase
        {
          /*
          var oldItemIndex = list.BinarySearch(item, comparer);
          if (oldItemIndex >= 0)
          {
            var oldItem = list[oldItemIndex];
            if (oldItem.LastModified < item.LastModified)
            {
              list[oldItemIndex] = item;
            }
          }
          else
          {
            list.Add(item);
            list.Sort(comparer);
          }
          */
          /*
          if (item.LastModified != default && (item.LastModified < this.StartDate || item.LastModified > this.EndDate))
          {
            this.ReadedEntityCount--;
            return;
          }
          */

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
          list.TryGetValue(key, out var oldItem);
          if (oldItem != null)
          {
            // if (oldItem.LastModified < item.LastModified)
            // {
            //   list.Remove(key);
            // }
            if (oldItem.LastModified >= item.LastModified)
            {
              return;
            }
          }

          list[key] = item;
        }

        if (targetSpecs != null && targetSpecs.Any() && !targetSpecs.Contains(spec))
        {
          this.link.Skip();
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
                ReadDic(item, data.Horses, item.Code);
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

                Read(item, data.SingleAndDoubleWinOdds, (a, b) => a.RaceKey == b.RaceKey && a.Time == b.Time, new ComparableComparer<SingleAndDoubleWinOdds>(x => x?.RaceKey + x?.Time));
                Read(item2, data.FrameNumberOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<FrameNumberOdds>(x => x?.RaceKey));
                break;
              }
            case "O2":
              {
                var a = new JVData_Struct.JV_O2_ODDS_UMAREN();
                a.SetDataB(ref d);
                var item = QuinellaOdds.FromJV(a);

                Read(item, data.QuinellaOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<QuinellaOdds>(x => x?.RaceKey));
                break;
              }
            case "O3":
              {
                var a = new JVData_Struct.JV_O3_ODDS_WIDE();
                a.SetDataB(ref d);
                var item = QuinellaPlaceOdds.FromJV(a);

                Read(item, data.QuinellaPlaceOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<QuinellaPlaceOdds>(x => x?.RaceKey));
                break;
              }
            case "O4":
              {
                var a = new JVData_Struct.JV_O4_ODDS_UMATAN();
                a.SetDataB(ref d);
                var item = ExactaOdds.FromJV(a);

                Read(item, data.ExactaOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<ExactaOdds>(x => x?.RaceKey));
                break;
              }
            case "O5":
              {
                var a = new JVData_Struct.JV_O5_ODDS_SANREN();
                a.SetDataB(ref d);
                var item = TrioOdds.FromJV(a);

                Read(item, data.TrioOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<TrioOdds>(x => x?.RaceKey));
                break;
              }
            case "O6":
              {
                var a = new JVData_Struct.JV_O6_ODDS_SANRENTAN();
                a.SetDataB(ref d);
                var item = TrifectaOdds.FromJV(a);

                Read(item, data.TrifectaOdds, (a, b) => a.RaceKey == b.RaceKey, new ComparableComparer<TrifectaOdds>(x => x?.RaceKey));
                break;
              }
            default:
              this.ReadedEntityCount--;
              this.link.Skip();
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

    public void Dispose()
    {
      try
      {
        if (this.ReadedEntityCount > 0)
        {
          this.link.Close();
        }
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

    public List<SingleAndDoubleWinOdds> SingleAndDoubleWinOdds { get; internal set; } = new();

    public List<FrameNumberOdds> FrameNumberOdds { get; internal set; } = new();

    public List<QuinellaOdds> QuinellaOdds { get; internal set; } = new();

    public List<QuinellaPlaceOdds> QuinellaPlaceOdds { get; internal set; } = new();

    public List<ExactaOdds> ExactaOdds { get; internal set; } = new();

    public List<TrioOdds> TrioOdds { get; internal set; } = new();

    public List<TrifectaOdds> TrifectaOdds { get; internal set; } = new();

    public Dictionary<string, Refund> Refunds { get; internal set; } = new();

    public List<HorseWeight> HorseWeights { get; internal set; } = new();

    public List<CourseWeatherCondition> CourseWeatherConditions { get; internal set; } = new();

    public List<HorseAbnormality> HorseAbnormalities { get; internal set; } = new();

    public List<HorseRiderChange> HorseRiderChanges { get; internal set; } = new();

    public Dictionary<string, Training> Trainings { get; internal set; } = new();
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
