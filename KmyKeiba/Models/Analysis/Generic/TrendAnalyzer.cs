using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Generic
{
  public class TrendAnalyzer : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private readonly List<Action> _postAnalysis = new();

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public ReactiveProperty<bool> IsLoaded { get; } = new();

    public ReactiveProperty<bool> IsAnalyzed { get; } = new();

    public int SizeMax { get; }

    protected TrendAnalyzer(int sizeMax)
    {
      this.IsAnalyzed.Subscribe(a =>
      {
        foreach (var action in this._postAnalysis)
        {
          action();
        }
        this._postAnalysis.Clear();
      });

      this.SizeMax = sizeMax;
    }

    public void PostAnalysis(Action action)
    {
      if (this.IsAnalyzed.Value)
      {
        action();
      }
      else
      {
        this._postAnalysis.Add(action);
      }
    }

    public async Task WaitAnalysisAsync()
    {
      while (!this.IsAnalyzed.Value)
      {
        await Task.Delay(10);
      }
    }

    public virtual void Dispose()
    {
      this._disposables.Dispose();

      // メモリリーク防止
      this._postAnalysis.Clear();
    }
  }
}
