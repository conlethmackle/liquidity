using Common.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Common.Models.DTOs
{
   public class LiquidationStrategyConfigDTO
   {
        [JsonPropertyName("id")]
        public int Id { get; set; }

      [JsonPropertyName("strategySPSubscriptionConfigId")]
      public int StrategySPSubscriptionConfigId { get; set; }
      public StrategyExchangeConfigDTO StrategySPSubscriptionConfig { get; set; }
      [JsonPropertyName("coinPairId")]
      public int CoinPairId { get; set; }
      public CoinPair CoinPair { get; set; }
      
      [JsonPropertyName("subscriptionPrice")]
     
      public decimal SubscriptionPrice { get; set; }
      [JsonPropertyName("numberOfCoins")]
      public decimal NumberOfCoins { get; set; }
      [JsonPropertyName("orderSize")]
      public decimal OrderSize { get; set; }
      [JsonPropertyName("percentageSpreadFromFV")]
      public decimal PercentageSpreadFromFV { get; set; }
      [JsonPropertyName("percentageSpreadLowerThreshold")]
      public decimal PercentageSpreadLowerThreshold { get; set; }
      [JsonPropertyName("cancellationPolicyOnStart")]
      public int CancellationPolicyOnStart { get; set; }
        [JsonPropertyName("venue")]
      public string Venue { get; set; }
      [JsonPropertyName("shortTimeInterval")]
      public int ShortTimeInterval { get; set; }
      [JsonPropertyName("longTimeInterval")]
      public int LongTimeInterval { get; set; }
      [JsonPropertyName("batchSize")]
      public int BatchSize { get; set; }
      [JsonPropertyName("priceDecimals")]
      public int PriceDecimals { get; set; }
      [JsonPropertyName("amountDecimals")]
      public int AmountDecimals { get; set; }
      [JsonPropertyName("symbol")]
      public string Symbol { get; set; }
      [JsonPropertyName("dateCreated")]
      public DateTime DateCreated { get; set; } 
      [JsonPropertyName("configName")]
      public string ConfigName { get; set; }
   }
}
