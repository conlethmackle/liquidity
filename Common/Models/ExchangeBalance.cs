using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Common.Models
{
   public class ExchangeBalance
   {
      [JsonPropertyName("accountId")]
      public string AccountId { get; set; }
      [JsonPropertyName("currency")] //: "BTC",  //Currency
      public string Currency { get; set; }              
      [JsonPropertyName("total")] //: "237582.04299",  //Total assets of a currency
      public decimal Total { get; set; }
      [JsonPropertyName("available")] //: "237582.032",  //Available assets of a currency
      public decimal Available { get; set; }
      [JsonPropertyName("holds")] //: "0.01099" //Hold assets of a currency
      public decimal Hold { get; set; }
      [JsonPropertyName("Type")]
      public string Type { get; set; } // Kucoin - main or trade - might be blank
      public string Account { get; set; }
      public string Instance { get; set; }

      public ExchangeBalance()
      {
         Total = 0;
         Available = 0; 
         Hold = 0;
      }
   }
}
