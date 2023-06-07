using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class LiquidationConfiguration
   {
      public int Id { get; set; }
      public int SPId { get; set; }
      public SP SP { get; set; }
      public int StrategyId { get; set; }
      public Strategy Strategy { get; set; }
      public int StrategySPSubscriptionConfigId { get; set; }
      public StrategySPSubscriptionConfig StrategySPSubscriptionConfig { get; set; }
      public int CoinPairId { get; set; }
      public CoinPair CoinPair { get; set; }
      public decimal SubscriptionPrice { get; set; }
      public decimal CoinAmount { get; set; }
      public decimal OrderSize { get; set; }
      public decimal DailyLiquidationTarget { get; set; }
      public decimal MaxOrderSize { get; set; }
      public decimal PercentageSpreadFromFV { get; set; }
      public decimal PercentageSpreadLowerThreshold { get; set; }
      public int ShortTimeInterval { get; set; }
      public int LongTimeInterval { get; set; }
      public int CancelTimerInterval { get; set; }
      public int TakerModeTimeInterval { get; set; }
      public int BatchSize { get; set; }
      public int PriceDecimals { get; set; }
      public int AmountDecimals { get; set; }
      public int LiquidationOrderLoadingConfigurationId { get; set; }
      public LiquidationOrderLoadingConfiguration LiquidationOrderLoadingConfiguration { get; set; }
      public DateTime DateCreated { get; set; }
      public DateTime EndDate { get; set; }
      public bool StrategyState { get; set; }
      public StratgeyMode MakerMode { get; set; }
      public bool StopOnDailyTargetReached { get; set; }
   }
}
