using System.Text.Json.Serialization;

namespace Okx.Models
{
   public class OkxOrderCancel
   {
      [JsonPropertyName("instId")]
      public string InstId { get; set; }
      [JsonPropertyName("ordId")]
      public string OrderId { get; set; }
      [JsonPropertyName("clOrdId")]
      public string ClientOrderId { get; set; }
   }
}
