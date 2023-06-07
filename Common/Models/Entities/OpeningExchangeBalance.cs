using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class OpeningExchangeBalance
   {
      public int OpeningExchangeBalanceId { get; set; }
      public string Description { get; set; }
      public string LiquidatingFromCurrency { get; set; }
      public decimal LiquidatingFromOpeningBalance { get; set; }
      public decimal AmountToBeLiquidated { get; set; }
      public string  LiquidatingToCurrency { get; set; }
      public decimal LiquidatingToOpeningBalance { get; set; }
      public DateTime Created { get; set; }
   }
}
