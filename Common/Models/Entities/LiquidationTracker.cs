using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class LiquidationTracker
   {
      public int LiquidationTrackerId { get; set; }
      public int StrategySubscriptionConfigId { get; set; }
      public StrategySPSubscriptionConfig StrategySpSubscriptionConfig { get; set; }
      public List<OpeningSubscription> OpeningSubscriptions { get; set; }
      public int CoinId { get; set; }
      public int RunId { get; set; }
      public DateTime DateStarted { get; set; }
      public int TotalDaysToRun { get; set; }
      public int CurrentDayNo { get; set; }
      public int NumberOfFillsInDay { get; set; }
      public int TotalNumberOfFillsIn { get; set; }
      public decimal CoinQtyLiquidatedInDay { get; set; }
      public decimal DollarQtyForDay { get; set; }
      public decimal TotalCoinQtyLiquidated { get; set; }
      public decimal TotalDollarQtyLiquidated { get; set; }
   }
}
