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
      this.link.Close();
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

    static JVLinkReader()
    {
      // SJISを扱う
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public JVLinkReader(IJVLinkObject link, int readCount, int downloadCount)
    {
      this.ReadCount = readCount;
      this.DownloadCount = downloadCount;

      this.link = link;
      link.IsOpen = true;
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
                this.link.Close();
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

        void Read<T>(T item, IList<T> list, Func<T, T, bool> isEquals)
          where T : EntityBase
        {
          var oldItem = list.FirstOrDefault((r) => isEquals(r, item) && r.LastModified < item.LastModified);
          if (oldItem != null)
          {
            list.Remove(oldItem);
          }

          var newItem = list.FirstOrDefault((r) => isEquals(r, item) && r.LastModified >= item.LastModified);
          if (newItem == null)
          {
            list.Add(item);
          }
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

                Read(item, data.Races, (a, b) => a.Key == b.Key);
                break;
              }
            case "SE":
              {
                var a = new JVData_Struct.JV_SE_RACE_UMA();
                a.SetDataB(ref d);
                var item = RaceHorse.FromJV(a);

                Read(item, data.RaceHorses, (a, b) => a.RaceKey == b.RaceKey && a.Name == b.Name);
                break;
              }
            case "WH":
              {
                var a = new JVData_Struct.JV_WH_BATAIJYU();
                a.SetDataB(ref d);
                var item = HorseWeight.FromJV(a);

                Read(item, data.HorseWeights, (a, b) => a.RaceKey == b.RaceKey);
                break;
              }
            case "WE":
              {
                var a = new JVData_Struct.JV_WE_WEATHER();
                a.SetDataB(ref d);
                var item = CourseWeatherCondition.FromJV(a);

                Read(item, data.CourseWeatherConditions, (a, b) => a.RaceKeyWithoutRaceNum == b.RaceKeyWithoutRaceNum);
                break;
              }
            case "AV":
              {
                var a = new JVData_Struct.JV_AV_INFO();
                a.SetDataB(ref d);
                var item = HorseAbnormality.FromJV(a);

                Read(item, data.HorseAbnormalities, (a, b) => a.RaceKey + a.HorseNumber == b.RaceKey + b.HorseNumber);
                break;
              }
            case "UM":
              {
                var a = new JVData_Struct.JV_UM_UMA();
                a.SetDataB(ref d);
                var item = Horse.FromJV(a);

                Read(item, data.Horses, (a, b) => a.Code == b.Code);
                break;
              }
            case "HN":
              {
                var a = new JVData_Struct.JV_HN_HANSYOKU();
                a.SetDataB(ref d);
                var item = HorseBlood.FromJV(a);

                Read(item, data.HorseBloods, (a, b) => a.Key == b.Key);
                break;
              }
            case "JC":
              {
                var a = new JVData_Struct.JV_JC_INFO();
                a.SetDataB(ref d);
                var item = HorseRiderChange.FromJV(a);

                Read(item, data.HorseRiderChanges, (a, b) => a.RaceKey + a.HorseNumber == b.RaceKey + b.HorseNumber);
                break;
              }
            case "HC":
              {
                var a = new JVData_Struct.JV_HC_HANRO();
                a.SetDataB(ref d);
                var item = Training.FromJV(a);

                Read(item, data.Trainings, (a, b) => a.HorseKey == b.HorseKey && a.StartTime == b.StartTime);
                break;
              }
            case "HR":
              {
                var a = new JVData_Struct.JV_HR_PAY();
                a.SetDataB(ref d);
                var item = Refund.FromJV(a);

                Read(item, data.Refunds, (a, b) => a.RaceKey == b.RaceKey);
                break;
              }
            case "O1":
              {
                var a = new JVData_Struct.JV_O1_ODDS_TANFUKUWAKU();
                a.SetDataB(ref d);
                var item = SingleAndDoubleWinOdds.FromJV(a);
                var item2 = FrameNumberOdds.FromJV(a);

                Read(item, data.SingleAndDoubleWinOdds, (a, b) => a.RaceKey == b.RaceKey && a.Time == b.Time);
                Read(item2, data.FrameNumberOdds, (a, b) => a.RaceKey == b.RaceKey);
                break;
              }
            case "O2":
              {
                var a = new JVData_Struct.JV_O2_ODDS_UMAREN();
                a.SetDataB(ref d);
                var item = QuinellaOdds.FromJV(a);

                Read(item, data.QuinellaOdds, (a, b) => a.RaceKey == b.RaceKey);
                break;
              }
            case "O3":
              {
                var a = new JVData_Struct.JV_O3_ODDS_WIDE();
                a.SetDataB(ref d);
                var item = QuinellaPlaceOdds.FromJV(a);

                Read(item, data.QuinellaPlaceOdds, (a, b) => a.RaceKey == b.RaceKey);
                break;
              }
            case "O4":
              {
                var a = new JVData_Struct.JV_O4_ODDS_UMATAN();
                a.SetDataB(ref d);
                var item = ExactaOdds.FromJV(a);

                Read(item, data.ExactaOdds, (a, b) => a.RaceKey == b.RaceKey);
                break;
              }
            case "O5":
              {
                var a = new JVData_Struct.JV_O5_ODDS_SANREN();
                a.SetDataB(ref d);
                var item = TrioOdds.FromJV(a);

                Read(item, data.TrioOdds, (a, b) => a.RaceKey == b.RaceKey);
                break;
              }
            case "O6":
              {
                var a = new JVData_Struct.JV_O6_ODDS_SANRENTAN();
                a.SetDataB(ref d);
                var item = TrifectaOdds.FromJV(a);

                Read(item, data.TrifectaOdds, (a, b) => a.RaceKey == b.RaceKey);
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
    public List<Race> Races { get; internal set; } = new();

    public List<RaceHorse> RaceHorses { get; internal set; } = new();

    public List<Horse> Horses { get; internal set; } = new();

    public List<HorseBlood> HorseBloods { get; internal set; } = new();

    public List<SingleAndDoubleWinOdds> SingleAndDoubleWinOdds { get; internal set; } = new();

    public List<FrameNumberOdds> FrameNumberOdds { get; internal set; } = new();

    public List<QuinellaOdds> QuinellaOdds { get; internal set; } = new();

    public List<QuinellaPlaceOdds> QuinellaPlaceOdds { get; internal set; } = new();

    public List<ExactaOdds> ExactaOdds { get; internal set; } = new();

    public List<TrioOdds> TrioOdds { get; internal set; } = new();

    public List<TrifectaOdds> TrifectaOdds { get; internal set; } = new();

    public List<Refund> Refunds { get; internal set; } = new();

    public List<HorseWeight> HorseWeights { get; internal set; } = new();

    public List<CourseWeatherCondition> CourseWeatherConditions { get; internal set; } = new();

    public List<HorseAbnormality> HorseAbnormalities { get; internal set; } = new();

    public List<HorseRiderChange> HorseRiderChanges { get; internal set; } = new();

    public List<Training> Trainings { get; internal set; } = new();
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
