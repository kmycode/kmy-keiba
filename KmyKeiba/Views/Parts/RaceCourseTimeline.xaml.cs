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
  /// RaceCourseTimeline.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceCourseTimeline : UserControl
  {
    public static readonly DependencyProperty CourseProperty
    = DependencyProperty.Register(
        nameof(Course),
        typeof(RaceCourseItem),
        typeof(RaceCourseTimeline),
        new PropertyMetadata(null));

    public RaceCourseItem? Course
    {
      get { return (RaceCourseItem)GetValue(CourseProperty); }
      set { SetValue(CourseProperty, value); }
    }

    public RaceCourseTimeline()
    {
      InitializeComponent();

      this.MinHeight = Definitions.RaceTimelineHeightPerMinutes * (21 - Definitions.RaceTimelineStartHour) * 60 + 30;
    }
  }
}
