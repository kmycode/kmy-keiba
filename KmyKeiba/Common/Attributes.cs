using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  public class LabelAttribute : Attribute
  {
    public string Label { get; }

    public string? ShortLabel { get; }

    public LabelAttribute(string label)
    {
      this.Label = label;
    }

    public LabelAttribute(string label, string shortLabel)
    {
      this.Label = label;
      this.ShortLabel = shortLabel;
    }
  }

  public static class OriginalAttributeExtensions
  {
    public static string? GetLabel(this Enum obj)
    {
      var attribute = GetFieldAttribute<LabelAttribute>(obj);
      return attribute?.Label;
    }

    public static string? GetShortLabel(this Enum obj)
    {
      var attribute = GetFieldAttribute<LabelAttribute>(obj);
      return attribute?.ShortLabel ?? attribute?.Label;
    }

    private static A? GetFieldAttribute<A>(object spec) where A : Attribute
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
