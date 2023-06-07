using System.Text.Json.Serialization;

namespace Okx.Models
{
   public class OkxOrderPlacementResponse
   {
      [JsonPropertyName("ordId")]
      public string OrderId { get; set; }
      [JsonPropertyName("clOrdId")]
      public string ClientOrderId { get; set; }
      [JsonPropertyName("tag")]
      public string Tag { get; set; }
      [JsonPropertyName("sCode")]
      public int sCode { get; set; }
      [JsonPropertyName("sMsg")]
      public string Msg { get; set; }
   }
}
