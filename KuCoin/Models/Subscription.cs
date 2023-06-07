using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class Subscription
   {
      [JsonPropertyName("id")]
      public string Id { get; set; }
      [JsonPropertyName("type")]
      public string Type { get; set; }
      [JsonPropertyName("topic")]
      public string Topic { get; set; }
      [JsonPropertyName("privateChannel")]
      public bool PrivateChannel { get; set; }
      [JsonPropertyName("response")]
      public bool Response { get; set; }
   }
}
