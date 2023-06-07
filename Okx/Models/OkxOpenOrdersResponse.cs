using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxOpenOrdersResponse
   {
      [JsonPropertyName("accFillSz")]
      public string AccFillSz { get; set; }
      [JsonPropertyName("avgPx")]
      public string AvgPx { get; set; }
      [JsonPropertyName("cTime")]
      public ulong CTime { get; set; }
      [JsonPropertyName("category")]
      public string Category { get; set; }
      [JsonPropertyName("ccy")]
      public string Ccy { get; set; }
      [JsonPropertyName("clOrdId")]
      public string ClOrdId { get; set; }
      [JsonPropertyName("fee")]
      public string Fee { get; set; }
      [JsonPropertyName("feeCcy")]
      public string FeeCcy { get; set; }
      [JsonPropertyName("fillPx")]
      public decimal FillPx { get; set; }
      [JsonPropertyName("fillSz")]
      public decimal FillSz { get; set; }
      [JsonPropertyName("fillTime")]
      public ulong FillTime { get; set; }
      [JsonPropertyName("instId")]
      public string InstId { get; set; }
      [JsonPropertyName("instType")]
      public string InstType { get; set; }
      [JsonPropertyName("lever")]
      public string Lever { get; set; }
      [JsonPropertyName("ordId")]
      public string OrdId { get; set; }
      [JsonPropertyName("ordType")]
      public string OrdType { get; set; }
      [JsonPropertyName("pnl")]
      public string Pnl { get; set; }
      [JsonPropertyName("posSide")]
      public string PosSide { get; set; }
      [JsonPropertyName("px")]
      public decimal Px { get; set; }
      [JsonPropertyName("rebate")]
      public string Rebate { get; set; }
      [JsonPropertyName("rebateCcy")]
      public string RebateCcy { get; set; }
      [JsonPropertyName("side")]
      public string Side { get; set; }
      [JsonPropertyName("slOrdPx")]
      public string SlOrdPx { get; set; }
      [JsonPropertyName("slTriggerPx")]
      public string SlTriggerPx { get; set; }
      [JsonPropertyName("slTriggerPxType")]
      public string SlTriggerPxType { get; set; }
      [JsonPropertyName("state")]
      public string State { get; set; }
      [JsonPropertyName("sz")]
      public decimal Sz { get; set; }
      [JsonPropertyName("tag")]
      public string Tag { get; set; }
      [JsonPropertyName("tgtCcy")]
      public string TgtCcy { get; set; }
      [JsonPropertyName("tdMode")]
      public string TdMode { get; set; }
      [JsonPropertyName("source")]
      public string Source { get; set; }
      [JsonPropertyName("tpOrdPx")]
      public string TpOrdPx { get; set; }
      [JsonPropertyName("tpTriggerPx")]
      public string TpTriggerPx { get; set; }
      [JsonPropertyName("tpTriggerPxType")]
      public string TpTriggerPxType { get; set; }
      [JsonPropertyName("tradeId")]
      public string TradeId { get; set; }
      [JsonPropertyName("reduceOnly")]
      public string ReduceOnly { get; set; }
      [JsonPropertyName("quickMgnType")]
      public string QuickMgnType { get; set; }
      [JsonPropertyName("algoClOrdId")]
      public string AlgoClOrdId { get; set; }
      [JsonPropertyName("algoId")]
      public string AlgoId { get; set; }
      [JsonPropertyName("uTime")]
      public ulong UTime { get; set; }
   }
}
