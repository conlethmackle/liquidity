using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxAccountPush
   {
      [JsonPropertyName("arg")]
      public OkxSubscriptionData Arg { get; set; }
      [JsonPropertyName("data")]
      public OkxTopAccountPush[] Data { get; set; }
   }
}
