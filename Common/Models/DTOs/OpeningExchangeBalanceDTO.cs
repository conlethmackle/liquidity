using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class OpeningExchangeBalanceDTO
   {
      [JsonPropertyName("openingExchangeBalanceId")]
      public int OpeningExchangeBalanceId { get; set; }
      [JsonPropertyName("description")]
      public string Description { get; set; }
      [JsonPropertyName("liquidatingFromCurrency")]
      public string LiquidatingFromCurrency { get; set; }
      [JsonPropertyName("liquidatingFromOpeningBalance")]
      public decimal LiquidatingFromOpeningBalance { get; set; }
      [JsonPropertyName("amountToBeLiquidated")]
      public decimal AmountToBeLiquidated { get; set; }
      [JsonPropertyName("liquidatingToCurrency")]
      public string LiquidatingToCurrency { get; set; }
      [JsonPropertyName("liquidatingToOpeningBalance")]
      public decimal LiquidatingToOpeningBalance { get; set; }
      [JsonPropertyName("created")]
      public DateTime Created { get; set; }

   }
}
