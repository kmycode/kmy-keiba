using KmyKeiba.Data.Entities;
using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(Code))]
  public class HorseSaleData : DataBase<HorseSale>
  {
    /// <summary>
    /// 血統登録番号
    /// </summary>
    public string Code { get; set; } = string.Empty;

    [StringLength(12)]
    public string FatherBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string MotherBreedingCode { get; set; } = string.Empty;

    public int BornYear { get; set; }

    [StringLength(8)]
    public string MarketCode { get; set; } = string.Empty;

    public string MarketOwnerName { get; set; } = string.Empty;

    public string MarketName { get; set; } = string.Empty;

    public DateTime MarketStartDate { get; set; }

    public DateTime MarketEndDate { get; set; }

    public short Age { get; set; }

    public long Price { get; set; }

    public override void SetEntity(HorseSale sale)
    {
      this.LastModified = sale.LastModified;
      this.DataStatus = sale.DataStatus;
      this.Code = sale.Code;
      this.FatherBreedingCode = sale.FatherBreedingCode;
      this.MotherBreedingCode = sale.MotherBreedingCode;
      this.BornYear = sale.BornYear;
      this.MarketCode = sale.MarketCode;
      this.MarketOwnerName = sale.MarketOwnerName;
      this.MarketName = sale.MarketName;
      this.MarketStartDate = sale.MarketStartDate;
      this.MarketEndDate = sale.MarketEndDate;
      this.Age = sale.Age;
      this.Price = sale.Price;
    }

    public override bool IsEquals(DataBase<HorseSale> b)
    {
      var sale = (HorseSaleData)b;
      return sale.Code == this.Code && sale.MarketCode == this.MarketCode && sale.MarketStartDate.ToString("yyyyMMdd") == this.MarketStartDate.ToString("yyyyMMdd");
    }

    public override int GetHashCode()
    {
      return (this.Code + this.MarketCode + this.MarketStartDate.ToString("yyyyMMdd")).GetHashCode();
    }
  }
}
