using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public class StrategyExchangeSymbolConfig
   {
      public ExchangesAndSymbols[] ExchangeSymbols { get; set; }
   }

   public class ExchangesAndSymbols
   {
      public int ExchangeId { get; set; }
      public string[] Symbols { get; set; }
   }
}
