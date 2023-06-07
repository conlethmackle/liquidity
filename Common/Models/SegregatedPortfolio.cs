using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public  class SegregatedPortfolio
   {
      public string Name { get; set; }
      public int PortfolioId { get; set; }
      List<ExchangeAccount> ExchangeAccounts { get; set; }
   }
}
