using System;
using System.Text.Json.Serialization;

namespace Common.Models
{
   public class OrderQueryResponse
   {
      [JsonPropertyName("id")] //"id": "5c35c02703aa673ceec2a168",   //orderid
      public string Id { get; set; }
      [JsonPropertyName("customerorderid")]
      public string CustomerOrderId { get; set; }
      [JsonPropertyName("symbol")] //: "BTC-USDT",   //symbol
      public string Symbol { get; set; }    
      [JsonPropertyName("isbuy")]
      public bool IsBuy { get; set; }
      [JsonPropertyName("type")] //: "limit",       // order type,e.g. limit,market,stop_limit.
      public OrderTypeEnum Type { get; set; }
      [JsonPropertyName("isOpen")]
      public bool IsOpen { get; set; }
      [JsonPropertyName("price")] //: "10",         // order price
      public decimal Price { get; set; }
      [JsonPropertyName("quantity")] //: "2",           // order quantity
      public decimal Quantity { get; set; }
      [JsonPropertyName("filledQuantity")]
      public decimal FilledQuantity { get; set; }
      [JsonPropertyName("remainingQuantity")]
      public decimal RemainingQuantity { get; set; }
      [JsonPropertyName("timeInForce")] //: "GTC",  // time InForce,include GTC,GTT,IOC,FOK
      public TimeInForceEnum TimeInForce { get; set; }      
      [JsonPropertyName("hidden")] //: false,       // hidden order
      public bool Hidden { get; set; }
      [JsonPropertyName("iceberg")] //: false,      // iceberg order
      public bool Iceberg { get; set; }
      [JsonPropertyName("visibleSize")] //: "0",    // display quantity for iceberg order
      public decimal? VisibleSize { get; set; }      
      [JsonPropertyName("createdAt")] //: 1547026471000,  // create time
      public DateTime DateCreated { get; set; }
      public string Account { get; set; }
      public string Instance { get; set; }
    
   }
}
