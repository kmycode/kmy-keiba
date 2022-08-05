using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class RaceSubjectInfo
  {
    public RaceSubject Subject { get; }

    public string DisplayName
    {
      get => this._displayName;
      set => this._displayName = value.Trim();
    }
    private string _displayName = string.Empty;

    public string DisplaySubjectName => this.Subject.DisplayName;

    public string ShorterName { get; }

    public RaceSubjectInfo(RaceData race)
    {
      this.Subject = RaceSubject.Parse(race.SubjectName, race.Name);

      this.Subject.IsLocal = race.Course >= RaceCourse.LocalMinValue;
      this.Subject.Grade = race.Grade;
      if (race.SubjectAge2 != RaceSubjectType.Unknown)
      {
        this.Subject.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 2,
          Type = race.SubjectAge2,
        });
      }
      if (race.SubjectAge3 != RaceSubjectType.Unknown)
      {
        this.Subject.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 3,
          Type = race.SubjectAge3,
        });
      }
      if (race.SubjectAge4 != RaceSubjectType.Unknown)
      {
        this.Subject.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 4,
          Type = race.SubjectAge4,
        });
      }
      if (race.SubjectAge5 != RaceSubjectType.Unknown)
      {
        this.Subject.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 5,
          Type = race.SubjectAge5,
        });
      }
      if (race.SubjectAgeYounger != RaceSubjectType.Unknown)
      {
        this.Subject.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 6,
          Type = race.SubjectAgeYounger,
        });
      }

      if (race.Course >= RaceCourse.LocalMinValue)
      {
        if (this.Subject.Grade == RaceGrade.Grade1)
        {
          this.Subject.Grade = RaceGrade.LocalGrade1;
        }
        if (this.Subject.Grade == RaceGrade.Grade2)
        {
          this.Subject.Grade = RaceGrade.LocalGrade2;
        }
        if (this.Subject.Grade == RaceGrade.Grade3)
        {
          this.Subject.Grade = RaceGrade.LocalGrade3;
        }
      }

      if (race.Course >= RaceCourse.Foreign)
      {
        if (this.Subject.Grade == RaceGrade.LocalGrade1)
        {
          this.Subject.Grade = RaceGrade.ForeignGrade1;
        }
        if (this.Subject.Grade == RaceGrade.LocalGrade2)
        {
          this.Subject.Grade = RaceGrade.ForeignGrade2;
        }
        if (this.Subject.Grade == RaceGrade.LocalGrade3)
        {
          this.Subject.Grade = RaceGrade.ForeignGrade3;
        }
      }

      if (!string.IsNullOrWhiteSpace(race.Name))
      {
        this.DisplayName = race.Name;
      }
      else if (!string.IsNullOrWhiteSpace(race.SubjectName))
      {
        this.DisplayName = race.SubjectName;
      }
      else
      {
        this.DisplayName = this.Subject.ToString();
      }

      // 連続するスペースを削除
      this.DisplayName = new Regex(@"(\s|　)[\s　]+").Replace(this.DisplayName, "　");

      this.ShorterName = this.DisplayName.Substring(0, Math.Min(6, this.DisplayName.Length));
    }
  }
}
