using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxCancelOrder
   {

      [JsonPropertyName("instId")]
      public string InstId { get; set; }
      [JsonPropertyName("ordId")]
      public string OrderId { get; set; }
      [JsonPropertyName("clOrdId")]
      public string ClOrdId { get; set; }
   }
}
