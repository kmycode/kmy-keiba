using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  internal static class ResourceUtil
  {
    public static T? TryGetResource<T>(string key) where T : class
    {
      return ViewMessages.TryGetResource?.Invoke(key) as T;
    }
  }

  public class RHColor
  {
    public byte Red { get; set; }

    public byte Green { get; set; }

    public byte Blue { get; set; }

    public byte Alpha { get; set; } = 255;

    public SKColor ToSKColor()
    {
      return new SKColor(this.Red, this.Green, this.Blue, this.Alpha);
    }

    public string ToHTMLColor()
    {
      return "#" + this.Red.ToString("X2") + this.Green.ToString("X2") + this.Blue.ToString("X2");
    }
  }
}
