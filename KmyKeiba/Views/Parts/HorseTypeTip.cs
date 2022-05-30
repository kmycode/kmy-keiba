using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KmyKeiba.Views.Parts
{
  public class HorseTypeTip : StackPanel
  {
    private readonly Brush _borderBrush = Application.Current.TryFindResource("BaseBorderBrush") as Brush ?? Brushes.Gray;
    private readonly Style? _textStyle = Application.Current.TryFindResource("TextBlockDefault") as Style;

    public static readonly DependencyProperty HorseTypeProperty
      = DependencyProperty.Register(
         nameof(HorseType),
         typeof(HorseType),
         typeof(HorseTypeTip),
         new PropertyMetadata(HorseType.Unknown, (sender, e) =>
         {
           if (sender is HorseTypeTip view)
           {
             view.Update();
           }
         }));

    public HorseType HorseType
    {
      get { return (HorseType)GetValue(HorseTypeProperty); }
      set { SetValue(HorseTypeProperty, value); }
    }

    public static readonly DependencyProperty FontSizeProperty
      = DependencyProperty.Register(
          nameof(FontSize),
          typeof(double),
          typeof(HorseTypeTip),
          new PropertyMetadata(16.0, (sender, e) =>
          {
            if (sender is HorseTypeTip view)
            {
              view.UpdateFontSize();
            }
          }));

    public double FontSize
    {
      get { return (double)GetValue(FontSizeProperty); }
      set { SetValue(FontSizeProperty, value); }
    }

    public HorseTypeTip()
    {
      this.Orientation = Orientation.Horizontal;
      this.Update();
    }

    private void Update()
    {
      this.Children.Clear();

      void AddRect(string text)
      {
        this.Children.Add(new Border
        {
          BorderBrush = _borderBrush,
          BorderThickness = new Thickness(1),
          Width = this.FontSize + 8,
          Height = this.FontSize + 8,
          Child = new TextBlock
          {
            Text = text,
            Style = _textStyle,
            FontSize = this.FontSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
          },
        });
      }

      void AddRound(string text)
      {
        AddRect(text);
        ((Border)this.Children[this.Children.Count - 1]).CornerRadius = new CornerRadius(this.FontSize / 2 + 4);
      }

      if (this.HorseType.HasFlag(HorseType.BringIn))
      {
        AddRound("持");
      }
      if (this.HorseType.HasFlag(HorseType.Invited))
      {
        AddRound("招");
      }
      if (this.HorseType.HasFlag(HorseType.ForeignA))
      {
        AddRound("外");
      }
      if (this.HorseType.HasFlag(HorseType.ForeignB))
      {
        AddRect("外");
      }
      if (this.HorseType.HasFlag(HorseType.LocalA))
      {
        AddRound("地");
      }
      if (this.HorseType.HasFlag(HorseType.LocalB))
      {
        AddRect("地");
      }
      if (this.HorseType.HasFlag(HorseType.Market))
      {
        AddRound("市");
      }
      if (this.HorseType.HasFlag(HorseType.Father))
      {
        AddRound("父");
      }
      if (this.HorseType.HasFlag(HorseType.LotteryA))
      {
        AddRound("抽");
      }
      if (this.HorseType.HasFlag(HorseType.LotteryB))
      {
        AddRect("抽");
      }
    }

    private void UpdateFontSize()
    {
      foreach (var child in this.Children.OfType<Border>().Select(c => c.Child).OfType<TextBlock>())
      {
        child.FontSize = this.FontSize;
      }
    }
  }
}
