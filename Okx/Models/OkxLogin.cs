using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxLogin
   {
      [JsonPropertyName("apiKey")]
      public string ApiKey { get; set; }
      [JsonPropertyName("passphrase")]
      public string PassPhrase { get; set; }
      [JsonPropertyName("timestamp")]
      public string Timestamp { get; set; }
      [JsonPropertyName("sign")]
      public string Sign { get; set; }
   }
}
