using KmyKeiba.Common;
using KmyKeiba.Models.RList;
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
  internal class RaceListItemStatusBackgroundConverter : IValueConverter
  {
    private static readonly Brush? _default = ResourceUtil.TryGetResource<Brush>("RaceTimelineItemBackground");
    private static readonly Brush? _selected = ResourceUtil.TryGetResource<Brush>("RaceTimelineSelectedItemBackground");
    private static readonly Brush? _notStart = ResourceUtil.TryGetResource<Brush>("RaceTimelineNotStartItemBackground");
    private static readonly Brush? _finished = ResourceUtil.TryGetResource<Brush>("RaceTimelineFinishedItemBackground");
    private static readonly Brush? _canceled = ResourceUtil.TryGetResource<Brush>("RaceTimelineCanceledItemBackground");

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is RaceListItemStatus status)
      {
        return status switch
        {
          RaceListItemStatus.NotStart => _notStart,
          RaceListItemStatus.Finished => _finished,
          RaceListItemStatus.Selected => _selected,
          RaceListItemStatus.Canceled => _canceled,
          _ => _default,
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
