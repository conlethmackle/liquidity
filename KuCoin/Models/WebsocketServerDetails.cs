//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class WebsocketEndpoints
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }

      [JsonPropertyName("data")]
      public WebsocketTokenAndServer Data { get; set; }
   }

   public class WebsocketTokenAndServer
   {
      [JsonPropertyName("token")]
      public string Token { get; set; }
      [JsonPropertyName("instanceServers")]
      public List<WebsocketServerDetails> Servers { get; set; }
   }

   public class WebsocketServerDetails
   {
      [JsonPropertyName("endpoint")]
      public string Endpoint { get; set; }
      [JsonPropertyName("encrypt")]
      public bool Encrypt { get; set; }
      [JsonPropertyName("protocol")]
      public string Protocol { get; set; }
      [JsonPropertyName("pingInterval")]
      public int PingInterval { get; set; }
      [JsonPropertyName("pingTimeout")]
      public int PingTimeout { get; set; }
   }
}
