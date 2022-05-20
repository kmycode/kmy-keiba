using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RaceKey))]
  public class TicketData : AppDataBase
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    public TicketType Type { get; set; }

    public TicketFormType FormType { get; set; }

    public byte[] Numbers1 { get; set; } = Array.Empty<byte>();

    public byte[] Numbers2 { get; set; } = Array.Empty<byte>();

    public byte[] Numbers3 { get; set; } = Array.Empty<byte>();
  }

  public enum TicketType : short
  {
    Unknown = 0,
    Single = 1,
    Place = 2,
    FrameNumber = 3,
    QuinellaPlace = 4,
    Quinella = 5,
    Exacta = 6,
    Trio = 7,
    Trifecta = 8,
  }

  public enum TicketFormType : short
  {
    Unknown = 0,
    Single = 1,
    Box = 2,
    Formation = 3,
    Nagashi = 4,
  }
}
