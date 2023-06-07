using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Common.Models;

namespace Common.Messages
{
   
   public class PlaceOrderCmd
   {
      [JsonPropertyName("clientOrderId")]
      public string ClientOrderId { get; set; }
      [JsonPropertyName("symbol")]
      public string Symbol { get; set; }      
      public OrderTypeEnum OrderType { get; set; }
      public bool IsBuy { get; set; }
      [JsonPropertyName("Price")]
      public decimal Price { get; set; }
      [JsonPropertyName("quantity")]
      public decimal Quantity { get; set; }
      [JsonPropertyName("timeInForce")]
      public TimeInForceEnum TimeInForce { get; set; }
      [JsonPropertyName("iceberg")]
      public bool Iceberg { get; set; }
      [JsonPropertyName("visibleSize")]
      public decimal VisibleSize { get; set; }
      [JsonPropertyName("venue")]
      public string Venue { get; set; }
   }
}
