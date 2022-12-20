using KmyKeiba.Common;
using KmyKeiba.Models.RList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KmyKeiba.Views.Parts
{
  /// <summary>
  /// RaceCourseTimelineItem.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceCourseTimelineItem : UserControl
  {
    public static readonly DependencyProperty RaceProperty
    = DependencyProperty.Register(
        nameof(Race),
        typeof(RaceListItem),
        typeof(RaceCourseTimelineItem),
        new PropertyMetadata(null, (sender, e) =>
        {
          if (sender is RaceCourseTimelineItem view)
          {
            view.Update();
          }
        }));

    public RaceListItem? Race
    {
      get { return (RaceListItem)GetValue(RaceProperty); }
      set { SetValue(RaceProperty, value); }
    }

    public RaceCourseTimelineItem()
    {
      InitializeComponent();
      this.Update();

      this.MouseLeftButtonUp += (_, _) => this.Race?.OnSelected();
      this.MouseRightButtonUp += (_, _) =>
      {
        if (this.Race != null)
        {
          OpenRaceRequest.Default.Request(this.Race.Key);
        }
      };
    }

    private void Update()
    {
      if (this.Race != null)
      {
        //Canvas.SetTop(this, this.Race.StartTime.TimeOfDay.TotalMinutes - 480);

        if (this.Race.PrevRaceStartTime.Value != DateTime.MinValue)
        {
          var timeLength = this.Race.StartTime - this.Race.PrevRaceStartTime.Value;
          this.TopChild.Height = Math.Max(0, timeLength.TotalMinutes) * Definitions.RaceTimelineHeightPerMinutes;
        }
        else
        {
          this.TopChild.Height = Definitions.RaceTimelineHeightPerMinutes * 40;
        }
      }
    }
  }
}
