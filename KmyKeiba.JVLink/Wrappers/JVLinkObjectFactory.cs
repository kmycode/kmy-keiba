using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Wrappers
{
  public interface IJVLinkObject : IDisposable
  {
    internal bool IsOpen { get; set; }

    JVLinkObjectType Type { get; }

    int Init();

    int SetUIProperties();

    int Open(string dataspec, string fromtime, int option, ref int readcount, ref int downloadcount, out string lastfiletimestamp);

    int RtOpen(string dataspec, string key);

    int Status();

    int Gets(ref byte[] buff, int size, out string filename);

    void Skip();

    void Cancel();

    void Close();

    int FileDelete(string filename);

    int Fuku(string pattern, ref byte[] buff);

    int MVPlayWithType(string type, string key);
  }

  internal static class JVLinkObjectFactory
  {
    /// <summary>
    /// 中央競馬
    /// </summary>
    /// <returns></returns>
    public static IJVLinkObject CreateCentral() => new JVLinkObjectImpl();

    /// <summary>
    /// 地方競馬
    /// </summary>
    /// <returns></returns>
    public static IJVLinkObject CreateLocal() => new NVLinkObjectImpl();

    internal static IJVLinkObject CreateDefault() => new DefaultJVLink();

    private class DefaultJVLink : IJVLinkObject
    {
      bool IJVLinkObject.IsOpen { get; set; }

      public JVLinkObjectType Type => JVLinkObjectType.Unknown;

      public void Cancel()
      {
      }

      public void Close()
      {
      }

      public void Dispose()
      {
      }

      public int FileDelete(string filename)
      {
        return default;
      }

      public int Gets(ref byte[] buff, int size, out string filename)
      {
        filename = string.Empty;
        return default;
      }

      public int Init()
      {
        return default;
      }

      public int Open(string dataspec, string fromtime, int option, ref int readcount, ref int downloadcount, out string lastfiletimestamp)
      {
        lastfiletimestamp = string.Empty;
        return default;
      }

      public int RtOpen(string dataspec, string key)
      {
        return default;
      }

      public int SetUIProperties()
      {
        return default;
      }

      public void Skip()
      {
      }

      public int Status()
      {
        return default;
      }

      public int Fuku(string pattern, ref byte[] buff)
      {
        return default;
      }

      public int MVPlayWithType(string type, string key)
      {
        return default;
      }
    }

    private class JVLinkObjectImpl : IJVLinkObject
    {
      private readonly JVDTLabLib.JVLink link = new();

      public JVLinkObjectType Type => JVLinkObjectType.Central;

      bool IJVLinkObject.IsOpen { get; set; }

      public int Init() => this.link.JVInit(JVLinkObject.CentralInitializationKey);

      public int SetUIProperties() => this.link.JVSetUIProperties();

      public void Dispose() => this.link.JVClose();

      public int Open(string dataspec, string fromtime, int option, ref int readcount, ref int downloadcount, out string lastfiletimestamp)
        => this.link.JVOpen(dataspec, fromtime, option, ref readcount, ref downloadcount, out lastfiletimestamp);

      public int RtOpen(string dataspec, string key)
        => this.link.JVRTOpen(dataspec, key);

      public int Status() => this.link.JVStatus();

      public int Gets(ref byte[] buff, int size, out string filename)
      {
        object obj = buff;
        var r = this.link.JVGets(ref obj, size, out filename);
        buff = (byte[])obj;
        return r;
      }

      public void Skip() => this.link.JVSkip();

      public void Cancel() => this.link.JVCancel();

      public void Close()
      {
        var r = this.link.JVClose();
        if (r != 0)
        {

        }
      }

      public int FileDelete(string filename) => this.link.JVFiledelete(filename);

      public int Fuku(string pattern, ref byte[] buff)
      {
        object obj = buff;
        var r = this.link.JVFuku(pattern, ref obj);
        buff = (byte[])obj;
        return r;
      }

      public int MVPlayWithType(string type, string key) => this.link.JVMVPlayWithType(type, key);
    }

    private class NVLinkObjectImpl : IJVLinkObject
    {
      private readonly NVDTLabLib.NVLink link = new();

      public JVLinkObjectType Type => JVLinkObjectType.Local;

      bool IJVLinkObject.IsOpen { get; set; }

      public int Init() => this.link.NVInit("UNKNOWN");

      public int SetUIProperties() => this.link.NVSetUIProperties();

      public void Dispose() => this.link.NVClose();

      public int Open(string dataspec, string fromtime, int option, ref int readcount, ref int downloadcount, out string lastfiletimestamp)
        => this.link.NVOpen(dataspec, fromtime, option, ref readcount, ref downloadcount, out lastfiletimestamp);

      public int RtOpen(string dataspec, string key)
        => this.link.NVRTOpen(dataspec, key);

      public int Status() => this.link.NVStatus();

      public int Gets(ref byte[] buff, int size, out string filename)
      {
        object obj = buff;
        var r = this.link.NVGets(ref obj, size, out filename);
        buff = (byte[])obj;
        return r;
      }

      public void Skip() => this.link.NVSkip();

      public void Cancel() => this.link.NVCancel();

      public void Close() => this.link.NVClose();

      public int FileDelete(string filename) => this.link.NVFiledelete(filename);

      public int Fuku(string pattern, ref byte[] buff)
      {
        object obj = buff;
        var r = this.link.NVFuku(pattern, ref obj);
        buff = (byte[])obj;
        return r;
      }

      public int MVPlayWithType(string type, string key)
      {
        JVLinkMovieResult result = JVLinkMovieResult.Succeed;

        var courseCode = key.Substring(8, 2);
        var rakutenCourseCode = courseCode switch
        {
          "45" => "2135",      // 川崎
          "41" => "2015",      // 大井
          "43" => "1914",      // 船橋
          "42" => "1813",      // 浦和
          "36" => "1106",      // 水沢
          "30" => "3601",      // 門別
          "83" => "0304",      // 帯広（ば）
          "46" => "2218",      // 金沢
          "47" => "2320",      // 笠松
          "48" => "2433",      // 名古屋
          "50" => "2726",      // 園田
          "54" => "3129",      // 高知
          "55" => "3230",      // 佐賀
          "51" => "2826",      // 姫路
          "35" => "1006",      // 盛岡
          _ => string.Empty,
        };
        var rakutenKey = key.Substring(0, 8) + rakutenCourseCode + key.Substring(10);

        try
        {
          System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
          {
            FileName = "https://keiba.rakuten.co.jp/archivemovie/RACEID/" + rakutenKey,
            UseShellExecute = true,
          });
        }
        catch
        {
          result = JVLinkMovieResult.ServerError;
        }

        return (int)result;
      }
    }
  }
}
