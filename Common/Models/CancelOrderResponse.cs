using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Common.Models
{
   public class SingleCancelOrderResponseContainer
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("data")]
      public SingleCancelledOrderId CancelledOrderId { get; set; }
   }

   public class SingleCancelledOrderId
   {
      [JsonPropertyName("cancelledOrderId")]
      public string OrderId { get; set; }
      [JsonPropertyName("clientOrderId")]
      public string ClientOrderId { get; set; }
      public string Account { get; set; }
      public string Instance { get; set; }
   }

   public class CancelAllOrderResponseContainer
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("data")]
      public CancelAllCancelledOrderIds CancelledOrderIds { get; set; }
   }

   public class CancelAllCancelledOrderIds
   {
      [JsonPropertyName("cancelledOrderIds")]
      public string[] OrderIds { get; set; }
      [JsonPropertyName("clientOrderIds")]
      public string[] ClientOrderIds { get; set; }

   }
}
