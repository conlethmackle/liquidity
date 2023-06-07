using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Dashboard.Models
{
   public class SP
   {
      [JsonPropertyName("name")]
      public string Name { get; set; }
      [JsonPropertyName("description")]
      public string Description { get; set; }
      public List<Balance> Balances { get; set; }
      public List<SubAccount> SubAccounts { get; set; }
   }

   public class Balance
   {
      [JsonPropertyName("currency")]
      public string Currency { get; set; }
      [JsonPropertyName("total")]
      public decimal Total { get; set; }
      [JsonPropertyName("available")]
      public decimal Available { get; set; }
      [JsonPropertyName("held")]
      public decimal Held { get; set; }
   }

   public class SubAccount
   {
      [JsonPropertyName("name")]
      public string Name { get; set; }
      [JsonPropertyName("currency")]
      public string Currency { get; set; }
      [JsonPropertyName("total")]
      public decimal Total { get; set; }
      [JsonPropertyName("available")]
      public decimal Available { get; set; }
      [JsonPropertyName("held")]
      public decimal Held { get; set; }
   }
}
