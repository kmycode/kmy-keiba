using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Wrappers
{
  public interface IJVLinkReader : IDisposable
  {
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
              var race = Race.FromJV(a);
              data.Races.RemoveAll((r) => r.Id == race.Id);
              data.Races.Add(race);
              break;
            }
          default:
            this.link.Skip();
            break;
        }
      }

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
    public List<Race> Races { get; } = new();
  }
}
