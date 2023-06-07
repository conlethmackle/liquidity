using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class ResponseContainer
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("data")]
      public string Data { get; set; }
   }
}
