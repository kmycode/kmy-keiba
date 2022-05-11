using KmyKeiba.Common;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Common
{
  public class ReactivePropertyFactory<T>
  {
    private static readonly List<List<ReactiveProperty<T>>> _stocks = new();

    public ReactivePropertyFactory()
    {
      this.AddStock(1000);
    }

    public async Task<ReactiveProperty<T>> GetNewAsync()
    {
      ReactiveProperty<T>? GetStock()
      {
        List<ReactiveProperty<T>>? targetList = _stocks.Where(l => l.Any()).FirstOrDefault();
        if (targetList != null)
        {
          var item = targetList[0];
          targetList.RemoveAt(0);
          return item;
        }
        return null;
      }

      var stock = GetStock();
      if (stock == null)
      {
        await this.WaitAddStockAsync(1000);
        stock = GetStock();
      }

      if (stock != null)
      {
        return stock;
      }
      throw new IndexOutOfRangeException();
    }

    private async Task WaitAddStockAsync(int size)
    {
      var isCompleted = false;
      this.AddStock(size, () => isCompleted = true);
      while (!isCompleted)
      {
        await Task.Delay(10);
      }
    }

    private void AddStock(int size, Action? callback = null)
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        var list = new List<ReactiveProperty<T>>();
        for (var i = 0; i < size; i++)
        {
          list.Add(new ReactiveProperty<T>());
        }
        _stocks.Add(list);

        callback?.Invoke();
      });
    }
  }
}
