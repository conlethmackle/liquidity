using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxRestApiErrorMsg
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("msg")]
      public string Msg { get; set; }
   }
}
