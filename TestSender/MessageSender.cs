using Common;
using Common.Messages;
using Common.Models;
using MessageBroker;
using MessageBroker.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderAndTradeProcessing;
using System;
using System.Linq;
using System.Text.Json;

namespace TestSender
{
   public interface IMessageSender
   {
      void Run();
   }
   public class MessageSender : IMessageSender
   {
      private readonly ILogger<MessageSender> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly IMessageReceiver _messageReceiver;
      private readonly IOrderAndTradeProcessing _orderAndTradeProcessing;
      public MessageSender(ILoggerFactory loggerFactory, IMessageBroker messageBroker, IMessageReceiver messageReceiver, IOrderAndTradeProcessing orderAndTradeProcessing)
      {
         _logger = loggerFactory.CreateLogger<MessageSender>();
         _messageBroker = messageBroker;
         _messageReceiver = messageReceiver;
         _orderAndTradeProcessing = orderAndTradeProcessing;
      }

      public void Run()
      {
         _logger.LogInformation("In here");
         MessageBusCommand msg = new MessageBusCommand();
         msg.CommandType = CommandTypesEnum.PLACE_ORDER;
         msg.Exchange = Constants.KUCOIN;

         var order = new PlaceOrderCmd();
         order.Symbol = "BTC-USDT";
         order.ClientOrderId = RandomString(16);
         order.Price = 81300;
         order.Quantity = 0.001m;
         order.IsBuy = true;
         order.OrderType = OrderTypeEnum.LIMIT;
         
         order.TimeInForce = TimeInForceEnum.GTC;

         var data = JsonSerializer.Serialize(order);
         msg.Data = data;


         var msg2 = new MessageBusCommand()
         {
            CommandType = CommandTypesEnum.GET_ACCOUNT_BALANCE,
            Exchange = Constants.KUCOIN
         };

         var msg3 = new MessageBusCommand()
         {
            CommandType = CommandTypesEnum.GET_OPEN_ORDERS,
            Exchange = Constants.KUCOIN,
            Data = "BTC-USDT",
            AccountName = "",
            IsPrivate = true 
         };

         var msg_ref_data = new MessageBusCommand()
         {
            CommandType = CommandTypesEnum.GET_REFERENCE_DATA,
            Exchange = Constants.KUCOIN,
            Data = ""
         };

         try
         {
            var getOrderBookCmd = new MessageBusCommand()
            {
               CommandType = CommandTypesEnum.GET_ORDERBOOK,
               Exchange = Constants.KUCOIN,
               Data = "KCS-USDT",
               IsPrivate = false
            };
            var bytesRef = MessageBusCommand.ProtoSerialize(getOrderBookCmd);
            _messageBroker.PublishToSubject(Constants.KUCOIN, bytesRef);
            /*     for(;;)
                 {
                    var bytesRef = MessageBusCommand.ProtoSerialize(msg_ref_data);
                    _messageBroker.PublishToSubject(Constants.KUCOIN, bytesRef);

                    //  var bytes = MessageBusCommand.ProtoSerialize(msg);
                    //    _messageBroker.PublishToSubject(Constants.KUCOIN, bytes);
                    _orderAndTradeProcessing.PlaceOrder(Constants.KUCOIN, "BTC-USDT", 81300, 0.001m, TimeInForceEnum.GTC, true, OrderTypeEnum.LIMIT);

                    var bytes2 = MessageBusCommand.ProtoSerialize(msg2);
                   _messageBroker.PublishToSubject(Constants.KUCOIN, bytes2);
                    var bytes3 = MessageBusCommand.ProtoSerialize(msg3);
                    _messageBroker.PublishToSubject(Constants.KUCOIN, bytes3);
                    var cancelMsg = new MessageBusCommand()
                    {
                       CommandType = CommandTypesEnum.CANCEL_ORDER,
                       Exchange = Constants.KUCOIN,
                       Data = _messageReceiver.GetOrderIdToCancel()
                    };
                    var bytes4 = MessageBusCommand.ProtoSerialize(cancelMsg);
                  //  _messageBroker.PublishToSubject(Constants.KUCOIN, bytes4);
                    Console.ReadLine();
                 } */
         }
         catch(Exception e)
         {
            _logger.LogError(" error {Message}", e.Message);
         }
      }

      public static string RandomString(int length)
      {
         var random = new Random();
         const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
         return new string(Enumerable.Repeat(chars, length)
             .Select(s => s[random.Next(s.Length)]).ToArray());
      }
   }
}
 