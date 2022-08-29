using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.Common
{
  internal class CommandBase<T> : ICommand where T : class
  {
    private readonly Action<T> action;
    private readonly Func<bool>? canExecute;

    public CommandBase(Action<T> act, Func<bool>? canExecute = null)
    {
      this.action = act;
      this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
      if (this.canExecute != null)
      {
        return this.canExecute();
      }
      return true;
    }

    public void OnCanExecuteChanged()
    {
      this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object? parameter)
    {
      if (parameter is T t)
      {
        this.action(t);
      }
    }
  }

  internal class CommandBase : ICommand
  {
    private readonly Action action;

    public CommandBase(Action act)
    {
      this.action = act;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
      return true;
    }

    public void Execute(object? parameter)
    {
      this.action();
    }
  }
}
