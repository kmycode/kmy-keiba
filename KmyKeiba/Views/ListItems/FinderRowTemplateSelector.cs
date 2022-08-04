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

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is FinderCell cell)
      {
        var template = cell.Column.Type switch
        {
          FinderColumnType.Text => this.TextTemplate,
          FinderColumnType.NumericText => this.NumericTextTemplate,
          FinderColumnType.BoldText => this.BoldTextTemplate,
          FinderColumnType.RaceSubject => this.RaceSubjectTemplate,
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
