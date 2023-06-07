using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;


namespace Common.Models.Entities
{
   public class Balance
   {
      public int BalanceId { get; set; }
      public int SPId { get; set; }
      public virtual SP SP { get; set; }
      public int ExchangeDetailsId { get; set; }
      public virtual ExchangeDetails ExchangeDetails { get; set; }
      public int CoinId { get; set; }
      public virtual Coin Coin { get; set; }
      public string Payload { get; set; }
      public decimal Amount { get; set; }
      
   }
}
