using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Common.Models;
using Common.Messages;
using Okx.Config;
using System.Globalization;
using Okx.Extensions;
using Okx.Models;

namespace Okx.RestApi
{
   public interface IOkxRestApiClient
   {
      void Configure(PrivateConnectionLogon config);
      Task<Result<OrderIdHolder>> PlaceSingleOrder(OkxOrderPlacement order);
      Task<Result<SingleCancelledOrderId>> CancelOrder(OrderIdHolder orderIdHolder);
      Task<Result<CancelAllCancelledOrderIds>> CancelAllOrders(string symbol);
      Task<Result<List<OrderQueryResponse>>> GetOpenOrdersList();
      Task<Result<List<ExchangeBalance>>> GetBalances();
   }

   public class OkxRestApiClient : IOkxRestApiClient
   {
      private readonly ILogger<OkxRestApiClient> _logger;
      private readonly OkxConnectionConfig _config;
      private readonly HttpClient _client;
      private const string OK_Response = "0";
      private JsonSerializerOptions _jsonSerializerOptions { get; set; }

      private PrivateConnectionLogon _apiKeyData { get; set; } = null;

      public OkxRestApiClient(ILoggerFactory loggerFactory, HttpClient client, IOptions<OkxConnectionConfig> privateConfig)
      {
         _logger = loggerFactory.CreateLogger<OkxRestApiClient>();
         _config = privateConfig.Value;
         _client = client;
         _jsonSerializerOptions = new JsonSerializerOptions()
         {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
         };
      }

      public void Configure(PrivateConnectionLogon config)
      {
         _apiKeyData = config;
      }
      private void GetPrivateHeaders(HttpRequestMessage messageHandler, string httpVerb, string method, string extra = "")
      {

         var now = DateTime.UtcNow;
         
         var timestamp = now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
        
         var body = now + httpVerb + method + "/"  + extra;
         var signature = Convert.ToBase64String(HashHMAC(new UTF8Encoding().GetBytes(_apiKeyData.Secret),
                  new UTF8Encoding().GetBytes(body)));

        // var passphrase = Convert.ToBase64String(HashHMAC(new UTF8Encoding().GetBytes(_apiKeyData.Secret),
           //       new UTF8Encoding().GetBytes(_apiKeyData.PassPhrase)));

         messageHandler.Headers.Add("OK-ACCESS-SIGN", signature);
         messageHandler.Headers.Add("OK-ACCESS-TIMESTAMP", timestamp);
         messageHandler.Headers.Add("OK-ACCESS-KEY", _apiKeyData.ApiKey);
         messageHandler.Headers.Add("OK-ACCESS-PASSPHRASE", _apiKeyData.PassPhrase);
         
         messageHandler.Content = new StringContent(
                              extra,
                              System.Text.Encoding.UTF8,
                              "application/json");


         if (httpVerb.Equals("POST"))
         {
            messageHandler.Content = new StringContent(
                                 extra,
                                 System.Text.Encoding.UTF8,
                                 "application/json");
         }
      }

      private static byte[] HashHMAC(byte[] key, byte[] message)
      {
         var hash = new HMACSHA256(key);
         return hash.ComputeHash(message);
      }

      public async Task<Result<OrderIdHolder>> PlaceSingleOrder(OkxOrderPlacement order)
      {
         try
         {
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Post,
               RequestUri = new Uri(_config.Url + "/api/v5/trade/order"),
            };
            var body = JsonSerializer.Serialize(order);
            GetPrivateHeaders(messageHandler, "POST", "/api/v5/trade/order", body);
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<OkxRestApiErrorMsg>(responseContent);
                     return Result.Fail<OrderIdHolder>(errorResponse?.Msg);
                  }

                  var data = JsonSerializer.Deserialize<OkxResponseData<OkxOrderPlacementResponse[]>>(responseContent, _jsonSerializerOptions);
                  var orderResponse = data.Data;
                  if (orderResponse != null)
                  {
                     var orderIdHolder = new OrderIdHolder()
                     {
                        OrderId = orderResponse[0].OrderId,
                        ClientOrderId = orderResponse[0].ClientOrderId,
                     };
                     return Result.OK(orderIdHolder);
                  }
                  else
                  {
                     return Result.Fail<OrderIdHolder>("No OrderResponse Data");
                  }
                  
                  
               }
               var msg = $"Error in data returned from Okx /api/v5/trade/order {response.StatusCode}";
               _logger.LogError("Error in data returned from Okx {Endpoint} {StatusCode}", "/api/v5/trade/order", response.StatusCode);
               return Result.Fail<OrderIdHolder>(msg);
            }
         }
         catch (Exception e)
         {
            var errorMsg = $"Error connecting to {_config.Url}/api/v1/orders with error {e.Message}";
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
               _config.Url, "/api/v5/trade/order", e.Message);
            return Result.Fail<OrderIdHolder>(errorMsg);
         }
      }

      public async Task<Result<SingleCancelledOrderId>> CancelOrder(OrderIdHolder holder)
      {
         try
         {
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Post,
               RequestUri = new Uri(_config.Url + "/api/v5/trade/cancel-order"),
            };

            var cancelOrder = holder.CreateOkxCancel();

            var body = JsonSerializer.Serialize(cancelOrder);
            GetPrivateHeaders(messageHandler, "POST", "/api/v5/trade/cancel-order", body);
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<OkxRestApiErrorMsg>(responseContent);
                     return Result.Fail<SingleCancelledOrderId>(errorResponse?.Msg);
                  }

                  var data = JsonSerializer.Deserialize<OkxResponseData<OkxCancelOrderResponsecs[]>>(responseContent, _jsonSerializerOptions);
                  var orderResponse = data.Data;
                  if (orderResponse != null)
                  {
                     var orderIdHolder = new SingleCancelledOrderId()
                     {
                        OrderId = orderResponse[0].OrdId,
                        ClientOrderId = orderResponse[0].ClOrdId,
                     };
                     return Result.OK(orderIdHolder);
                  }
                  else
                  {
                     return Result.Fail<SingleCancelledOrderId>("No OrderResponse Data");
                  }


               }
               var msg = $"Error in data returned from Okx /api/v5/trade/order {response.StatusCode}";
               _logger.LogError("Error in data returned from Okx {Endpoint} {StatusCode}", "/api/v5/trade/order", response.StatusCode);
               return Result.Fail<SingleCancelledOrderId>(msg);
            }
         }
         catch (Exception e)
         {
            var errorMsg = $"Error connecting to {_config.Url}/api/v5/trade/cancel-order with error {e.Message}";
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
               _config.Url, "/api/v5/trade/cancel-order", e.Message);
            return Result.Fail<SingleCancelledOrderId>(errorMsg);
         }
      }

      public async Task<Result<List<OrderQueryResponse>>> GetOpenOrdersList()
      {
         try
         {
            var cmd = $"/api/v5/trade/orders-pending";
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Get,
               RequestUri = new Uri(_config.Url + cmd),
            };

            GetPrivateHeaders(messageHandler, "GET", cmd, "");
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<OkxRestApiErrorMsg>(responseContent);
                     return Result.Fail<List<OrderQueryResponse>>(errorResponse?.Msg);
                  }

                  var data = JsonSerializer.Deserialize<OkxResponseData<OkxOpenOrdersResponse[]>>(responseContent,
                     _jsonSerializerOptions);
                  var openOrderList = new List<OrderQueryResponse>();
                  var orderResponse = data.Data;
                  foreach (var resp in orderResponse)
                  {
                     openOrderList.Add(resp.ToOrderQueryResponse());
                  }
                  return Result.OK(openOrderList);
               }
               return Result.Fail<List<OrderQueryResponse>>("No OrderResponse Data");
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
               _config.Url, "/api/v5/trade/orders-pending", e.Message);
            throw;
         }
      }

      public async Task<Result<List<ExchangeBalance>>> GetBalances()
      {
         try
         {
            var cmd = $"/api/v5/account/balance";
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Get,
               RequestUri = new Uri(_config.Url + cmd),
            };

            GetPrivateHeaders(messageHandler, "GET", cmd, "");
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<OkxRestApiErrorMsg>(responseContent);
                     return Result.Fail<List<ExchangeBalance>>(errorResponse?.Msg);
                  }

                  var data = JsonSerializer.Deserialize<OkxResponseData<OkxTopAccountPush[]>>(responseContent,
                     _jsonSerializerOptions);
                  var balanceList = new List<ExchangeBalance>();
                  var bals = data.Data[0];
                  
                  balanceList.AddRange(bals.Clone());
                 
                  return Result.OK(balanceList);
               }
               return Result.Fail<List<ExchangeBalance>>("No Get Balance Data");
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
               _config.Url, "/api/v5/account/balance", e.Message);
            throw;
         }
      }

      public Task<Result<CancelAllCancelledOrderIds>> CancelAllOrders(string symbol)
      {
         throw new NotImplementedException();
      }

      private static bool IsResponseAnError(string response)
      {
         string strRegex = $".*?code.*?:.*{OK_Response}.*?";

         var regex = new Regex(strRegex);

         if (regex.IsMatch(response))
            return false;
         return true;
      }

      
   }
}
