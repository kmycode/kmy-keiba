using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public interface IFinderColumnDefinition
  {
    FinderColumnType Type { get; }

    string Header { get; }

    int Width { get; }

    ReactiveProperty<bool> IsVisible { get; }
  }

  public class FinderColumnDefinition<T> : IFinderColumnDefinition
  {
    public FinderColumnType Type { get; }

    public string Header { get; }

    public int Width { get; }

    public Func<T, object> Value { get; }

    public ReactiveProperty<bool> IsVisible { get; } = new();

    public FinderColumnDefinition(FinderColumnType type, int width, string header, Func<T, object> value)
    {
      this.Type = type;
      this.Header = header;
      this.Value = value;
      this.Width = width;
    }

    public FinderColumnDefinition<T> Clone()
    {
      var item = new FinderColumnDefinition<T>(this.Type, this.Width, this.Header, this.Value);
      item.IsVisible.Value = this.IsVisible.Value;
      return item;
    }
  }

  public enum FinderColumnType
  {
    Unknown,
    Text,
    NumericText,
    BoldText,
    BoldNumericText,
    RunningStyle,
    CornerPlaces,
    RaceSubject,
  }
}
