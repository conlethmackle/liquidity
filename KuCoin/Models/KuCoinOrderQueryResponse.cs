using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class KuCoinOrderQueryResponse
   {
      [JsonPropertyName("id")] //"id": "5c35c02703aa673ceec2a168",   //orderid
      public string Id { get; set; }
      [JsonPropertyName("symbol")] //: "BTC-USDT",   //symbol
      public string Symbol { get; set; }
      [JsonPropertyName("side")]
      public string Side { get; set; }
      [JsonPropertyName("opType")] //: "DEAL",      // operation type: DEAL
      public string OpType { get; set; }
      [JsonPropertyName("type")] //: "limit",       // order type,e.g. limit,market,stop_limit.
      public string Type { get; set; }
      [JsonPropertyName("price")] //: "10",         // order price
      public decimal Price { get; set; }
      [JsonPropertyName("size")] //: "2",           // order quantity
      public decimal Quantity { get; set; }
      [JsonPropertyName("funds")] //: "0",          // order funds
      public decimal Funds { get; set; }
      [JsonPropertyName("dealFunds")] //: "0.166",  // deal funds
      public decimal DealFunds { get; set; }
      [JsonPropertyName("dealSize")] //: "2",       // deal quantity
      public decimal DealSize { get; set; }
      [JsonPropertyName("fee")] //: "0",            // fee
      public decimal Fee { get; set; }
      [JsonPropertyName("feeCurrency")] //: "USDT", // charge fee currency
      public string FeeCurrency { get; set; }
      [JsonPropertyName("stp")] //: "",             // self trade prevention,include CN,CO,DC,CB
      public string Stp { get; set; }
      [JsonPropertyName("stop")] //: "",            // stop type
      public string Stop { get; set; }
      [JsonPropertyName("stopTriggered")] //: false,  // stop order is triggered
      public bool StopTriggered { get; set; }
      [JsonPropertyName("stopPrice")] //: "0",      // stop price
      public decimal StopPrice { get; set; }
      [JsonPropertyName("timeInForce")] //: "GTC",  // time InForce,include GTC,GTT,IOC,FOK
      public string TimeInForce { get; set; }
      [JsonPropertyName("postOnly")] //: false,     // postOnly
      public bool PostOnly { get; set; }
      [JsonPropertyName("hidden")] //: false,       // hidden order
      public bool Hidden { get; set; }
      [JsonPropertyName("iceberg")] //: false,      // iceberg order
      public bool Iceberg { get; set; }
      [JsonPropertyName("visibleSize")] //: "0",    // display quantity for iceberg order
      public decimal VisibleSize { get; set; }
      [JsonPropertyName("cancelAfter")] //: 0,      // cancel orders time，requires timeInForce to be GTT
      public UInt64 CancelAfter { get; set; }
      [JsonPropertyName("channel")] //: "IOS",      // order source
      public string Channel { get; set; }
      [JsonPropertyName("clientOid")] //: "",       // user-entered order unique mark
      public string ClientOid { get; set; }
      [JsonPropertyName("remark")] //: "",          // remark
      public string Remark { get; set; }
      [JsonPropertyName("tags")] //: "",            // tag order source
      public string Tags { get; set; }
      [JsonPropertyName("isActive")] //: false,     // status before unfilled or uncancelled
      public bool IsActive { get; set; }
      [JsonPropertyName("cancelExist")] //: false,   // order cancellation transaction record
      public bool CancelExist { get; set; }
      [JsonPropertyName("createdAt")] //: 1547026471000,  // create time
      public UInt64 CreatedAt { get; set; }
      [JsonPropertyName("tradeType")] //: "TRADE"
      public string TradeType { get; set; }
   }
}
