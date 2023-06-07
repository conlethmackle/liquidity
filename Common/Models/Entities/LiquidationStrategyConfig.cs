using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class LiquidationStrategyConfig
   {
      public int Id { get; set; }
      public int StrategySPSubscriptionConfigId { get; set; }
      public StrategySPSubscriptionConfig StrategySPSubscriptionConfig { get; set; }
      public int CoinPairId { get; set; }
      public CoinPair CoinPair { get; set; }
      public decimal SubscriptionPrice { get; set; }
      public decimal NumberOfCoins { get; set; }
      public decimal OrderSize { get; set; }
      public decimal PercentageSpreadFromFV { get; set; }
      public decimal PercentageSpreadLowerThreshold { get; set; }
      public int CancellationPolicyOnStart { get; set; }
      public string Venue { get; set; }
      public int ShortTimeInterval { get; set; }
      public int LongTimeInterval { get; set; }
      public int BatchSize { get; set; }
      public int PriceDecimals { get; set; }
      public int AmountDecimals { get; set; }
      public string Symbol { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
