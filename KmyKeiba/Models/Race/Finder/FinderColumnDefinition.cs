using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public interface IFinderColumnDefiniton
  {
    public FinderColumnType Type { get; }

    public string Header { get; }
  }

  public class FinderColumnDefinition<T> : IFinderColumnDefiniton
  {
    public FinderColumnType Type { get; }

    public string Header { get; }

    public Func<T, object> Value { get; }

    public FinderColumnDefinition(FinderColumnType type, string header, Func<T, object> value)
    {
      this.Type = type;
      this.Header = header;
      this.Value = value;
    }
  }

  public enum FinderColumnType
  {
    Unknown,
    Text,
    BoldText,
    RunningStyle,
    CornerPlaces,
    RaceSubject,
  }
}
