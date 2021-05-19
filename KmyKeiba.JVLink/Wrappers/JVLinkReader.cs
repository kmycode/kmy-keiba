using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Wrappers
{
  public interface IJVLinkReader : IDisposable
  {
    int DownloadedCount => 0;

    int DownloadCount => 0;

    JVLinkReaderData Load();
  }

  class EmptyJVLinkReader : IJVLinkReader
  {
    public void Dispose()
    {
    }

    public JVLinkReaderData Load()
    {
      return new JVLinkReaderData();
    }
  }

  class JVLinkReader : IJVLinkReader
  {
    private readonly IJVLinkObject link;
    private readonly int readCount;
    private readonly int downloadCount;

    public int DownloadedCount => this.link.Status();

    public int DownloadCount => this.downloadCount;

    static JVLinkReader()
    {
      // SJISを扱う
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public JVLinkReader(IJVLinkObject link, int readCount, int downloadCount)
    {
      this.readCount = readCount;
      this.downloadCount = downloadCount;

      this.link = link;
      link.IsOpen = true;
    }

    public JVLinkReaderData Load()
    {
      var data = new JVLinkReaderData();
      var buffSize = 110000;
      var buff = new byte[buffSize];

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

        switch (spec)
        {
          case "RA":
            {
              var a = new JVData_Struct.JV_RA_RACE();
              a.SetDataB(ref d);
              var item = Race.FromJV(a);
              data.Races.Add(item);
              break;
            }
          case "SE":
            {
              var a = new JVData_Struct.JV_SE_RACE_UMA();
              a.SetDataB(ref d);
              var item = RaceHorse.FromJV(a);
              data.RaceHorses.Add(item);
              break;
            }
          default:
            this.link.Skip();
            break;
        }
      }

      data.Races = ((IEnumerable<Race>)data.Races)
        .Reverse()
        .Distinct(new SimpleDistinctComparer<Race>((a, b) => a.Key == b.Key))
        .Reverse()
        .ToList();
      data.RaceHorses = ((IEnumerable<RaceHorse>)data.RaceHorses)
        .Reverse()
        .Distinct(new SimpleDistinctComparer<RaceHorse>((a, b) => a.Name == b.Name && a.RaceKey == b.RaceKey))
        .ToList();

      return data;
    }

    public void Dispose()
    {
      this.link.Close();
      this.link.IsOpen = false;
    }
  }

  public class JVLinkReaderData
  {
    public List<Race> Races { get; internal set; } = new();

    public List<RaceHorse> RaceHorses { get; internal set; } = new();
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
