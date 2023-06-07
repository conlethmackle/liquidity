using Common.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;

namespace Common.Models.DTOs
{
   public class StrategyExchangeConfigDTO
   {
      [Required]
      public string ConfigName { get; set; }
      public int StrategySPSubscriptionConfigId { get; set; }
      [Required]
      public int SPId { get; set; }
      public SPDTO SP { get; set; }
      [Required]
      public int StrategyId { get; set; }
      public StrategyDTO Strategy { get; set; }
      public List<ExchangeDetailsDTO> ExchangeDetails { get; set; }
   }
}
