using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class AccountContainer
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("data")]
      public KuCoinAccount[] Data { get; set; }
   }
   public class KuCoinAccount
   {
      [JsonPropertyName("id")] //  //accountId
      public string Id { get; set; }
      [JsonPropertyName("currency")] //: "BTC",  //Currency
      public string Currency { get; set; }
      [JsonPropertyName("type")] //: "main",     //Account type, including main and trade
      public string AccountType { get; set; }
      [JsonPropertyName("balance")] //: "237582.04299",  //Total assets of a currency
      public string Balance { get; set; }
      [JsonPropertyName("available")] //: "237582.032",  //Available assets of a currency
      public string Available { get; set; }
      [JsonPropertyName("holds")] //: "0.01099" //Hold assets of a currency
      public string Hold { get; set; }
   }
}
