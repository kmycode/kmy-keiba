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

    JVLinkReaderData Load();
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

    public JVLinkReaderData Load()
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

    public JVLinkReaderData Load()
    {
      var data = new JVLinkReaderData();
      var buffSize = 110000;
      var buff = new byte[buffSize];

      var lastFileName = string.Empty;

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

        void Read<T>(T item, IList<T> list, Func<T, T, bool> isEquals)
          where T : EntityBase
        {
          var oldItem = list.FirstOrDefault((r) => isEquals(r, item) && r.LastModified < item.LastModified);
          if (oldItem != null)
          {
            list.Remove(oldItem);
          }

          list.Add(item);
        }

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
          case "O1":
            {
              var a = new JVData_Struct.JV_O1_ODDS_TANFUKUWAKU();
              a.SetDataB(ref d);
              var item = SingleAndDoubleWinOdds.FromJV(a);

              Read(item, data.SingleAndDoubleWinOdds, (a, b) => a.RaceKey == b.RaceKey);
              break;
            }
          default:
            this.link.Skip();
            break;
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
      this.link.Close();
      this.link.IsOpen = false;
    }

    [DllImport("ole32.dll", SetLastError = true)]
    static extern void CoTaskMemFree(IntPtr lpMem);
  }

  public class JVLinkReaderData
  {
    public List<Race> Races { get; internal set; } = new();

    public List<RaceHorse> RaceHorses { get; internal set; } = new();

    public List<SingleAndDoubleWinOdds> SingleAndDoubleWinOdds { get; internal set; } = new();
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
