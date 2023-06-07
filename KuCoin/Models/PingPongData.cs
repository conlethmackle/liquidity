using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   internal class PingPongData
   {
      [JsonPropertyName("id")]
      public string Id { get; set; }
      [JsonPropertyName("type")]
      public string Type { get; set; }

   }
}
