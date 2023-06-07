using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class KuCoinCoinOrderQueryResponseData
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("data")]
      public KuCoinOrderQueryResponseContainer Data { get; set; }
   }

   public class KuCoinOrderQueryResponseContainer
   {
      [JsonPropertyName("currentPage")] //: 1,
      public int CurrentPage { get; set; }
      [JsonPropertyName("pageSize")] //: 1,
      public UInt64 PageSize { get; set; }
      [JsonPropertyName("totalNum")] //: 153408,
      public UInt64 totalNum { get; set; }
      [JsonPropertyName("totalPage")] //: 153408,
      public UInt64 totalPage { get; set; }
      [JsonPropertyName("items")] //: [
      public KuCoinOrderQueryResponse[] Items { get; set; }
   }
}
