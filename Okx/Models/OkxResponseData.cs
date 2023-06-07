
using System.Text.Json.Serialization;

namespace Okx.Models
{
   public class OkxResponseData<T>
   {
      [JsonPropertyName("code")] 
      public string Code { get; set; }
      [JsonPropertyName("msg")] 
      public string Msg { get; set; }
      [JsonPropertyName("data")]
      public T Data { get; set; }
   }
}
