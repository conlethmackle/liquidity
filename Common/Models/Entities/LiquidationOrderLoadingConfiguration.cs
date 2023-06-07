using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class LiquidationOrderLoadingConfiguration
   {
      public int Id { get; set; }
      public bool IsAuto { get; set; }
      public string Name { get; set; }
      public decimal StartPercentage { get; set; }
      public decimal ScalingFactor { get; set; }
      public bool IsHighestFirst { get; set; }
   }
}
