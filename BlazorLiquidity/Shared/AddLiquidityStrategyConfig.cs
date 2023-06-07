using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorLiquidity.Shared
{
   public class AddLiquidityStrategyConfig
   {
      public int StrategySPSubscriptionConfigId { get; set; }
      public string Symbol { get; set; }
      public decimal SubscriptionPrice { get; set; }
      public decimal InitialNumberOfCoins { get; set; }
      public decimal LotSize { get; set; }
      public decimal MaxSpreadFromFV { get; set; }
      public decimal MinSpreadFromFV { get; set; }
      public int BatchSize { get; set; }
      public int ShortPeriodInterval { get; set; }
      public int LongPeriodInterval { get; set; }
   }
}
