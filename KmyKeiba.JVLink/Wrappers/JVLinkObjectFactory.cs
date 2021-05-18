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
  }

  public static class JVLinkObjectFactory
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

    private class JVLinkObjectImpl : IJVLinkObject
    {
      private readonly JVDTLabLib.JVLink link = new();

      bool IJVLinkObject.IsOpen { get; set; }

      public int Init() => this.link.JVInit("UNKNOWN");

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
        return this.link.JVGets(ref obj, size, out filename);
      }

      public void Skip() => this.link.JVSkip();

      public void Cancel() => this.link.JVCancel();

      public void Close() => this.link.JVClose();

      public int FileDelete(string filename) => this.link.JVFiledelete(filename);
    }

    private class NVLinkObjectImpl : IJVLinkObject
    {
      private readonly NVDTLabLib.NVLink link = new();

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
        return this.link.NVGets(ref obj, size, out filename);
      }

      public void Skip() => this.link.NVSkip();

      public void Cancel() => this.link.NVCancel();

      public void Close() => this.link.NVClose();

      public int FileDelete(string filename) => this.link.NVFiledelete(filename);
    }
  }
}
