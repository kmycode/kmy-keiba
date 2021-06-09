using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Wrappers
{

  public static class EnumExtensions
  {
    internal static RaceClassInfoAttribute? GetAttribute(this RaceClass cls)
      => GetAttribute<RaceClassInfoAttribute, RaceClass>(cls);

    internal static RaceCourseInfoAttribute? GetAttribute(this RaceCourse cls)
      => GetAttribute<RaceCourseInfoAttribute, RaceCourse>(cls);

    internal static RaceGradeInfoAttribute? GetAttribute(this RaceGrade cls)
      => GetAttribute<RaceGradeInfoAttribute, RaceGrade>(cls);

    internal static RaceSubjectTypeInfoAttribute? GetAttribute(this RaceSubjectType cls)
      => GetAttribute<RaceSubjectTypeInfoAttribute, RaceSubjectType>(cls);

    internal static RaceAbnormalityInfoAttribute? GetAttribute(this RaceAbnormality cls)
      => GetAttribute<RaceAbnormalityInfoAttribute, RaceAbnormality>(cls);

    public static string GetName(this RaceCourse cls)
      => cls.GetAttribute()?.Name ?? string.Empty;

    public static RaceCourseType GetCourseType(this RaceCourse cls)
      => cls.GetAttribute()?.Type ?? RaceCourseType.Unknown;

    public static string GetLabel(this RaceGrade cls)
      => cls.GetAttribute()?.Label ?? string.Empty;

    public static string GetLabel(this RaceSubjectType cls)
      => cls.GetAttribute()?.Label ?? string.Empty;

    public static string GetLabel(this RaceAbnormality cls)
      => cls.GetAttribute()?.Label ?? string.Empty;

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

  public class JVLinkCodeAttribute : Attribute
  {
    public string Message { get; }

    public JVLinkCodeAttribute(string message)
    {
      this.Message = message;
    }
  }
}
