using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class LiquidationManualOrderLoading
   {
      public int Id { get; set; }
      public int StrategySPSubscriptionConfigId { get; set; }
      public StrategySPSubscriptionConfig StrategyInstance { get; set; }
      public int OrderNo { get; set; }
      public decimal Percentage { get; set; } 
   }
}
