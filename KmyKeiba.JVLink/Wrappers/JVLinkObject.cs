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

    public static string CentralInitializationKey { get; set; } = "SA000000/SD000004";

    private readonly IJVLinkObject link;
    private bool hasInitialized = false;

    public bool IsError { get; private set; }

    public JVLinkObjectType Type { get; }

    public int MainWindowHandle
    {
      set => this.link.MainWindowHandle = value;
    }

    private JVLinkObject(JVLinkObjectType type)
    {
      this.Type = type;

      try
      {
        this.link = type switch
        {
          JVLinkObjectType.Central => JVLinkObjectFactory.CreateCentral(),
          JVLinkObjectType.Local => JVLinkObjectFactory.CreateLocal(),
          _ => throw new ArgumentException(),
        };
      }
      catch
      {
        this.IsError = true;
        this.link = JVLinkObjectFactory.CreateDefault();
      }
    }

    public void OpenConfigWindow()
    {
      this.link.SetUIProperties();
    }

    public JVLinkMovieResult PlayMovie(JVLinkMovieType type, string key)
    {
      this.CheckInitialized();

      var result = this.link.MVPlayWithType(((short)type).ToString("00"), key);
      if (result != 0)
      {
        throw JVLinkException.GetError((JVLinkMovieResult)result);
      }

      return (JVLinkMovieResult)result;
    }

    public JVLinkMovieReader OpenMovie(JVLinkTrainingMovieType type, string key)
    {
      this.CheckInitialized();

      var result = this.link.MVOpen(((short)type).ToString(), key);
      if (result != 0)
      {
        throw new JVLinkException<JVLinkMovieResult>((JVLinkMovieResult)result);
      }

      return new JVLinkMovieReader(this.link);
    }

    public IJVLinkReader StartRead(JVLinkDataspec dataspec, JVLinkOpenOption options, DateTime from, DateTime? to = null)
    {
      return this.StartRead(dataspec, options, from, to, null);
    }

    public IJVLinkReader StartRead(JVLinkDataspec dataspec, JVLinkOpenOption options, string raceKey)
    {
      return this.StartRead(dataspec, options, null, null, raceKey);
    }

    private IJVLinkReader StartRead(JVLinkDataspec dataspec, JVLinkOpenOption options, DateTime? from, DateTime? to, string? raceKey)
    {
      this.CheckInitialized();
      this.CheckOpen();

      if (options == JVLinkOpenOption.RealTime && to != null)
      {
        throw new ArgumentException();
      }

      var specs = new List<JVLinkDataspec>();
      {
        var specId = (uint)dataspec;
        var num = 1u;
        for (var i = 0; i < 32; i++)
        {
          var isHit = (specId & num) != 0;
          if (isHit)
          {
            specs.Add((JVLinkDataspec)num);
          }
          num <<= 1;
        }
      }

      if (options == JVLinkOpenOption.RealTime && specs.Count > 1)
      {
        throw JVLinkException.GetError(JVLinkLoadResult.InvalidDataspec);
      }
      if (!specs.Any())
      {
        throw JVLinkException.GetError(JVLinkLoadResult.InvalidDataspec);
      }

      var attributes = specs
        .Select((s) => s.GetAttribute())
        .Where((s) => s != null)
        .ToArray();
      if (attributes.Any((a) => !a!.Options.HasFlag(options)))
      {
        throw JVLinkException.GetError(JVLinkLoadResult.InvalidDataspec);
      }

      var readCount = 0;
      var downloadCount = 0;
      var result = 0;

      if (options != JVLinkOpenOption.RealTime)
      {
        from ??= DateTime.Today;
        var during = this.ToString((DateTime)from) + (to != null ? $"-{this.ToString((DateTime)to)}" : string.Empty);
        result = this.link.Open(string.Join(string.Empty, attributes.Select((a) => a!.Code)),
                                during, (int)options,
                                ref readCount, ref downloadCount, out string lastFileTimeStamp);
      }
      else
      {
        var key = from != null ? ((DateTime)from).ToString("yyyyMMdd")! : raceKey!;
        result = this.link.RtOpen(string.Join(string.Empty, attributes.Select((a) => a!.Code)),
                                  key);
      }

      if (result != 0 && result != -1)
      {
        throw JVLinkException.GetError((JVLinkLoadResult)result);
      }

      if (result != -1)
      {
        return new JVLinkReader(this.link, readCount, downloadCount, from, to, options == JVLinkOpenOption.RealTime);
      }

      return new EmptyJVLinkReader(this.link);
    }

    public byte[] GetUniformBitmap(string format)
    {
      this.CheckInitialized();

      var buff = new byte[3 * 50 * 50];
      var result = this.link.Fuku(format, ref buff);

      if (result == 0 || result == -1)
      {
        return buff;
      }

      throw JVLinkException.GetError((JVLinkUniformResult)result);
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

    private void CheckOpen()
    {
      if (this.link.IsOpen)
      {
        throw JVLinkException.GetError(JVLinkLoadResult.AlreadyOpen);
      }
    }

    private string ToString(DateTime dt)
    {
      return dt.ToString("yyyyMMddHHmmss");
    }

    public void Dispose() => this.link.Dispose();
  }

  public enum JVLinkObjectType
  {
    Unknown,
    Central,
    Local,
  }

  [Flags]
  public enum JVLinkOpenOption
  {
    Normal = 1,
    ThisWeek = 2,
    Setup = 4,
    SetupWithoutDialog = 8,
    All = 15,
    SetupAll = 12,
    WithoutThisWeek = 13,

    RealTime = 16,
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
  public enum JVLinkDataspec : ulong
  {
    [JVLinkDataspec("TOKU", JVLinkOpenOption.All)]
    Toku = 0b1,

    [JVLinkDataspec("RACE", JVLinkOpenOption.All)]
    Race = 0b10,

    [JVLinkDataspec("DIFN", JVLinkOpenOption.WithoutThisWeek)]
    Diff = 0b100,

    [JVLinkDataspec("BLDN", JVLinkOpenOption.WithoutThisWeek)]
    Blod = 0b1000,

    [JVLinkDataspec("MING", JVLinkOpenOption.WithoutThisWeek)]
    Ming = 0b1_0000,

    [JVLinkDataspec("SNPN", JVLinkOpenOption.All)]
    Snap = 0b10_0000,

    [JVLinkDataspec("SLOP", JVLinkOpenOption.WithoutThisWeek)]
    Slop = 0b100_0000,

    [JVLinkDataspec("YSCH", JVLinkOpenOption.WithoutThisWeek)]
    Ysch = 0b1000_0000,

    [JVLinkDataspec("HOSN", JVLinkOpenOption.WithoutThisWeek)]
    Hose = 0b1_0000_0000,

    [JVLinkDataspec("HOYU", JVLinkOpenOption.WithoutThisWeek)]
    Hoyu = 0b10_0000_0000,

    [JVLinkDataspec("COMM", JVLinkOpenOption.WithoutThisWeek)]
    Comm = 0b100_0000_0000,

    [JVLinkDataspec("TCOV", JVLinkOpenOption.ThisWeek)]
    Tcov = 0b1000_0000_0000,

    [JVLinkDataspec("RCOV", JVLinkOpenOption.ThisWeek)]
    Rcov = 0b1_0000_0000_0000,

    [JVLinkDataspec("0B12", JVLinkOpenOption.RealTime)]
    RB12 = 0b10_0000_0000_0000,

    [JVLinkDataspec("0B13", JVLinkOpenOption.RealTime)]
    RB13 = 0b100_0000_0000_0000,

    [JVLinkDataspec("0B14", JVLinkOpenOption.RealTime)]
    RB14 = 0b1000_0000_0000_0000,

    [JVLinkDataspec("0B15", JVLinkOpenOption.RealTime)]
    RB15 = 0b1_0000_0000_0000_0000,

    [JVLinkDataspec("0B16", JVLinkOpenOption.RealTime)]
    RB16 = 0b10_0000_0000_0000_0000,

    [JVLinkDataspec("0B17", JVLinkOpenOption.RealTime)]
    RB17 = 0b100_0000_0000_0000_0000,

    [JVLinkDataspec("0B11", JVLinkOpenOption.RealTime)]
    RB11 = 0b1000_0000_0000_0000_0000,

    [JVLinkDataspec("0B20", JVLinkOpenOption.RealTime)]
    RB20 = 0b1_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B30", JVLinkOpenOption.RealTime)]
    RB30 = 0b10_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B31", JVLinkOpenOption.RealTime)]
    RB31 = 0b100_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B32", JVLinkOpenOption.RealTime)]
    RB32 = 0b1000_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B33", JVLinkOpenOption.RealTime)]
    RB33 = 0b1_0000_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B34", JVLinkOpenOption.RealTime)]
    RB34 = 0b10_0000_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B35", JVLinkOpenOption.RealTime)]
    RB35 = 0b100_0000_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B36", JVLinkOpenOption.RealTime)]
    RB36 = 0b1000_0000_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B41", JVLinkOpenOption.RealTime)]
    RB41 = 0b1_0000_0000_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B42", JVLinkOpenOption.RealTime)]
    RB42 = 0b10_0000_0000_0000_0000_0000_0000_0000,

    [JVLinkDataspec("0B51", JVLinkOpenOption.RealTime)]
    RB51 = 0b100_0000_0000_0000_0000_0000_0000_0000,

    [JVLinkDataspec("WOOD", JVLinkOpenOption.WithoutThisWeek)]
    Wood = 0b1000_0000_0000_0000_0000_0000_0000_0000,

    [JVLinkDataspec("NOSI", JVLinkOpenOption.WithoutThisWeek)]
    Nosi = 0b1_0000_0000_0000_0000_0000_0000_0000_0000,
  }

  public enum JVLinkMovieType
  {
    Race = 0,
    Paddock = 1,
    MultiCameras = 2,
    Patrol = 3,
    Training = 11,
  }

  public enum JVLinkTrainingMovieType
  {
    Weekly = 11,
    WeekAndHorse = 12,
    Horse = 13,
  }

  public static class JVLinkExtensions
  {
    internal static JVLinkDataspecAttribute? GetAttribute(this JVLinkDataspec spec)
      => GetAttribute<JVLinkDataspecAttribute, JVLinkDataspec>(spec);

    private static A? GetAttribute<A, T>(T spec) where A : Attribute
    {
      if (spec == null)
      {
        return null;
      }

      var type = spec.GetType();
      var fieldInfo = type.GetField(spec.ToString()!);
      if (fieldInfo == null)
      {
        return null;
      }
      var attributes = fieldInfo.GetCustomAttributes(typeof(A), false) as A[];
      if (attributes != null && attributes.Length > 0)
      {
        return attributes[0];
      }
      return null;
    }
  }
}
