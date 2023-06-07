using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common.Models.Entities;

namespace Common.Models.DTOs
{
   public class LiquidationConfigurationDTO
   {
      public int Id { get; set; }
      [Required]
      public int SPId { get; set; }
      public SPDTO SP { get; set; }
      public int StrategyId { get; set; }
      public StrategyDTO Strategy { get; set; }
      [Required]
      public int StrategySPSubscriptionConfigId { get; set; }
      public StrategyExchangeConfigDTO StrategySPSubscriptionConfig { get; set; }
      public string Exchanges { get; set; }
    //  [Required]
      public int CoinPairId { get; set; }
      public CoinPairDTO CoinPair { get; set; }
      [Required]
      public decimal SubscriptionPrice { get; set; }
      [Required]
      public decimal CoinAmount { get; set; }
      [Required] 
      public decimal OrderSize { get; set; }
  //    [Required]
      public string Side { get; set; } = "SELL";
 //     [Required]
      public decimal DailyLiquidationTarget { get; set; }
     
      public decimal MaxOrderSize { get; set; }
      [Required]
      public decimal PercentageSpreadFromFV { get; set; }
      [Required]
      public decimal PercentageSpreadLowerThreshold { get; set; }
      [Required]
      public int ShortTimeInterval { get; set; }
      public int LongTimeInterval { get; set; }
      public int CancelTimerInterval { get; set; }
      public int TakerModeTimeInterval { get; set; }
      [Required]
      public int BatchSize { get; set; }
      [Required]
      public int PriceDecimals { get; set; }
      [Required]
      public int AmountDecimals { get; set; }
      [Required]
     
      public int LiquidationOrderLoadingConfigurationId { get; set; }
      public LiquidationOrderLoadingConfigurationDTO LiquidationOrderLoadingConfiguration { get; set; }
      public DateTime DateCreated { get; set; }
      [Required]
      public DateTime EndDate { get; set; }
      public int NumDaysRemaining { get; set; }
      public decimal LivePrice { get; set; }
      public string  CurrencyLiquidatedFrom { get; set; }
      public string CurrencyLiquidatedTo { get; set; }
      public decimal BalanceLiquidationFrom { get; set; }
      public decimal BalanceLiquidationTo { get; set; }
      public decimal DailyLiquidationFrom { get; set; }
      public decimal DailyLiquidationTo { get; set; }
      public int TotalFills { get; set; }
      public int DailyFills { get; set; }
      public bool StrategyState { get; set; }
      public string StrategyStateStr { get; set; } = "";
      public StratgeyMode MakerMode { get; set; }
      public bool StopOnDailyTargetReached { get; set; }
      public string LiquidatedCoin { get; set; }
      public string StableCoin { get; set; }
      public decimal TotalCoinLiquidated { get; set; }
      public decimal TotalCoinLiquidatedToday { get; set; }
      public decimal StartOfDayCoinBalance { get; set; }
      public decimal StartOfDayStableBalance { get; set; }
      public bool TradingPaused { get; set; }
      public bool LiquidationFinishedForToday { get; set; }
   }
}
