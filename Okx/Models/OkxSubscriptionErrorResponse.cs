using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   internal class OkxSubscriptionErrorResponse
   {
      [JsonPropertyName("event")]
      public string Event { get; set; }
      [JsonPropertyName("code")]
      public int Code { get; set; }
      [JsonPropertyName("msg")]
      public string Msg { get; set; }
   }
}
