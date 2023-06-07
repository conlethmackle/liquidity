using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class LiquidationManualOrderLoadingDTO
   {
      public int Id { get; set; }
      public int StrategySPSubscriptionConfigId { get; set; }
      public string Name { get; set; }
      public StrategyExchangeConfigDTO StrategyInstance { get; set; }
      public int OrderNo { get; set; }
      public decimal Percentage { get; set; }
   }
}
