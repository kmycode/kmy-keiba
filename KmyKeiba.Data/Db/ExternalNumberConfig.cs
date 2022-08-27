using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class ExternalNumberConfig : AppDataBase
  {
    public string Name { get; set; } = string.Empty;

    public string FileNamePattern { get; set; } = string.Empty;

    public ExternalNumberFileFormat FileFormat { get; set; }

    public ExternalNumberValuesFormat ValuesFormat { get; set; }

    // 常に自動判別可能
    public ExternalNumberDotFormat DotFormat { get; set; }

    public ExternalNumberSortRule SortRule { get; set; }

    public ExternalNumberRaceIdFormat RaceIdFormat { get; set; }

    public short Order { get; set; }
  }

  public enum ExternalNumberFileFormat : short
  {
    Unknown = 0,
    RaceCsv = 1,
    RaceFixedLength = 2,
    RaceHorseCsv = 3,
    RaceHorseFixedLength = 4,
  }

  public enum ExternalNumberValuesFormat : short
  {
    Unknown = 0,
    NumberOnly = 1,
    NumberAndOrder = 2,
  }

  public enum ExternalNumberDotFormat : short
  {
    Auto = 0,
    Integer = 1,
    IntegerWithSign = 2,
    Real1 = 3,
    Real2 = 4,
  }

  public enum ExternalNumberSortRule : short
  {
    Unknown = 0,
    Larger = 1,
    Smaller = 2,
    SmallerWithoutZero = 3,
  }

  public enum ExternalNumberRaceIdFormat : short
  {
    Auto = 0,
    CurrentRule = 1,
    OldRule = 2,
    ThirdRule = 3,
  }
}
