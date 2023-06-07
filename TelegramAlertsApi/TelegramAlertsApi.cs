using System.Text.Json;
using Common;
using Common.Messages;
using MessageBroker;
using Microsoft.Extensions.Logging;

namespace TelegramAlertsApi
{
   public interface ITelegramAlertsApi
   {
      void SendLiquidityAlert(ResponseTypeEnums alertCategory, 
                              LiquidityAlertTypes alertType, 
                              string account, 
                              string instance, 
                              string alertMsg, string venue);
   }

   public class TelegramAlertsApi : ITelegramAlertsApi
   {
      private readonly ILogger<TelegramAlertsApi> _logger;
      private readonly IMessageBroker _messageBroker;
      public TelegramAlertsApi(ILoggerFactory factory, IMessageBroker messageBroker)
      {
         _logger = factory.CreateLogger<TelegramAlertsApi>();
         _messageBroker = messageBroker;
      }

      public void SendLiquidityAlert(ResponseTypeEnums alertCategory, LiquidityAlertTypes alertType, string account, string instance, string alertMsg, string venue = "")
      {
         var alert = new TelegramLiquidityAlert()
         {
            SpecificAlert = alertType,
            Message = alertMsg,
            Venue = venue,
            Time = DateTime.UtcNow,
         };

         var response = new MessageBusReponse()
         {
            AccountName = account,
            OriginatingSource = instance,
            ResponseType = alertCategory,
            FromVenue = venue,
            Data = JsonSerializer.Serialize(alert)
         };
         PublishHelper.PublishToTopic(Constants.TELEGRAM_LIQUIDITY_ALERT, response, _messageBroker);
      }
   }
}