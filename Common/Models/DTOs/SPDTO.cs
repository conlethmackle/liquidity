using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class SPDTO
   {
      public int SPId { get; set; }
      public string Name { get; set; }
      public int FundId { get; set; }
      public FundDTO Fund { get; set; }
      public string Description { get; set; }
      public List<ExchangeDetailsDTO> Exchanges { get; set; } = new List<ExchangeDetailsDTO>();
      public List<BalanceDTO> Balances { get; set; } = new List<BalanceDTO>();  
      public DateTime DateCreated { get; set; }

      
   }
}
