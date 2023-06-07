using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{

   public class OkxOrderBookChanges
   {
      [JsonPropertyName("asks")]
      public List<List<string>> Asks { get; set; } = new List<List<string>>()
      {

      };
      [JsonPropertyName("bids")]
      public List<List<string>> Bids { get; set; } = new List<List<string>>()
      {
      };
   }
}
