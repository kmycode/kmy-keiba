using KmyKeiba.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  public class ScriptOddsTimelineItem
  {
    private readonly OddsTimelineItem _item;

    [JsonPropertyName("odds")]
    public short Odds => this._item.Odds;

    [JsonPropertyName("time")]
    public DateTime Time => this._item.Time;

    public ScriptOddsTimelineItem(OddsTimelineItem item)
    {
      this._item = item;
    }
  }
}
