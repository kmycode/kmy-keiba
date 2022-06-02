using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Wrappers
{
  public class JVLinkMovieReader : IDisposable
  {
    private readonly IJVLinkObject link;

    static JVLinkMovieReader()
    {
      // SJISを扱う
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    internal JVLinkMovieReader(IJVLinkObject link)
    {
      this.link = link;
      link.IsOpen = true;
    }

    public string[] ReadKeys()
    {
      // throw new NotImplementedException("実装方法がわかりません");

      var size = 18 + 1;
      var list = new List<string>();

      int result;
      var buff = new string(Enumerable.Repeat(' ', size).ToArray());

      while ((result = this.link.MVRead(out buff, out size)) != 0)
      {
        if (result > 0)
        {
          list.Add(buff.ToString());
        }
        else
        {
          throw new JVLinkException<JVLinkMovieResult>((JVLinkMovieResult)result);
        }
      }

      return list.ToArray();
    }

    public void Dispose()
    {
      this.link.Close();
    }
  }
}
