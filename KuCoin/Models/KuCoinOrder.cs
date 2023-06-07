using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class KuCoinOrder
   {
      [JsonPropertyName("clientOid")]
      public string ClientOid { get; set; }
      [JsonPropertyName("side")]
      public string Side { get; set; }
      [JsonPropertyName("Symbol")]
      public string Symbol { get; set; }
      [JsonPropertyName("type")]
      public string Type { get; set; }
      [JsonPropertyName("remark")]
      public string Remark { get; set; }
      [JsonPropertyName("stp")]
      public string Stp { get; set; }
      [JsonPropertyName("tradeType")]
      public string TradeType { get; set; }
      [JsonPropertyName("Price")]
      public decimal Price { get; set; }
      [JsonPropertyName("Size")]
      public decimal Size { get; set; }
      [JsonPropertyName("TimeInForce")]
      public string TimeInForce { get; set; } //String[Optional] GTC, GTT, IOC, or FOK(default is GTC), read Time In Force.
      [JsonPropertyName("cancelAfter")]
      public Int64  CancelAfter { get; set; } //long[Optional] cancel after n seconds, requires timeInForce to be GTT
      [JsonPropertyName("postOnly")]
      public bool PostOnly { get; set; } //boolean[Optional] Post only flag, invalid when timeInForce is IOC or FOK
      [JsonPropertyName("hidden")]
      public bool Hidden { get; set; } // boolean[Optional] Order will not be displayed in the order book
      [JsonPropertyName("iceberg")]
      public bool Iceberg { get; set; }// boolean[Optional] Only aportion of the order is displayed in the order book
      [JsonPropertyName("visibleSize")]
      public decimal VisibleSize { get; set; } // String[Optional] The maximum visible size of an iceberg order
   }
}
