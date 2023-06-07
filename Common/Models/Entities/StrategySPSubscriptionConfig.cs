using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace Common.Models.Entities
{
   [Index(nameof(StrategySPSubscriptionConfig.ConfigName), IsUnique = true)]
   public class StrategySPSubscriptionConfig
   {
      public int StrategySPSubscriptionConfigId { get; set; }            
      public string ConfigName { get; set; }
      public int SPId { get; set; }
      public SP SP { get; set; }
      public int StrategyId { get; set; }
      public Strategy Strategy { get; set; }
      public ICollection<ExchangeDetails> ExchangeDetails { get; set; }
   }
}
