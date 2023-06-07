using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class OrderBookSnapShotContainer
   {
      //  "sequence": "3262786978",
      //  "time": 1550653727731,
      // "bids": [["6500.12", "0.45054140"],
      //           ["6500.11", "0.45054140"]],  //[price，size]
      // "asks": [["6500.16", "0.57753524"],
      //          ["6500.15", "0.57753524"]]  

      //   {"code":"200000","data":{"time":1648747354572,"sequence":"1636714873115","bids":[["20.314","6.2692"],["20.309","8.0672"],["20.308","18.6948"],["20.307","3.512"],["20.306","8.4385"],["20.304","9"],["20.301","82.0954"],["20.299","9"],["20.294","9.0714"],["20.292","3.512"],["20.29","35.241"],["20.289","9"],["20.288","0.2737"],["20.287","0.1146"],["20.284","9.51"],["20.283","0.7672"],["20.281","0.0115"],["20.28","1.0152"],["20.279","5.7536"],["20.278","9"],["20.276","191.4567"],["19.7","26.72"]],"asks":[["20.32","75"],["20.321","200"],["20.322","1060.22466982"],["20.324","21"],["20.329","17.9354"],["20.33","43.1"],["20.333","0.02"],["20.334","18"],["20.339","18"],["20.343","9.1087"],["20.344","9"],["20.35","11.4576"],["20.351","72.5"],["20.355","0.11353231"],["20.359","180.2733"],["20.36","8.0295"],["20.363","3.997"],["20.374","27.3153"],["22.4154","1878.16477945"],["433669","1.22966825"]]}
      //}
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("data")]
      public KuCoinOrderBookSnapShot Data { get; set; }
   }

   public class KuCoinOrderBookSnapShot
   {
      [JsonPropertyName("time")]
      public UInt64 Time { get; set; }
      [JsonPropertyName("sequence")] //:1545896669105,
      public string Sequence { get; set; }

     [JsonPropertyName("bids")]
     public List<List<string>> Bids { get; set; } = new List<List<string>>()
     {
     };

     [JsonPropertyName("asks")]
     public List<List<string>> Asks { get; set; } = new List<List<string>>()
     {
     };
   }
   
}
