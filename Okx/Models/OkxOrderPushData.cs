using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxOrderPushData
   {
      [JsonPropertyName("accFillSz")]
      public string AccFillSz { get; set; }
      [JsonPropertyName("amendResult")]
      public string AmendResult { get; set; }
      [JsonPropertyName("avgPx")]
      public decimal AvgPx { get; set; }
      [JsonPropertyName("cTime")]
      public ulong CTime { get; set; }
      [JsonPropertyName("category")]
      public string Category { get; set; }
      [JsonPropertyName("ccy")]
      public string Ccy { get; set; }
      [JsonPropertyName("clOrdId")]
      public string ClOrdId { get; set; }
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("execType")]
      public string ExecType { get; set; }
      [JsonPropertyName("fee")]
      public string Fee { get; set; }
      [JsonPropertyName("feeCcy")]
      public string FeeCcy { get; set; }
      [JsonPropertyName("fillFee")]
      public decimal FillFee { get; set; }
      [JsonPropertyName("fillFeeCcy")]
      public string FillFeeCcy { get; set; }
      [JsonPropertyName("fillNotionalUsd")]
      public decimal FillNotionalUsd { get; set; }
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
      [JsonPropertyName("msg")]
      public string Msg { get; set; }
      [JsonPropertyName("notionalUsd")]
      public string NotionalUsd { get; set; }
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
      [JsonPropertyName("reduceOnly")]
      public string ReduceOnly { get; set; }
      [JsonPropertyName("reqId")]
      public string ReqId { get; set; }
      [JsonPropertyName("side")]
      public string Side { get; set; }
      [JsonPropertyName("slOrdPx")]
      public decimal SlOrdPx { get; set; }
      [JsonPropertyName("slTriggerPx")]
      public decimal SlTriggerPx { get; set; }
      [JsonPropertyName("slTriggerPxType")]
      public string SlTriggerPxType { get; set; }
      [JsonPropertyName("source")]
      public string Source { get; set; }
      [JsonPropertyName("state")]
      public string State { get; set; }
      [JsonPropertyName("sz")]
      public decimal Sz { get; set; }
      [JsonPropertyName("tag")]
      public string Tag { get; set; }
      [JsonPropertyName("tdMode")]
      public string TdMode { get; set; }
      [JsonPropertyName("tgtCcy")]
      public string TgtCcy { get; set; }
      [JsonPropertyName("tpOrdPx")]
      public decimal TpOrdPx { get; set; }
      [JsonPropertyName("tpTriggerPx")]
      public string TpTriggerPx { get; set; }
      [JsonPropertyName("tpTriggerPxType")]
      public string TpTriggerPxType { get; set; }
      [JsonPropertyName("tradeId")]
      public string TradeId { get; set; }
      [JsonPropertyName("quickMgnType")]
      public string QuickMgnType { get; set; }
      [JsonPropertyName("algoClOrdId")]
      public string AlgoClOrdId { get; set; }
      [JsonPropertyName("algoId")]
      public string AlgoId { get; set; }
      [JsonPropertyName("amendSource")]
      public string AmendSource { get; set; }
      [JsonPropertyName("cancelSource")]
      public string CancelSource { get; set; }
      [JsonPropertyName("uTime")]
      public ulong UTime { get; set; }
   }
}
