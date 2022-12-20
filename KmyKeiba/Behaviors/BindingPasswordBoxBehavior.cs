using Microsoft.Xaml.Behaviors;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KmyKeiba.Behaviors
{
  public class BindingPasswordBoxBehavior : Behavior<PasswordBox>
  {
    public static readonly DependencyProperty PasswordProperty
    = DependencyProperty.Register(
        nameof(Password),
        typeof(string),
        typeof(BindingPasswordBoxBehavior),
        new PropertyMetadata(null, (sender, e) =>
        {
          if (sender is BindingPasswordBoxBehavior view)
          {
            if (e.NewValue is string @new)
            {
              view.OnSourceUpdated();
            }
          }
        }));

    public string? Password
    {
      get { return (string)GetValue(PasswordProperty); }
      set { SetValue(PasswordProperty, value); }
    }

    protected override void OnAttached()
    {
      base.OnAttached();
      this.AssociatedObject.PasswordChanged += this.AssociatedObject_PasswordChanged;
    }

    private void AssociatedObject_PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.Password = this.AssociatedObject.Password;
    }

    protected override void OnDetaching()
    {
      this.AssociatedObject.PasswordChanged -= this.AssociatedObject_PasswordChanged;
      base.OnDetaching();
    }

    private void OnSourceUpdated()
    {
      if (this.AssociatedObject.Password != this.Password)
      {
        this.AssociatedObject.Password = this.Password;
      }
    }
  }
}
