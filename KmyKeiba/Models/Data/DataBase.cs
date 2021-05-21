using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  public abstract class DataBase<T>
  {
    public uint Id { get; set; }

    public DateTime LastModified { get; set; }

    public abstract void SetEntity(T race);

    public override bool Equals(object? obj)
    {
      if (obj is DataBase<T> b)
      {
        return this.IsEquals(b);
      }
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return this.Id.GetHashCode();
    }

    public virtual bool IsEquals(DataBase<T> b)
    {
      return this.Id == b.Id;
    }
  }
}
