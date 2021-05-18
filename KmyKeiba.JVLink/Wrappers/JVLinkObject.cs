using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Wrappers
{
  public class JVLinkObject : IDisposable
  {
    private static JVLinkObject? _central;
    private static JVLinkObject? _local;
    public static JVLinkObject Central => _central ??= new JVLinkObject(JVLinkObjectType.Central);
    public static JVLinkObject Local => _local ??= new JVLinkObject(JVLinkObjectType.Local);

    private enum JVLinkObjectType
    {
      Central,
      Local,
    }

    private readonly IJVLinkObject link;
    private bool hasInitialized = false;

    private JVLinkObject(JVLinkObjectType type)
    {
      this.link = type switch
      {
        JVLinkObjectType.Central => JVLinkObjectFactory.CreateCentral(),
        JVLinkObjectType.Local => JVLinkObjectFactory.CreateLocal(),
        _ => throw new ArgumentException(),
      };
    }

    private void CheckInitialized()
    {
      if (!this.hasInitialized)
      {
        var r = this.link.Init();
        if (r != 0)
        {
          throw JVLinkException.GetError(JVLinkInitializeResult.Unknown);
        }
        this.hasInitialized = true;
      }
    }

    public void Dispose() => this.link.Dispose();
  }

  public class JVLinkReader : IDisposable
  {
    private readonly IJVLinkObject link;

    static JVLinkReader()
    {
      // SJISを扱う
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public JVLinkReader(IJVLinkObject link)
    {
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
        if (result != 0 && result != -1)
        {
          var isBreak = true;
          switch (result)
          {
            case -3:
              {
                throw JVLinkException.GetError(JVLinkLoadResult.Downloading);
              }
            case -403:
              {
                link.FileDelete(fileName);
                isBreak = false;
                break;
              }
            case -503:
              {
                isBreak = false;
                break;
              }
          }

          if (isBreak)
          {
            break;
          }
          else
          {
            continue;
          }
        }

        var d = Encoding.GetEncoding(932).GetString(buff);
        var spec = d.Substring(0, 2);

        switch (spec)
        {
          case "RA":
            {
              var a = new JVData_Struct.JV_RA_RACE();
              a.SetDataB(ref d);
              data.Races.Add(Race.FromJV(a));
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

  [Flags]
  public enum JVLinkOpenOption
  {
    Normal = 1,
    ThisWeek = 2,
    Setup = 4,
    SetupWithoutDialog = 8,
    All = 255,
    SetupAll = 12,
    WithoutThisWeek = 13,
  }

  public class JVLinkDataspecAttribute : Attribute
  {
    public string Code { get; init; }

    public JVLinkOpenOption Options { get; init; }

    public JVLinkDataspecAttribute(string code, JVLinkOpenOption options)
    {
      this.Code = code;
      this.Options = options;
    }
  }

  [Flags]
  enum JVLinkDataspec
  {
    [JVLinkDataspec("TOKU", JVLinkOpenOption.All)]
    Toku = 1,

    [JVLinkDataspec("RACE", JVLinkOpenOption.All)]
    Race = 2,

    [JVLinkDataspec("DIFF", JVLinkOpenOption.WithoutThisWeek)]
    Diff = 4,

    [JVLinkDataspec("BLOD", JVLinkOpenOption.WithoutThisWeek)]
    Blod = 8,

    [JVLinkDataspec("MING", JVLinkOpenOption.WithoutThisWeek)]
    Ming = 16,

    [JVLinkDataspec("SNAP", JVLinkOpenOption.All)]
    Snap = 32,

    [JVLinkDataspec("SLOP", JVLinkOpenOption.WithoutThisWeek)]
    Slop = 64,

    [JVLinkDataspec("YSCH", JVLinkOpenOption.WithoutThisWeek)]
    Ysch = 128,

    [JVLinkDataspec("HOSE", JVLinkOpenOption.WithoutThisWeek)]
    Hose = 256,

    [JVLinkDataspec("HOYU", JVLinkOpenOption.WithoutThisWeek)]
    Hoyu = 512,

    [JVLinkDataspec("COMM", JVLinkOpenOption.WithoutThisWeek)]
    Comm = 1024,

    [JVLinkDataspec("TCOV", JVLinkOpenOption.ThisWeek)]
    Tcov = 2048,

    [JVLinkDataspec("RCOV", JVLinkOpenOption.ThisWeek)]
    Rcov = 4096,
  }

  static class EnumExtensions
  {
    public static JVLinkDataspecAttribute? GetAttribute(this JVLinkDataspec spec)
    {
      var type = spec.GetType();
      var fieldInfo = type.GetField(spec.ToString()!);
      if (fieldInfo == null)
      {
        return null;
      }
      var attributes = fieldInfo.GetCustomAttributes(typeof(JVLinkDataspecAttribute), false) as JVLinkDataspecAttribute[];
      if (attributes != null && attributes.Length > 0)
      {
        return attributes[0];
      }
      return null;
    }
  }

  public class JVLinkCodeAttribute : Attribute
  {
    public string Message { get; set; }

    public JVLinkCodeAttribute(string message)
    {
      this.Message = message;
    }
  }

  public class JVLinkException<T> : Exception where T : System.Enum
  {
    public T Code { get; init; }

    public JVLinkException(T code) : base()
    {
      this.Code = code;
    }

    public JVLinkException(T code, Exception inner) : base(string.Empty, inner)
    {
      this.Code = code;
    }
  }

  public enum JVLinkCommonCode
  {
    [JVLinkCode("不明")]
    Unknown,
  }

  public enum JVLinkInitializeResult
  {
    [JVLinkCode("不明")]
    Unknown,
  }

  public enum JVLinkLoadResult
  {
    [JVLinkCode("データダウンロード途中")]
    Downloading,
  }

  public class JVLinkException : JVLinkException<JVLinkCommonCode>
  {
    public static JVLinkException<T> GetError<T>(T code) where T : System.Enum => new JVLinkException<T>(code);
    public static JVLinkException<T> GetError<T>(T code, Exception inner) where T : System.Enum => new JVLinkException<T>(code, inner);

    public static JVLinkCodeAttribute GetAttribute(object code)
    {
      var type = code.GetType();
      var fieldInfo = type.GetField(code.ToString()!);
      if (fieldInfo == null)
      {
        return new JVLinkCodeAttribute("不明");
      }
      var attributes = fieldInfo.GetCustomAttributes(typeof(JVLinkCodeAttribute), false) as JVLinkCodeAttribute[];
      if (attributes != null && attributes.Length > 0)
      {
        return attributes[0];
      }
      return new JVLinkCodeAttribute("不明");
    }

    public JVLinkException() : base(JVLinkCommonCode.Unknown)
    {
    }

    public JVLinkException(Exception inner) : base(JVLinkCommonCode.Unknown, inner)
    {
    }
  }
}
