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
  public class RaceSubjectIcon : Label
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

    public RaceSubjectIcon()
    {
      this.Foreground = Brushes.White;
      this.HorizontalContentAlignment = HorizontalAlignment.Center;
      this.VerticalContentAlignment = VerticalAlignment.Center;
      this.FontWeight = FontWeights.Bold;
      this.Update();
    }

    private void Update()
    {
      if (this.Subject == null)
      {
        this.Content = string.Empty;
        this.Background = this.GetBrush(string.Empty);    // 引数は何でもいい
        return;
      }

      this.Content = this.Subject.ClassName;
      this.Background = this.GetBrush(this.Subject.DisplayClass);
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
