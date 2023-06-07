using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   //{"data":{"accountId":"624d631329c69200018cd82e","total":"0.979","holdChange":"0.001","relationEvent":"trade.hold","available":"0.978","currency":"BTC","relationContext":{"symbol":"BTC-USDT","orderId":"6252ab0b29c6920001b45e0c"},"availableChange":"-0.001","time":"1649584907287","hold":"0.001","relationEventId":"6252ab0b29c6920001b45e10"}}
   public class AccountBalanceNotice
   {
      [JsonPropertyName("accountId")]
      public string AccountId { get; set; }
      [JsonPropertyName("total")] //: "88", // total balance
      public string Total { get; set; }
      [JsonPropertyName("available")] //: "88", // available balance
      public string Available { get; set; }
      [JsonPropertyName("availableChange")] //: "88", // the change of available balance
      public string AvailableChange { get; set; }
      [JsonPropertyName("currency")] //: "KCS", // currency
      public string Currency { get; set; }
      [JsonPropertyName("hold")] //: "0", // hold amount
      public string Hold { get; set; }
      [JsonPropertyName("holdChange")] //: "0", // the change of hold balance
      public string HoldChange { get; set; }
      [JsonPropertyName("relationEvent")] //: "trade.setted", //relation event
      public string RelationEvent { get; set; }
      [JsonPropertyName("relationEventId")] //: "5c21e80303aa677bd09d7dff", // relation event id
      public string RelationEventId { get; set; }
      [JsonPropertyName("relationContext")] //: {
      public TradeContext RelationContext { get; set; }    
      public string Account { get; set; }
      public string Instance { get; set; }
   }

   public class TradeContext
   {
      [JsonPropertyName("symbol")]
      public string Symbol { get; set; }
      [JsonPropertyName("tradeId")]
      public string TradeId { get; set; }
      [JsonPropertyName("orderId")]
      public string OrderId { get; set; }
   }

}
