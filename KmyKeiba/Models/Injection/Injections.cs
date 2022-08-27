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
    Task<double> GetTimeDeviationValueAsync(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime);

    Task<double> GetA3HTimeDeviationValueAsync(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime);

    Task<double> GetUntilA3HTimeDeviationValueAsync(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime);

    double GetPciDeviationValue(RaceData race, double pci, RaceStandardTimeMasterData standardTime);
  }

  public interface IInternalDataGenerator
  {
    public Task GenerateBaseStandardTimeDataAsync();
  }


  public interface IBuyer
  {
    string GetPurchaseLabel(RaceData race);

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

    Task SendAsync(Action<bool>? isSucceedCallback);
  }
}
