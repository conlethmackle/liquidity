using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.Models;

namespace BlazorLiquidity.Shared
{
  

   public class OrderDetails
   {
      [JsonPropertyName("clientOrderId")]
      public string ClientOrderId { get; set; }
      [JsonPropertyName("symbol")]
      [Required]
      public string Symbol { get; set; } = "BTC/USDT";
      [Required]
      public OrderTypeEnum OrderType { get; set; }
      [Required]
      public bool Side { get; set; } = false;
      [JsonPropertyName("Price")]
     
      public decimal Price { get; set; }
      [JsonPropertyName("quantity")]
      [Required]
      public decimal Quantity { get; set; }
      [JsonPropertyName("timeInForce")]
      public TimeInForceEnum TimeInForce { get; set; }
      [JsonPropertyName("iceberg")]
      public bool Iceberg { get; set; }
      [JsonPropertyName("visibleSize")]
      public decimal VisibleSize { get; set; }
      [JsonPropertyName("venue")]
      [Required]
      public string Venue { get; set; } //= "Binance_Exchange";
      public string PortfolioName { get; set; } //= "VetLiquidationTest";
      public string InstanceName { get; set; } //= "VetFairValueConfig";
   }
}
