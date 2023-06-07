using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxSubscriptionRequestData
   {
      [JsonPropertyName("op")]
      public string Operation { get; set; }
      [JsonPropertyName("args")]
      public OkxSubscriptionData[] Args { get; set; }
   }

   public class OkxSubscriptionData
   {
      [JsonPropertyName("channel")]
      public string Channel { get; set; }
      [JsonPropertyName("instId")]
      public string InstId { get; set; }
      [JsonPropertyName("instFamily")]
      public string InstFamily { get; set; }
      [JsonPropertyName("instType")]
      public string InstType { get; set; }
      [JsonPropertyName("uid")]
      public string Uid { get; set; }
   }
}
