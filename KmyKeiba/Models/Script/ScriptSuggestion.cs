using KmyKeiba.Data.Db;
using Microsoft.ClearScript;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  [NoDefaultScriptAccess]
  public class ScriptSuggestion
  {
    private readonly string _raceKey;

    public ReactiveCollection<TicketData> Tickets { get; } = new();

    public ReactiveCollection<HorseMarkSuggestion> Marks { get; } = new();

    public ReactiveProperty<bool> HasTickets { get; } = new();

    public ReactiveProperty<bool> HasMarks { get; } = new();

    public ScriptSuggestion(string raceKey)
    {
      this._raceKey = raceKey;
    }

    [ScriptMember("mark")]
    public void AddMark(short number, short mark)
    {
      this.Marks.Add(new HorseMarkSuggestion
      {
        HorseNumber = number,
        Mark = (RaceHorseMark)mark,
      });
      this.HasMarks.Value = true;
    }

    [ScriptMember("single")]
    public void AddSingleNumberTicket(short type, short count, object[] nums1Arr)
    {
      if (type != (short)TicketType.Single && type != (short)TicketType.Place)
      {
        return;
      }

      var nums1 = nums1Arr.Select(n => (int)n).ToArray();

      foreach (var num in nums1)
      {
        this.Tickets.Add(new TicketData
        {
          RaceKey = this._raceKey,
          Type = (TicketType)type,
          FormType = TicketFormType.Single,
          Count = count,
          Numbers1 = new byte[] { (byte)num, },
        });
      }

      this.HasTickets.Value = true;
    }

    private void AddNumbersTicket(TicketType type, TicketFormType formType, short count, bool isMulti, object[] nums1Arr, object[]? nums2Arr, object[]? nums3Arr)
    {
      var nums1 = nums1Arr.Select(n => (byte)(int)n).ToArray();
      var nums2 = nums2Arr?.Select(n => (byte)(int)n).ToArray();
      var nums3 = nums3Arr?.Select(n => (byte)(int)n).ToArray();

      var ticket = new TicketData
      {
        RaceKey = this._raceKey,
        Type = type,
        FormType = formType,
        Count = count,
        IsMulti = isMulti,
        Numbers1 = nums1,
      };
      if (nums2 != null) ticket.Numbers2 = nums2;
      if (nums3 != null) ticket.Numbers3 = nums3;

      this.Tickets.Add(ticket);

      this.HasTickets.Value = true;
    }

    private void AddTwoNumbersTicket(TicketType type, short formType, short count, bool isMulti, object[] nums1Arr, object[] nums2Arr)
    {
      var ft = (TicketFormType)formType;
      if (ft == TicketFormType.Formation)
      {
        this.AddNumbersTicket(type, ft, count, isMulti, nums1Arr, nums2Arr, null);
      }
      else if (ft == TicketFormType.Box)
      {
        this.AddNumbersTicket(type, ft, count, isMulti, nums1Arr, nums1Arr, null);
      }
      else
      {
        throw new ArgumentException("馬券の買い方が不正です");
      }
    }

    private void AddThreeNumbersTicket(TicketType type, short formType, short count, bool isMulti, object[] nums1Arr, object[] nums2Arr, object[] nums3Arr)
    {
      var ft = (TicketFormType)formType;
      if (ft == TicketFormType.Formation)
      {
        this.AddNumbersTicket(type, ft, count, isMulti, nums1Arr, nums2Arr, nums3Arr);
      }
      else if (ft == TicketFormType.Box)
      {
        this.AddNumbersTicket(type, ft, count, isMulti, nums1Arr, nums1Arr, nums1Arr);
      }
      else if (ft == TicketFormType.Nagashi)
      {
        this.AddNumbersTicket(type, ft, count, isMulti, nums1Arr, nums2Arr, nums2Arr);
      }
      else
      {
        throw new ArgumentException("馬券の買い方が不正です");
      }
    }

    [ScriptMember("frame")]
    public void AddFrameNumberTicket(short formType, short count, object[] nums1Arr, object[] nums2Arr)
    {
      this.AddTwoNumbersTicket(TicketType.FrameNumber, formType, count, false, nums1Arr, nums2Arr);
    }

    [ScriptMember("quinellaPlace")]
    public void AddQuinellaPlaceTicket(short formType, short count, object[] nums1Arr, object[] nums2Arr)
    {
      this.AddTwoNumbersTicket(TicketType.QuinellaPlace, formType, count, false, nums1Arr, nums2Arr);
    }

    [ScriptMember("quinella")]
    public void AddQuinellaTicket(short formType, short count, object[] nums1Arr, object[] nums2Arr)
    {
      this.AddTwoNumbersTicket(TicketType.Quinella, formType, count, false, nums1Arr, nums2Arr);
    }

    [ScriptMember("exacta")]
    public void AddExactaTicket(short formType, short count, bool isMulti, object[] nums1Arr, object[] nums2Arr)
    {
      this.AddTwoNumbersTicket(TicketType.Exacta, formType, count, isMulti, nums1Arr, nums2Arr);
    }

    [ScriptMember("trio")]
    public void AddTrioTicket(short formType, short count, object[] nums1Arr, object[] nums2Arr, object[] nums3Arr)
    {
      this.AddThreeNumbersTicket(TicketType.Trio, formType, count, false, nums1Arr, nums2Arr, nums3Arr);
    }

    [ScriptMember("trifecta")]
    public void AddTrifectaTicket(short formType, short count, bool isMulti, object[] nums1Arr, object[] nums2Arr, object[] nums3Arr)
    {
      this.AddThreeNumbersTicket(TicketType.Trio, formType, count, isMulti, nums1Arr, nums2Arr, nums3Arr);
    }

    public struct HorseMarkSuggestion
    {
      public short HorseNumber { get; init; }

      public RaceHorseMark Mark { get; init; }
    }
  }
}
