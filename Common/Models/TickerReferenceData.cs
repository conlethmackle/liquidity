using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Common.Models
{
   public class TickerReferenceData
   {
      [JsonPropertyName("symbol")] //: "XLM-USDT",
      public string Symbol { get; set; }           
      [JsonPropertyName("feeCurrency")] //: "USDT",
      public string FeeCurrency { get; set; }     
      [JsonPropertyName("minQuantity")] //: "0.1",
      public decimal MinQuantity { get; set; }      
      [JsonPropertyName("maxQuantity")] //: "10000000000",
      public decimal MaxQuantity { get; set; }     
      [JsonPropertyName("quantityIncrement")] //: "0.0001",
      public decimal QuantityIncrement { get; set; }     
      [JsonPropertyName("priceIncrement")] //: "0.000001",
      public decimal PriceIncrement { get; set; }     
      [JsonPropertyName("enableTrading")] //: true
      public bool EnableTrading { get; set; }
   }
}
