using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class BornHorse : EntityBase
  {
    internal BornHorse()
    {
    }

    /// <summary>
    /// 血統登録番号
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public DateTime Born { get; set; }

    public HorseSex Sex { get; set; }

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

    public static BornHorse FromJV(JVData_Struct.JV_SK_SANKU uma)
    {
      short.TryParse(uma.SexCD, out var sex);
      short.TryParse(uma.KeiroCD, out var color);

      return new()
      {
        LastModified = uma.head.MakeDate.ToDateTime(),
        DataStatus = uma.head.DataKubun.ToDataStatus(),
        Code = uma.KettoNum.Trim(),
        Born = uma.BirthDate.ToDateTime(),
        Sex = (HorseSex)sex,
        Color = (HorseBodyColor)color,
        FatherBreedingCode = uma.HansyokuNum[0].Trim(),
        MotherBreedingCode = uma.HansyokuNum[1].Trim(),
        FFBreedingCode = uma.HansyokuNum[2].Trim(),
        FMBreedingCode = uma.HansyokuNum[3].Trim(),
        MFBreedingCode = uma.HansyokuNum[4].Trim(),
        MMBreedingCode = uma.HansyokuNum[5].Trim(),
        FFFBreedingCode = uma.HansyokuNum[6].Trim(),
        FFMBreedingCode = uma.HansyokuNum[7].Trim(),
        FMFBreedingCode = uma.HansyokuNum[8].Trim(),
        FMMBreedingCode = uma.HansyokuNum[9].Trim(),
        MFFBreedingCode = uma.HansyokuNum[10].Trim(),
        MFMBreedingCode = uma.HansyokuNum[11].Trim(),
        MMFBreedingCode = uma.HansyokuNum[12].Trim(),
        MMMBreedingCode = uma.HansyokuNum[13].Trim(),
      };
    }
  }
}
