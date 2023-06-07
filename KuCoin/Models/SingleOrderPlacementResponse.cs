using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using ProtoBuf;
using Common.Models;

namespace KuCoin.Models
{
   [ProtoContract]
   public class SingleOrderPlacementResponse
   {
      [ProtoMember(1)]
      [JsonPropertyName("code")]
      public string Code { get; set; }
    //  {"code":"200000","data":{"orderId":"624dd0ebf1ee300001d84235"}}
      [ProtoMember(2)]
      [JsonPropertyName("data")]
      public OrderIdHolder Data { get; set; }
   }

   
}
