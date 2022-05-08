using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  internal static class EnumUtil
  {
    public static string ToLabelString(this RunningStyle style)
    {
      return style switch
      {
        RunningStyle.FrontRunner => "逃げ",
        RunningStyle.Stalker => "先行",
        RunningStyle.Sotp => "差し",
        RunningStyle.SaveRunner => "追込",
        RunningStyle.NotClear => "不明",
        _ => string.Empty,
      };
    }
  }
}
