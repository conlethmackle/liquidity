using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxDetailedAccountPush
   {
      [JsonPropertyName("availBal")]
      public decimal AvailBal { get; set; }

      [JsonPropertyName("availEq")] 
      public decimal AvailEq { get; set; }
      [JsonPropertyName("ccy")]
      public string Ccy { get; set; }
      [JsonPropertyName("cashBal")]
      public decimal CashBal { get; set; }
      [JsonPropertyName("uTime")]
      public ulong UTime { get; set; }
      [JsonPropertyName("disEq")]
      public decimal DisEq { get; set; }
      [JsonPropertyName("eq")]
      public decimal Eq { get; set; }
      [JsonPropertyName("eqUsd")]
      public decimal EqUsd { get; set; }
      [JsonPropertyName("frozenBal")]
      public decimal FrozenBal { get; set; }
      [JsonPropertyName("interest")]
      public decimal Interest { get; set; }
      [JsonPropertyName("isoEq")]
      public decimal IsoEq { get; set; }
      [JsonPropertyName("liab")]
      public decimal Liab { get; set; }
      [JsonPropertyName("maxLoan")]
      public decimal MaxLoan { get; set; }
      [JsonPropertyName("mgnRatio")]
      public decimal MgnRatio { get; set; }
      [JsonPropertyName("notionalLever")]
      public decimal NotionalLever { get; set; }
      [JsonPropertyName("ordFrozen")]
      public decimal OrdFrozen { get; set; }
      [JsonPropertyName("upl")]
      public decimal Upl { get; set; }
      [JsonPropertyName("uplLiab")]
      public decimal UplLiab { get; set; }
      [JsonPropertyName("crossLiab")]
      public decimal CrossLiab { get; set; }
      [JsonPropertyName("isoLiab")]
      public decimal IsoLiab { get; set; }
      [JsonPropertyName("coinUsdPrice")]
      public decimal CoinUsdPrice { get; set; }
      [JsonPropertyName("stgyEq")]
      public decimal StgyEq { get; set; }
      [JsonPropertyName("spotInUseAmt")]
      public decimal SpotInUseAmt { get; set; }
      [JsonPropertyName("isoUpl")]
      public decimal IsoUpl { get; set; }
   }
}
