using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class OpenOrderDTO
   {
      public int OpenOrderId { get; set; }
      public int ExchangeId { get; set; }
      public int CoinPairId { get; set; }
      public DateTime DateCreated { get; set; }
      public int SPId { get; set; }    
      public bool IsBuy { get; set; }
      public decimal Price { get; set; }
      public decimal Quantity { get; set; }
      public decimal LeaveQuantity { get; set; }
   }
}
