using KmyKeiba.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderCell
  {
    public IFinderColumnDefiniton Column { get; }

    public object Value { get; }

    public ValueComparation Comparation { get; set; }

    public FinderCell(IFinderColumnDefiniton column, object value)
    {
      this.Column = column;
      this.Value = value;
    }
  }

  public static class FinderCellExtensions
  {
    public static FinderCell CreateCell<T>(this FinderColumnDefinition<T> column, T data)
    {
      return new FinderCell(column, column.Value(data));
    }
  }
}
