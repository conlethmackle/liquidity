using System;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class OrderFillResponseData
   {
      [JsonPropertyName("code")]
      public string Code { get; set; }
      [JsonPropertyName("data")]
      public OrderFillResponseContainer Data { get; set; }
   }

   public class OrderFillResponseContainer
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
      public OrderFillResponse[] Items { get; set; }
   }
   public class OrderFillResponse
   {
      [JsonPropertyName("symbol")] //:"BTC-USDT",    //symbol
      public string Symbol { get; set; }
      [JsonPropertyName("tradeId")] //:"5c35c02709e4f67d5266954e",   //trade id
      public string TradeId { get; set; }
      [JsonPropertyName("orderId")] //:"5c35c02703aa673ceec2a168",   //order id
      public string OrderId { get; set; }
      [JsonPropertyName("counterOrderId")] //:"5c1ab46003aa676e487fa8e3",  //counter order id
      public string CounterOrderId { get; set; }
      [JsonPropertyName("side")] // :"buy")] //,   //transaction direction,include buy and sell
      public string Side { get; set; }
      [JsonPropertyName("liquidity")] //:"taker",  //include taker and maker
      public string Liquidity { get; set; }
      [JsonPropertyName("forceTaker")] //:true,  //forced to become taker
      bool ForcedTaker { get; set; }
      [JsonPropertyName("price")] //:"0.083",   //order price
      public decimal Price { get; set; }
      [JsonPropertyName("size")] //:"0.8424304",  //order quantity
      public decimal Quantity { get; set; }
      [JsonPropertyName("funds")] //:"0.0699217232",  //order funds
      public decimal Funds { get; set; }
      [JsonPropertyName("fee")] //:"0",  //fee
      public decimal Fee { get; set; }
      [JsonPropertyName("feeRate")] //:"0",  //fee rate
      public decimal FeeRate { get; set; }
      [JsonPropertyName("feeCurrency")] //:"USDT",  // charge fee currency
      public string FeeCurrency { get; set; }
      [JsonPropertyName("stop")] //:"",        // stop type
      public string Stop { get; set; }
      [JsonPropertyName("type")] //:"limit",  // order type,e.g. limit,market,stop_limit.
      public string Type { get; set; }
      [JsonPropertyName("createdAt")] //:1547026472000,  //time
      public UInt64 CreatedAt { get; set; }
      [JsonPropertyName("tradeType")] //: "TRADE"
      public string TradeType { get; set; }
   }
}
