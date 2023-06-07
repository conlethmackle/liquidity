using Common;
using Common.Messages;
using MessageBroker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ProtoBuf;
using System.IO;
using Common.Models;
using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;
using OrderAndTradeProcessing;
using OrderBookProcessing;

namespace TestSender
{
   public interface IMessageReceiver
   {
      void Start();
      string GetOrderIdToCancel();
   }

   class MessageReceiver : IMessageReceiver
   {
      private readonly ILogger<MessageReceiver> _logger;
      private readonly IMessageBroker _messageBroker;
      private JsonSerializerOptions _jsonSerializerOptions { get; set; }
      private readonly IOrderAndTradeProcessing _orderProcessingModule;
      private readonly IOrderBookProcessor _orderBookProcessing;    
      private string _orderId { get; set; }

      public MessageReceiver(ILoggerFactory loggerFactory, 
                             IMessageBroker messageBroker, 
                             IOrderAndTradeProcessing orderProcessingModule,
                             IOrderBookProcessor orderBookProcessing
                            )
      {
         _logger = loggerFactory.CreateLogger<MessageReceiver>();
         _messageBroker = messageBroker;
         _jsonSerializerOptions = new JsonSerializerOptions()
         {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
         };
         _orderProcessingModule = orderProcessingModule;
         _orderBookProcessing = orderBookProcessing;       
      }
      public void Start()
      {
         _messageBroker.SubscribeToSubject(Constants.TESTER, ProcessCommands);
         _messageBroker.SubscribeToSubject(Constants.SINKS, ProcessCommands);
      }

      private void ProcessCommands(string subject, byte[] data)
      {
         using (var stream = new MemoryStream(data))
         {
            var response =  Serializer.Deserialize<MessageBusReponse>(stream);
            var venue = response.FromVenue;
            switch(response.ResponseType)
            {
               case ResponseTypeEnums.GET_BALANCE_RESPONSE:
                  var balances = JsonSerializer.Deserialize<ExchangeBalance[]>(response.Data, _jsonSerializerOptions);
                  break;
               case ResponseTypeEnums.BALANCE_UPDATE:
                  var balance = JsonSerializer.Deserialize<ExchangeBalance>(response.Data, _jsonSerializerOptions);
                  HandleBalanceUpdate(balance);
                  break;
               case ResponseTypeEnums.PLACE_ORDER_RESPONSE:
                  var orderIdData = JsonSerializer.Deserialize<OrderIdHolder>(response.Data);
                  
                  HandlePlaceOrderResponse(response.FromVenue, orderIdData);
                  break;
               case ResponseTypeEnums.NEW_ORDER:
               case ResponseTypeEnums.FILLED_ORDER:
               case ResponseTypeEnums.PARTIALLY_FILLED_ORDER:
               case ResponseTypeEnums.CANCELLED_ORDER:
               case ResponseTypeEnums.OWN_ORDER_UPDATE:
                  var orderUpdate = JsonSerializer.Deserialize<OwnOrderChange>(response.Data, _jsonSerializerOptions);
                  _orderProcessingModule.OrderUpdate(response.FromVenue, orderUpdate);                  
                  break;                      
               case ResponseTypeEnums.ORDERBOOK_UPDATE:
                  var orderbookUpdate = JsonSerializer.Deserialize<OrderBookChanged>(response.Data, _jsonSerializerOptions);
                  HandleOrderbookUpdate(venue, orderbookUpdate);
                  break;
               case ResponseTypeEnums.ORDERBOOK_SNAPSHOT:
                  var snapshot = JsonSerializer.Deserialize<OrderBookSnapshot>(response.Data, _jsonSerializerOptions);
                  HandleOrderBookSnapshot(venue, snapshot);
                  break;
               case ResponseTypeEnums.GET_OPEN_ORDERS_RESPONSE:
                  var openOrders = JsonSerializer.Deserialize<List<OrderQueryResponse>>(response.Data, _jsonSerializerOptions);
                  break;
               case ResponseTypeEnums.CANCEL_ORDERS_RESPONSE:
                  var cancelledOrders = JsonSerializer.Deserialize<SingleCancelledOrderId>(response.Data, _jsonSerializerOptions);
                  _orderProcessingModule.CancelOrderPlacementResponse(response.FromVenue, cancelledOrders);
                  break;
               case ResponseTypeEnums.REFERENCE_DATA_RESPONSE:
                  var tickerRefData = JsonSerializer.Deserialize<List<TickerReferenceData>>(response.Data, _jsonSerializerOptions);
                  break;
               case ResponseTypeEnums.TRADE_UPDATE:
                  var tradeData = JsonSerializer.Deserialize<TradeMsg>(response.Data, _jsonSerializerOptions);
                  _orderProcessingModule.TradeUpdate(tradeData);
                  break;
            }
         }
      }

      private void HandleBalanceUpdate(ExchangeBalance balance)
      {
        
      }

      private void HandleOrderBookSnapshot(string venue, OrderBookSnapshot snapshot)
      {
         _orderBookProcessing.SnapshotOrderBook(venue, snapshot.Symbol, snapshot);
      }

      private void HandleOrderbookUpdate(string venue, OrderBookChanged orderbookUpdate)
      {
         _orderBookProcessing.UpdateOrderBook(venue, orderbookUpdate.Symbol, orderbookUpdate);
      }

     

      private void HandlePlaceOrderResponse(string venue, OrderIdHolder orderData)
      {
         _orderProcessingModule.OrderPlacementUpdate(venue, orderData);
      }

      private void HandleGetBalanceResponse(ExchangeBalance[] balances)
      {
        
      }

      public string GetOrderIdToCancel()
      {
         return _orderId;
      }
   }
   
}
