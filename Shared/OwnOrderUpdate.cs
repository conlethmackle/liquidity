using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncfusionLiquidity.Shared
{
   public class OwnOrderUpdate
   {
      public string Venue { get; set; }
      public int OrderId { get; set; }
      public decimal Price { get; set; }
      public decimal Quantity { get; set; }
      public bool IsBuy { get; set; }
      public decimal QuantityRemaining { get; set; }
      public string Status { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
