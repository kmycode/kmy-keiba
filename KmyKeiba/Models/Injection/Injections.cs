using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Injection
{
  public interface ITimeDeviationValueCalculator
  {
    double GetTimeDeviationValue(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime);

    double GetA3HTimeDeviationValue(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime);
  }

  public interface IBuyer
  {
    bool CanBuy(RaceData race);

    IPurchaseBuilder CreateNewPurchase(RaceData race);
  }

  public interface IPurchaseBuilder
  {
    IPurchaseBuilder AddTicket(TicketData ticket);

    IPurchaseBuilder AddTicketRange(IEnumerable<TicketData> tickets)
    {
      foreach (var ticket in tickets)
      {
        this.AddTicket(ticket);
      }
      return this;
    }

    void Send(Action<bool>? isSucceedCallback);
    void Test();
  }
}
