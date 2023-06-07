using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class CoinPair
   {
      public int CoinPairId { get; set; }
      public string Name { get; set; }
      public virtual Coin PCoin { get; set; }
      public int PCoinId { get; set; }
      public virtual Coin SCoin { get; set; }
      public int SCoinId { get; set; }
   }
}
