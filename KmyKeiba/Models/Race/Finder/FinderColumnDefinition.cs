using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
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
    int Tab { get; }

    FinderColumnType Type { get; }

    string Header { get; }

    int Width { get; }

    ReactiveProperty<bool> IsVisible { get; }

    CellTextAlignment Alignment { get; }
  }

  public class FinderColumnDefinition<T> : IFinderColumnDefinition
  {
    public int Tab { get; }

    public FinderColumnType Type { get; }

    public string Header { get; }

    public int Width { get; }

    public Func<T, object> Value { get; }

    public ReactiveProperty<bool> IsVisible { get; } = new();

    public CellTextAlignment Alignment { get; }

    public Func<T, object, ValueComparation>? Comparation { get; }

    public Func<object, object> ToStringFunc { get; }

    public FinderColumnDefinition(int tab, FinderColumnType type, int width, string header, Func<T, object> value, Func<object, object> toString, Func<T, object, ValueComparation>? comparation)
    {
      this.Tab = tab;
      this.Type = type;
      this.Header = header;
      this.Value = value;
      this.Width = width;
      this.Comparation = comparation;
      this.ToStringFunc = toString;

      this.Alignment = type == FinderColumnType.NumericText || type == FinderColumnType.BoldNumericText ||
        type == FinderColumnType.RunningStyle || type == FinderColumnType.CornerPlaces ? CellTextAlignment.Right : CellTextAlignment.Left;
    }

    public FinderColumnDefinition<T> Clone()
    {
      var item = new FinderColumnDefinition<T>(this.Tab, this.Type, this.Width, this.Header, this.Value, this.ToStringFunc, this.Comparation);
      item.IsVisible.Value = this.IsVisible.Value;
      return item;
    }
  }

  public class FinderColumnDefinitionBuilder<T>
  {
    public string Name { get; init; }

    public FinderColumnProperty Property { get; init; }

    public FinderColumnType Type { get; init; }

    public int Width { get; init; }

    public string Header { get; init; } = string.Empty;

    public Func<T, object> Value { get; init; } = _ => string.Empty;

    public Func<object, object>? ToStringFunc { get; init; }

    public Func<T, object, ValueComparation>? Comparation { get; init; }

    public FinderColumnDefinition<T> Build(int tab)
    {
      if (this.ToStringFunc == null)
      {
        return FinderColumnDefinition.Create(tab, this.Type, this.Width, this.Header, this.Value, this.Comparation);
      }
      return FinderColumnDefinition.Create(tab, this.Type, this.Width, this.Header, this.Value, this.ToStringFunc, this.Comparation);
    }

    public FinderColumnDefinitionBuilder(FinderColumnProperty property, FinderColumnType type, int width, string header, string name, Func<T, object> value, Func<object, object>? toStringFunc, Func<T, object, ValueComparation>? comparation = null)
    {
      this.Name = string.IsNullOrEmpty(name) ? header : name;
      this.Property = property;
      this.Type = type;
      this.Width = width;
      this.Header = header;
      this.Value = value;
      this.ToStringFunc = toStringFunc;
      this.Comparation = comparation;
    }

    public FinderColumnDefinitionBuilder(FinderColumnProperty property, FinderColumnType type, int width, string header, Func<T, object> value, Func<object, object>? toStringFunc, Func<T, object, ValueComparation>? comparation = null)
      : this(property, type, width, header, string.Empty, value, toStringFunc, comparation)
    {
    }

    public FinderColumnDefinitionBuilder(FinderColumnProperty property, FinderColumnType type, int width, string header, string name, Func<T, object> value, Func<T, object, ValueComparation>? comparation = null)
      : this(property, type, width, header, name, value, null, comparation)
    {
    }

    public FinderColumnDefinitionBuilder(FinderColumnProperty property, FinderColumnType type, int width, string header, Func<T, object> value, Func<T, object, ValueComparation>? comparation = null)
      : this(property, type, width, header, string.Empty, value, null, comparation)
    {
    }
  }

  public static class FinderColumnDefinition
  {
    public static FinderColumnDefinition<T> Create<T>(int tab, FinderColumnType type, int width, string header, Func<T, object> value, Func<T, object, ValueComparation>? comparation = null)
      => new FinderColumnDefinition<T>(tab,
                                       type,
                                       width,
                                       header,
                                       value,
                                       type == FinderColumnType.NumericTextWithoutZero ? NonZeroValueToString : FinderColumnToString,
                                       comparation);

    public static FinderColumnDefinition<T> Create<T>(int tab, FinderColumnType type, int width, string header, Func<T, object> value, Func<object, object> toString, Func<T, object, ValueComparation>? comparation = null)
      => new FinderColumnDefinition<T>(tab, type, width, header, value, toString, comparation);

    private static object FinderColumnToString(object value)
    {
      return value;
    }

    private static object NonZeroValueToString(object value)
    {
      if (value is short sv) return sv == 0 ? string.Empty : value;
      if (value is int iv) return iv == 0 ? string.Empty : value;
      if (value is float fv) return fv == 0 ? string.Empty : value;
      if (value is double dv) return dv == 0 ? string.Empty : value;
      return value;
    }

    public static FinderColumnDefinition<T> CreateDelay<T>(int tab, FinderColumnType type, int width, string header, Func<MyContext, T, Task<object>> value)
    {
      return new FinderColumnDefinition<T>(tab, type, width, header, obj =>
      {
        var property = new ReactiveProperty<object>();
        Task.Run(() =>
        {
          try
          {
            using var db = new MyContext();
            property.Value = value(db, obj).Result;
          }
          catch
          {
            // TODO
          }
        });
        return property;
      }, FinderColumnToString, null);
    }

    public static FinderColumnDefinition<T> CreateDelay<T>(int tab, FinderColumnType type, int width, string header, Func<T, Task<object>> value)
    {
      return new FinderColumnDefinition<T>(tab, type, width, header, obj =>
      {
        var property = new ReactiveProperty<object>();
        Task.Run(() =>
        {
          try
          {
            property.Value = value(obj).Result;
          }
          catch
          {
            // TODO
          }
        });
        return property;
      }, FinderColumnToString, null);
    }
  }

  public enum FinderColumnType
  {
    Unknown,
    Text,
    NumericText,
    NumericTextWithoutZero,
    BoldText,
    BoldNumericText,
    RunningStyle,
    CornerPlaces,
    RaceSubject,
    CourseInfo,
    HorseName,
    HorseMark,
    HorseSex,
    RaceCourseWeather,
    RaceCourseCondition,
  }

  public enum CellTextAlignment
  {
    Left,
    Center,
    Right,
  }
}
