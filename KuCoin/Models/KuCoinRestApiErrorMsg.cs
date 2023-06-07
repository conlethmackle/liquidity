using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class KuCoinRestApiErrorMsg
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("msg")]
      public string Msg { get; set; }
   }
}
