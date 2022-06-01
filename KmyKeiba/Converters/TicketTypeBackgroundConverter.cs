using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace KmyKeiba.Converters
{
  internal class TicketTypeBackgroundConverter : IValueConverter
  {
    private Brush? Single { get; } = ResourceUtil.TryGetResource<Brush>("SingleTicketBackground");
    private Brush? Place { get; } = ResourceUtil.TryGetResource<Brush>("PlaceTicketBackground");
    private Brush? Frame { get; } = ResourceUtil.TryGetResource<Brush>("FrameNumberTicketBackground");
    private Brush? QuinellaPlace { get; } = ResourceUtil.TryGetResource<Brush>("QuinellaPlaceTicketBackground");
    private Brush? Quinella { get; } = ResourceUtil.TryGetResource<Brush>("QuinellaTicketBackground");
    private Brush? Exacta { get; } = ResourceUtil.TryGetResource<Brush>("ExactaTicketBackground");
    private Brush? Trio { get; } = ResourceUtil.TryGetResource<Brush>("TrioTicketBackground");
    private Brush? Trifecta { get; } = ResourceUtil.TryGetResource<Brush>("TrifectaTicketBackground");

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is TicketType type)
      {
        return type switch
        {
          TicketType.Single => this.Single,
          TicketType.Place => this.Place,
          TicketType.FrameNumber => this.Frame,
          TicketType.QuinellaPlace => this.QuinellaPlace,
          TicketType.Quinella => this.Quinella,
          TicketType.Exacta => this.Exacta,
          TicketType.Trio => this.Trio,
          TicketType.Trifecta => this.Trifecta,
          _ => null,
        };
      }
      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
