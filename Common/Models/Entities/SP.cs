using System;
using System.Collections.Generic;

namespace Common.Models.Entities
{
   public class SP
   {
      public int SPId { get; set; }
      public string Name { get; set; }
      public int FundId { get; set; }
      public Fund Fund { get; set; }
      public bool IsEnabled { get; set; }
      public string Description { get; set; }
   
      public DateTime DateCreated { get; set; }

      public SP()
      {
      //   Exchanges = new List<ExchangeDetails>();
        // Balances = new List<Balance>();
      }
   }
}
