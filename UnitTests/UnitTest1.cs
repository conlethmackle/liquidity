using NUnit.Framework;
//using Newtonsoft.Json;
using KuCoin.Models;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnitTests
{
   public class ResponseData1<T>
   {   
      [JsonPropertyName("type")] //:"message",
      public string Type { get; set; }
      [JsonPropertyName("topic")] //:"/market/ticker:BTC-USDT",
      public string Topic { get; set; }
      [JsonPropertyName("subject")] // :"trade.ticker",
      public string Subject { get; set; }
      [JsonPropertyName("channelType")]
      public string ChannelType { get; set; }
      [JsonPropertyName("data")]
      public T Data { get; set; }
   }

   public class Level2Changes1
   {
      [JsonPropertyName("sequenceStart")] //:1545896669105,
      public Int64 SequenceStart { get; set; }
      [JsonPropertyName("sequenceEnd")] //:1545896669106,
      public Int64 SequenceEnd { get; set; }
      [JsonPropertyName("symbol")] //:"BTC-USDT" //,
      public string Symbol { get; set; }
      [JsonPropertyName("changes")] //:{
      public OrderBookChanges1 Changes { get; set; }
      //  "asks":[["6","1","1545896669105"]],           //price, size, sequence
      //   "bids":[["4","1","1545896669106"]]
   }

   public class OrderBookChanges1
   {
      [JsonPropertyName("asks")]
      public List<List<string>> Asks { get; set; } = new List<List<string>>()
      {


      };
      [JsonPropertyName("bids")]
      public List<List<string>> Bids { get; set; } = new List<List<string>>()
      {



      };
   }



   public class Tests
   {
      [SetUp]
      public void Setup()
      {
      }

      [Test]
      public void Test1()
      {
         var more = "{\"type\":\"message\",\"topic\":\" /market/level2:KCS-USDT\",\"subject\":\"trade.l2update\",\"data\":{\"sequenceStart\":1636697871277,\"symbol\":\"KCS-USDT\",\"changes\":{\"asks\":[[\"21.549\",\"691.09449418\",\"1636697871277\"]],\"bids\":[]},\"sequenceEnd\":1636697871277}}";
         var jsonData = "{\"type\":\"message\",\"topic\":\"/market/level2:KCS-USDT\",\"subject\":\"trade.l2update\",\"data\":{\"sequenceStart\":1636696609333,\"symbol\":\"KCS-USDT\",\"changes\":{\"asks\":[],\"bids\":[[\"20.484\",\"0.04\",\"1636696609333\"]]},\"sequenceEnd\":1636696609333}}";
         var level2Data = "{\"sequenceStart\":1636696609333,\"symbol\":\"KCS-USDT\",\"changes\":{\"asks\":[[\"20.484\",\"0.04\",\"1636696609333\"], [\"20.484\",\"0.04\",\"1636696609333\"]],\"bids\":[[\"20.484\",\"0.04\",\"1636696609333\"], [\"20.484\",\"0.04\",\"1636696609333\"], [\"20.484\",\"0.04\",\"1636696609333\"]]},\"sequenceEnd\":1636696609333}";
          var v1 = JsonSerializer.Deserialize<ResponseData1<Level2Changes1>>(more);
         var v3 = v1.Data;
         var v4 = v3.Changes;
         foreach (var side in v4.Asks)
         {
            var price = side[0];
            var size = side[1];
            var seq = side[2];
         }
         foreach (var side in v4.Bids)
         {
            var price = side[0];
            var size = side[1];
            var seq = side[2];
         }

         //   var v2 = JsonSerializer.Deserialize<Level2Changes>()
         try
         {

            //  var responseData = JsonConvert.DeserializeObject<ResponseData>(jsonData);

            //  var res1 = JsonConvert.DeserializeObject<Level2Changes>(responseData.Data);
            // var res = JsonConvert.DeserializeObject<Level2Changes>(level2Data);
            /*  var sides = res.Changes;
              foreach (var side in sides.Asks)
              {
                 var price = side[0];
                 var size = side[1];
                 var seq = side[2];
              }
              foreach (var side in sides.Bids)
              {
                 var price = side[0];
                 var size = side[1];
                 var seq = side[2];
              }*/
         }
         catch (Exception e)
         {

         }
         Assert.Pass();
      }

      [Test]
      public void Test2()
      {
         var msg = "{\"code\":\"200000\",\"data\":{\"orderId\":\"625572ec0091a600010c9870\"}}";
         Assert.IsFalse( IsResponseAnError(msg));
      }

      [Test]
      public void Test4()
      {
         var msg = "{\"code\":\"400000\",\"data\":{\"orderId\":\"625572ec0091a600010c9870\"}}";
         Assert.IsTrue(IsResponseAnError(msg));
      }

      private static bool IsResponseAnError(string response)
      {
         string strRegex = $".*?code.*?:.*200000.*?";

         var regex = new Regex(strRegex);

         if (regex.IsMatch(response))
            return false;
         return true;
      }
   }
}