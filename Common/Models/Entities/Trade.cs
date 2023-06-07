using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class Trade
   {
      public int TradeId { get; set; }
      public string ExchangeTradeId { get; set; }
      public int VenueId { get; set; }

      public Venue Venue { get; set; }
      public int CoinPairId { get; set; }
      public CoinPair CoinPair { get; set; }
      public string OrderId { get; set; }
      public DateTime DateCreated { get; set; } = DateTime.Now;
      public int SPId { get; set; }
      public SP SP { get; set; }
      public string InstanceName { get; set; }
      public bool IsBuy { get; set; }
      public decimal Price { get; set; }
      public decimal Quantity { get; set; }
      public decimal LeaveQuantity { get; set; }
      public decimal Fee { get; set; }
   }
}
