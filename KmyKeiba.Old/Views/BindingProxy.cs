using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KmyKeiba.Views
{
  class BindingProxy : Freezable
  {
    public object Data { get => GetValue(DataProperty); set => SetValue(DataProperty, value); }
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy),
            new PropertyMetadata(null));

    protected override Freezable CreateInstanceCore() => new BindingProxy();
  }
}
