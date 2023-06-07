using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
   public class OrderIdHolder
   {
      [ProtoMember(1)]
      [JsonPropertyName("symbol")]
      public string Symbol { get; set; }
      [ProtoMember(2)]
      [JsonPropertyName("orderId")]
      public string OrderId { get; set; }
      [ProtoMember(3)]
      [JsonPropertyName("clientOrderId")]
      public string ClientOrderId { get; set; }
      public string Account { get; set; }
      public string Instance { get; set; }
   }
}
