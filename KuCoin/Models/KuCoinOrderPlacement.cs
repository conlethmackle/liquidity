using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Messages;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class KuCoinOrderPlacement
   {
      [JsonPropertyName("clientOid")]
      public string ClientOid { get; set; }
      [JsonPropertyName("side")]
      public string Side { get; set; } // buy or sell
      [JsonPropertyName("symbol")]
      public string Symbol { get; set; }
      [JsonPropertyName("type")]
      public string Type { get; set; } // limit or market -> default is limit
 //     [JsonPropertyName("remark")]
 //     public string Remark { get; set; } // optional 
  //    [JsonPropertyName("stp")]
 //     public string Stp { get; set; }
      [JsonPropertyName("tradeType")]
      public string TradeType { get; set; }
      [JsonPropertyName("price")]
      public decimal Price { get; set; }
      [JsonPropertyName("size")]
      public decimal Quantity { get; set; }
      [JsonPropertyName("timeInForce")]
      public string TimeInForce { get; set; }
   //   [JsonPropertyName("cancelAfter")]
  //    public long CancelAfter { get; set; }
  //    [JsonPropertyName("postOnly")]
 //     public bool PostOnly { get; set; }
  //    [JsonPropertyName("hidden")]
      public bool Hidden { get; set; }
      [JsonPropertyName("iceberg")]
      public bool Iceberg { get; set; }
      [JsonPropertyName("visibleSize")]
      public decimal VisibleSize { get; set; }

      
   }

   public class KuCoinOrderMarketOrderPlacement
   {
      [JsonPropertyName("clientOid")]
      public string ClientOid { get; set; }
      [JsonPropertyName("side")]
      public string Side { get; set; } // buy or sell
      [JsonPropertyName("symbol")]
      public string Symbol { get; set; }
      [JsonPropertyName("type")]
      public string Type { get; set; } // limit or market -> default is limit
      //     [JsonPropertyName("remark")]
      //     public string Remark { get; set; } // optional 
      //    [JsonPropertyName("stp")]
      //     public string Stp { get; set; }
      [JsonPropertyName("tradeType")]
      public string TradeType { get; set; }
     
      [JsonPropertyName("size")]
      public decimal Quantity { get; set; }
      [JsonPropertyName("timeInForce")]
      public string TimeInForce { get; set; }
      //   [JsonPropertyName("cancelAfter")]
      //    public long CancelAfter { get; set; }
      //    [JsonPropertyName("postOnly")]
      //     public bool PostOnly { get; set; }
      //    [JsonPropertyName("hidden")]
      public bool Hidden { get; set; }
      [JsonPropertyName("iceberg")]
      public bool Iceberg { get; set; }
      [JsonPropertyName("visibleSize")]
      public decimal VisibleSize { get; set; }


   }
}
