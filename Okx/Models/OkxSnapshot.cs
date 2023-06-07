using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   internal class OkxSnapshot
   {
      [JsonPropertyName("args")]
      public OkxSubscriptionData Args { get; set; }
      [JsonPropertyName("action")]
      public string Action { get; set; }
      [JsonPropertyName("data")]
      public OkxOrderBookChanges[] Data { get; set; }
   }
}
