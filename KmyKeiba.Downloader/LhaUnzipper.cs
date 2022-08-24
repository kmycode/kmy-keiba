using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Downloader
{
  internal partial class Program
  {
    private static void UnlhaFile(string path, string dist)
    {
      try
      {
        var output = new StringBuilder();
        var result = Unlha(IntPtr.Zero, $"x -r2 -jf0 -jso1 {path} {dist}", output, output.Capacity);
        if (result < 0)
        {
          throw new Exception(output.ToString());
        }
      }
      catch (Exception ex)
      {
        logger.Error("LHA解凍でエラー発生", ex);
      }
    }

    [DllImport("UNLHA32.DLL", CharSet = CharSet.Ansi)]
    private extern static int Unlha(
      IntPtr hwnd,            // ウィンドウハンドル
      string szCmdLine,       // コマンドライン
      StringBuilder szOutput, // 処理結果文字列
      int dwSize);            // 引数szOutputの文字列サイズ
  }
}
