using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxWebsocketRequestData<T>
   {
      [JsonPropertyName("op")]
      public string Operation { get; set; }
      [JsonPropertyName("args")]
      public T[] Args { get; set; }
   }
}
