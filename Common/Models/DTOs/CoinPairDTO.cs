using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models.Entities;

namespace Common.Models.DTOs
{
   public class CoinPairDTO
   {
      public int CoinPairId { get; set; }
      public string Name { get; set; }
      public virtual CoinDTO PCoin { get; set; }
      public int PCoinId { get; set; }
      public virtual CoinDTO SCoin { get; set; }
      public int SCoinId { get; set; }
   }
}
