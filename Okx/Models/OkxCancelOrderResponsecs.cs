using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxCancelOrderResponsecs
   {
      [JsonPropertyName("clOrdId")]
      public string ClOrdId { get; set; }
      [JsonPropertyName("ordId")]
      public string OrdId { get; set; }
      [JsonPropertyName("sCode")]
      public string SCode { get; set; }
      [JsonPropertyName("sMsg")]
      public string SMsg { get; set; }

   }
}
