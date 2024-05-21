using KmyKeiba.Models.Race.Finder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KmyKeiba.Views.ListItems
{
  class FinderRowTemplateSelector : DataTemplateSelector
  {
    public DataTemplate? TextTemplate { get; set; }
    public DataTemplate? NumericTextTemplate { get; set; }
    public DataTemplate? BoldTextTemplate { get; set; }
    public DataTemplate? BoldNumericTextTemplate { get; set; }
    public DataTemplate? RaceSubjectTemplate { get; set; }
    public DataTemplate? CourseInfoTemplate { get; set; }
    public DataTemplate? RunningStyleTemplate { get; set; }
    public DataTemplate? CornerPlaceTemplate { get; set; }
    public DataTemplate? HorseNameTemplate { get; set; }
    public DataTemplate? HorseMarkTemplate { get; set; }
    public DataTemplate? HorseSexTemplate { get; set; }
    public DataTemplate? RaceCourseWeatherTemplate { get; set; }
    public DataTemplate? RaceCourseConditionTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is FinderCell cell)
      {
        var template = cell.Column.Type switch
        {
          FinderColumnType.Text => this.TextTemplate,
          FinderColumnType.NumericText => this.NumericTextTemplate,
          FinderColumnType.NumericTextWithoutZero => this.NumericTextTemplate,
          FinderColumnType.BoldText => this.BoldTextTemplate,
          FinderColumnType.RaceSubject => this.RaceSubjectTemplate,
          FinderColumnType.CourseInfo => this.CourseInfoTemplate,
          FinderColumnType.RunningStyle => this.RunningStyleTemplate,
          FinderColumnType.CornerPlaces => this.CornerPlaceTemplate,
          FinderColumnType.HorseName => this.HorseNameTemplate,
          FinderColumnType.HorseMark => this.HorseMarkTemplate,
          FinderColumnType.HorseSex => this.HorseSexTemplate,
          FinderColumnType.RaceCourseWeather => this.RaceCourseWeatherTemplate,
          FinderColumnType.RaceCourseCondition => this.RaceCourseConditionTemplate,
          _ => null,
        };
        if (template != null)
        {
          return template;
        }
      }

      return base.SelectTemplate(item, container);
    }
  }
}
