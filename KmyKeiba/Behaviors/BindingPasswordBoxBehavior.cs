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
      this.AssociatedObject.TextInput += this.AssociatedObject_TextInput;
    }

    protected override void OnDetaching()
    {
      this.AssociatedObject.TextInput -= this.AssociatedObject_TextInput;
      base.OnDetaching();
    }

    private void AssociatedObject_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
      this.Password = this.AssociatedObject.Password;
    }

    private void OnSourceUpdated()
    {
      this.AssociatedObject.Password = this.Password;
    }
  }
}
