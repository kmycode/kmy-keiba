using KmyKeiba.Models.Logics.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KmyKeiba.Resources.TemplateSelectors
{
  class MainTabItemTemplateSelector : DataTemplateSelector
  {
    public DataTemplate RaceListTemplate { get; set; } = new();

    public DataTemplate RaceDetailTemplate { get; set; } = new();

    public DataTemplate RiderDetailTemplate { get; set; } = new();

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      switch (item)
      {
        case RaceTabFrame:
          return this.RaceDetailTemplate;
        case RaceListTabFrame:
          return this.RaceListTemplate;
        case RiderTabFrame:
          return this.RiderDetailTemplate;
      }

      return base.SelectTemplate(item, container);
    }
  }
}
