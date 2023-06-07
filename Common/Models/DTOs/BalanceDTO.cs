using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class BalanceDTO
   {
      public string Currency { get; set; }
      public string Exchange { get; set; }
      public decimal Available { get; set; }
      public decimal Total { get; set; }
      public decimal Held { get; set; }
   }
}
