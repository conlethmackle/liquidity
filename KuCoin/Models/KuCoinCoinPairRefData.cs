using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class RefDataContainer
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("data")]
      public KuCoinCoinPairRefData[] Data { get; set; }
   }

   public class KuCoinCoinPairRefData
   {       
    [JsonPropertyName("symbol")] //: "XLM-USDT",
    public string Symbol { get; set; }
    [JsonPropertyName("name")] //: "XLM-USDT",
    public string Name { get; set; }
    [JsonPropertyName("baseCurrency")] //: "XLM",
    public string BaseCurrency { get; set; }
    [JsonPropertyName("quoteCurrency")] //: "USDT",
    public string QuoteCurrency { get; set; }
    [JsonPropertyName("feeCurrency")] //: "USDT",
    public string FeeCurrency { get; set; }
    [JsonPropertyName("market")] //: "USDS",
    public string Market { get; set; }
    [JsonPropertyName("baseMinSize")] //: "0.1",
    public decimal BaseMinSize { get; set; }
    [JsonPropertyName("quoteMinSize")] //: "0.01",
    public decimal QuoteMinSize { get; set; }
    [JsonPropertyName("baseMaxSize")] //: "10000000000",
    public decimal BaseMaxSize { get; set; }
    [JsonPropertyName("quoteMaxSize")] //: "99999999",
    public decimal QuoteMaxSize { get; set; }
    [JsonPropertyName("baseIncrement")] //: "0.0001",
    public decimal BaseIncrement { get; set; }
    [JsonPropertyName("quoteIncrement")] //: "0.000001",
    public decimal QuoteIncrement { get; set; }
    [JsonPropertyName("priceIncrement")] //: "0.000001",
    public decimal PriceIncrement { get; set; }
    [JsonPropertyName("priceLimitRate")] //: "0.1",
    public decimal PriceLimitRate { get; set; }
    [JsonPropertyName("isMarginEnabled")] //: true,
    public bool IsMarginEnabled { get; set; }
    [JsonPropertyName("enableTrading")] //: true
    public bool EnableTrading { get; set; }  
   }
}
