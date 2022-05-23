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
  /// <summary>
  /// レース条件の色付きアイコン
  /// </summary>
  public class RaceSubjectIcon : Grid
  {
    public static readonly DependencyProperty SubjectProperty
      = DependencyProperty.Register(
          nameof(Subject),
          typeof(RaceSubject),
          typeof(RaceSubjectIcon),
          new PropertyMetadata(null, (sender, e) =>
          {
            if (sender is RaceSubjectIcon view)
            {
              view.Update();
            }
          }));

    public RaceSubject? Subject
    {
      get { return (RaceSubject)GetValue(SubjectProperty); }
      set { SetValue(SubjectProperty, value); }
    }

    public static readonly DependencyProperty FontSizeProperty
      = DependencyProperty.Register(
          nameof(FontSize),
          typeof(double),
          typeof(RaceSubjectIcon),
          new PropertyMetadata(12.0, (sender, e) =>
          {
            if (sender is RaceSubjectIcon view)
            {
              view.textBlock.FontSize = (double)e.NewValue;
            }
          }));

    public double FontSize
    {
      get { return (double)GetValue(FontSizeProperty); }
      set { SetValue(FontSizeProperty, value); }
    }

    private readonly Border border = new();
    private readonly TextBlock textBlock = new TextBlock
    {
      Foreground = Brushes.White,
      FontWeight = FontWeights.Bold,
      VerticalAlignment = VerticalAlignment.Center,
      HorizontalAlignment = HorizontalAlignment.Center,
    };
    private readonly Border subBorder = new Border
    {
      VerticalAlignment = VerticalAlignment.Bottom,
    };

    public RaceSubjectIcon()
    {
      this.Children.Add(this.border);
      this.Children.Add(this.subBorder);
      this.Children.Add(this.textBlock);
      this.Update();

      this.LayoutUpdated += (_, _) =>
      {
        if (this.Subject?.SecondaryClass != null)
        {
          this.subBorder.Background = this.GetBrush(this.Subject.SecondaryClass);
          var height = !double.IsNaN(this.Height) ? this.Height : 0;
          var actualHeight = !double.IsNaN(this.ActualHeight) ? this.ActualHeight : 0;
          var size = Math.Max(Math.Max(height, actualHeight) / 2, 3);
          this.subBorder.Height = size;
        }
        else
        {
          this.subBorder.Background = this.GetBrush(string.Empty);
        }
      };
    }

    private void Update()
    {
      if (this.Subject == null)
      {
        this.textBlock.Text = string.Empty;
        this.border.Background = this.subBorder.Background = this.GetBrush(string.Empty);    // 引数は何でもいい
        return;
      }

      this.textBlock.Text = this.Subject.ClassName;
      this.border.Background = this.GetBrush(this.Subject.DisplayClass);

      if (this.Subject.SecondaryClass != null)
      {
        this.subBorder.Background = this.GetBrush(this.Subject.SecondaryClass);
        var height = !double.IsNaN(this.Height) ? this.Height : 0;
        var actualHeight = !double.IsNaN(this.ActualHeight) ? this.ActualHeight : 0;
        var size = Math.Max(Math.Max(height, actualHeight) / 2, 3);
        this.subBorder.Height = size;
        this.subBorder.Visibility = Visibility.Visible;
      }
      else
      {
        this.subBorder.Visibility = Visibility.Collapsed;
      }
    }

    private Brush GetBrush(object cls)
    {
      return cls switch
      {
        RaceClass.ClassA => Brushes.DarkRed,
        RaceClass.ClassB => Brushes.DarkBlue,
        RaceClass.ClassC => Brushes.Green,
        RaceClass.ClassD => Brushes.Gray,
        RaceClass.Money => Brushes.DarkGoldenrod,
        RaceClass.Age => Brushes.CadetBlue,
        RaceGrade.Grade1 => Brushes.DarkRed,
        RaceGrade.Grade2 => Brushes.DarkBlue,
        RaceGrade.Grade3 => Brushes.Green,
        RaceGrade.LocalGrade1 => Brushes.DeepPink,
        RaceGrade.LocalGrade2 => Brushes.DeepSkyBlue,
        RaceGrade.LocalGrade3 => Brushes.Lime,
        RaceGrade.Steeplechase1 => Brushes.DarkRed,
        RaceGrade.Steeplechase2 => Brushes.DarkBlue,
        RaceGrade.Steeplechase3 => Brushes.Green,
        RaceGrade.NoNamedGrade => Brushes.Gray,
        RaceGrade.NonGradeSpecial => Brushes.DarkGoldenrod,
        RaceGrade.Listed => Brushes.Gray,
        _ => Brushes.LightGray,
      };
    }
  }
}
