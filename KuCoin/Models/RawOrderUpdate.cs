using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.Models
{
   public class RawOrderUpdate
   {
      public decimal Price { get; set; }
      public decimal Quantity { get; set; }
      public Int64 Sequence { get; set; }
   }
}
