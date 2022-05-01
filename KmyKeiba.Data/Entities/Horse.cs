using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class Horse : EntityBase
  {
    internal Horse()
    {
    }

    /// <summary>
    /// 血統登録番号
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public DateTime Entried { get; set; }

    public DateTime Retired { get; set; }

    public DateTime Born { get; set; }

    public string Name { get; set; } = string.Empty;

    public HorseSex Sex { get; set; }

    public HorseType Type { get; set; }

    public HorseBodyColor Color { get; set; }

    public string FatherBreedingCode { get; set; } = string.Empty;

    public string MotherBreedingCode { get; set; } = string.Empty;

    public string FFBreedingCode { get; set; } = string.Empty;

    public string FMBreedingCode { get; set; } = string.Empty;

    public string MFBreedingCode { get; set; } = string.Empty;

    public string MMBreedingCode { get; set; } = string.Empty;

    public string FFFBreedingCode { get; set; } = string.Empty;

    public string FFMBreedingCode { get; set; } = string.Empty;

    public string FMFBreedingCode { get; set; } = string.Empty;

    public string FMMBreedingCode { get; set; } = string.Empty;

    public string MFFBreedingCode { get; set; } = string.Empty;

    public string MFMBreedingCode { get; set; } = string.Empty;

    public string MMFBreedingCode { get; set; } = string.Empty;

    public string MMMBreedingCode { get; set; } = string.Empty;

    public HorseBelongs Belongs { get; set; }

    public string TrainerCode { get; set; } = string.Empty;

    public string TrainerName { get; set; } = string.Empty;

    public string InviteFrom { get; set; } = string.Empty;

    public string ProducingCode { get; set; } = string.Empty;

    public string OwnerCode { get; set; } = string.Empty;

    internal static HorseType ToHorseType(int tc)
    {
      var type = HorseType.Unknown;
      if (tc == 1 || tc == 7) type |= HorseType.LotteryA;
      if (tc == 2) type |= HorseType.LotteryB;
      if (tc == 3 || tc == 7 || tc == 8 || tc == 9 || tc == 12 || tc == 17 || tc == 19 || tc == 20 || tc == 23 || tc == 25 || tc == 27 || tc == 40 || tc == 41) type |= HorseType.Father;
      if (tc == 4 || tc == 8 || tc == 10 || tc == 12 || tc == 18 || tc == 19 || tc == 24 || tc == 25) type |= HorseType.Market;
      if (tc == 5 || tc == 9 || tc == 10 || tc == 11 || tc == 12 || tc == 40) type |= HorseType.LocalA;
      if (tc == 21 || tc == 22 || tc == 23 || tc == 24 || tc == 25 || tc == 41) type |= HorseType.LocalB;
      if (tc == 6 || tc == 11 || tc == 16 || tc == 20 || tc == 22 || tc == 40 || tc == 41) type |= HorseType.ForeignA;
      if (tc == 26 || tc == 27) type |= HorseType.ForeignB;
      if (tc == 15 || tc == 16 || tc == 17 || tc == 18 || tc == 19) type |= HorseType.Invited;
      if (tc == 31) type |= HorseType.BringIn;
      return type;
    }

    public static Horse FromJV(JVData_Struct.JV_UM_UMA uma)
    {
      int.TryParse(uma.UmaKigoCD, out int tc);

      short.TryParse(uma.SexCD, out var sex);
      short.TryParse(uma.KeiroCD, out var color);
      short.TryParse(uma.TozaiCD, out var belongs);

      return new()
      {
        LastModified = uma.head.MakeDate.ToDateTime(),
        DataStatus = uma.head.DataKubun.ToDataStatus(),
        Code = uma.KettoNum.Trim(),
        Name = uma.Bamei.Trim(),
        Entried = uma.RegDate.ToDateTime(),
        Retired = uma.DelDate.ToDateTime(),
        Born = uma.BirthDate.ToDateTime(),
        Sex = (HorseSex)sex,
        Type = ToHorseType(tc),
        Color = (HorseBodyColor)color,
        FatherBreedingCode = uma.Ketto3Info[0].HansyokuNum.Trim(),
        MotherBreedingCode = uma.Ketto3Info[1].HansyokuNum.Trim(),
        FFBreedingCode = uma.Ketto3Info[2].HansyokuNum.Trim(),
        FMBreedingCode = uma.Ketto3Info[3].HansyokuNum.Trim(),
        MFBreedingCode = uma.Ketto3Info[4].HansyokuNum.Trim(),
        MMBreedingCode = uma.Ketto3Info[5].HansyokuNum.Trim(),
        FFFBreedingCode = uma.Ketto3Info[6].HansyokuNum.Trim(),
        FFMBreedingCode = uma.Ketto3Info[7].HansyokuNum.Trim(),
        FMFBreedingCode = uma.Ketto3Info[8].HansyokuNum.Trim(),
        FMMBreedingCode = uma.Ketto3Info[9].HansyokuNum.Trim(),
        MFFBreedingCode = uma.Ketto3Info[10].HansyokuNum.Trim(),
        MFMBreedingCode = uma.Ketto3Info[11].HansyokuNum.Trim(),
        MMFBreedingCode = uma.Ketto3Info[12].HansyokuNum.Trim(),
        MMMBreedingCode = uma.Ketto3Info[13].HansyokuNum.Trim(),
        Belongs = (HorseBelongs)belongs,
        TrainerCode = uma.ChokyosiCode.Trim(),
        TrainerName = uma.ChokyosiRyakusyo.Trim(),
        InviteFrom = uma.Syotai.Trim(),
        ProducingCode = uma.BreederCode.Trim(),
        OwnerCode = uma.BanusiCode.Trim(),
      };
    }
  }

  public class HorseParent
  {
    /// <summary>
    /// 繁殖登録番号
    /// </summary>
    public int Number { get; set; }

    public HorseParent? Father { get; set; }

    public HorseParent? Mother { get; set; }
  }

  public enum HorseStatus : short
  {
    Unknown = 0,
    Active = 1,
    Retired = 2,
  }

  [Flags]
  public enum HorseType : short
  {
    Unknown = 0,

    /// <summary>
    /// （抽）
    /// </summary>
    LotteryA = 0b1,

    /// <summary>
    /// 「抽」
    /// </summary>
    LotteryB = 0b10,

    /// <summary>
    /// （父）
    /// </summary>
    Father = 0b100,

    /// <summary>
    /// （市）
    /// </summary>
    Market = 0b1000,

    /// <summary>
    /// （地）
    /// </summary>
    LocalA = 0b1_0000,

    /// <summary>
    /// 「地」
    /// </summary>
    LocalB = 0b10_0000,

    /// <summary>
    /// （外）
    /// </summary>
    ForeignA = 0b100_0000,

    /// <summary>
    /// 「外」
    /// </summary>
    ForeignB = 0b1000_0000,

    /// <summary>
    /// （招）
    /// </summary>
    Invited = 0b1_0000_0000,

    /// <summary>
    /// （持）
    /// </summary>
    BringIn = 0b10_0000_0000,
  }

  public enum HorseBodyColor : short
  {
    Unknown = 0,

    /// <summary>
    /// 栗
    /// </summary>
    Chestnut = 1,

    /// <summary>
    /// 栃栗
    /// </summary>
    DarkChestnut = 2,

    /// <summary>
    /// 鹿
    /// </summary>
    Bay = 3,

    /// <summary>
    /// 黒鹿
    /// </summary>
    DarkBay = 4,

    /// <summary>
    /// 青鹿
    /// </summary>
    Brown = 5,

    /// <summary>
    /// 青
    /// </summary>
    Black = 6,

    /// <summary>
    /// 芦
    /// </summary>
    Grey = 7,

    /// <summary>
    /// 栗かす
    /// </summary>
    DregChestnut = 8,

    /// <summary>
    /// 鹿かす
    /// </summary>
    DregBay = 9,

    /// <summary>
    /// 青かす
    /// </summary>
    DregBlack = 10,

    /// <summary>
    /// 白
    /// </summary>
    White = 11,
  }

  public enum HorseBelongs : short
  {
    Unknown = 0,

    Miho = 1,

    Ritto = 2,

    Local = 3,

    Foreign = 4,
  }
}
