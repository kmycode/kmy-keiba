using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Entities
{
  public class HorseSale : EntityBase
  {
    internal HorseSale()
    {
    }

    /// <summary>
    /// 血統登録番号
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public string FatherBreedingCode { get; set; } = string.Empty;

    public string MotherBreedingCode { get; set; } = string.Empty;

    public int BornYear { get; set; }

    public string MarketCode { get; set; } = string.Empty;

    public string MarketOwnerName { get; set; } = string.Empty;

    public string MarketName { get; set; } = string.Empty;

    public DateTime MarketStartDate { get; set; }

    public DateTime MarketEndDate { get; set; }

    public short Age { get; set; }

    public long Price { get; set; }

    public static HorseSale FromJV(JVData_Struct.JV_HS_SALE sale)
    {
      int.TryParse(sale.BirthYear.Trim(), out var born);
      short.TryParse(sale.Barei.Trim(), out var age);
      long.TryParse(sale.Price.Trim(), out var price);

      return new()
      {
        LastModified = sale.head.MakeDate.ToDateTime(),
        DataStatus = sale.head.DataKubun.ToDataStatus(),
        Code = sale.KettoNum.Trim(),
        FatherBreedingCode = sale.HansyokuFNum.Trim(),
        MotherBreedingCode = sale.HansyokuMNum.Trim(),
        BornYear = born,
        MarketCode = sale.SaleCode.Trim(),
        MarketOwnerName = sale.SaleHostName.Trim(),
        MarketName = sale.SaleName.Trim(),
        MarketStartDate = sale.FromDate.ToDateTime(),
        MarketEndDate = sale.ToDate.ToDateTime(),
        Age = age,
        Price = price,
      };
    }
  }
}
