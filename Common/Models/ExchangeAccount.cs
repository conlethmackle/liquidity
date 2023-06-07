using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public class ExchangeAccount
   {
      public int Id { get; set; }
      public string ExchangeName { get; set; }
      public List<ExchangeBalance> ExchangeBalances { get; set; }
   }
}
