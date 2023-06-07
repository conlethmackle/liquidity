using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   internal class OkxSubscriptionSuccessResponse<T>
   {
      [JsonPropertyName("event")]
      public string Event { get; set; }
      [JsonPropertyName("arg")]
      public T Arg { get; set; }
     
   }
}
