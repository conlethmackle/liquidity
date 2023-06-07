using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Messages;
using Common.Models.DTOs;
using DataStore;
using Microsoft.Extensions.Logging;
using TelegramAlertsServer.Services;
using Telegram.Bot;

namespace TelegramAlertsServer.Router
{
   public interface ITelegramAlertRouting
   {
      Task ProcessConnectionAlert(string venue, TelegramLiquidityAlert alert);
      Task ProcessLiquidityProgressAlert(string venue, TelegramLiquidityAlert alert);
      Task Init();
   }
   
   public class TelegramAlertRouting : ITelegramAlertRouting
   {
     
      private readonly ILogger<TelegramAlertRouting> _logger;
     
      private readonly IPortfolioRepository _repository;
      private List<TelegramChannelDTO> _channels = new List<TelegramChannelDTO>();
      private Dictionary<int, List<TelegramAlertToChannelDTO>> _alertsToChannels = new ();
      private Dictionary<int, List<TelegramSubscriberToChannelDTO>> _usersToChannels = new();
      private List<TelegramUserDTO> _telegramUsers = new ();
      private Dictionary<int, TelegramAlertBehaviourDTO> _alertBehaviours = new ();
      private TelegramUserDTO _telegramUser;
      private Dictionary<string, TelegramBotClient> _botClients = new ();
      private Dictionary<string, DateTime> _msgPublishTable = new();

      public TelegramAlertRouting(ILoggerFactory loggerFactory,  IPortfolioRepository repository)
      {
            _logger = loggerFactory.CreateLogger<TelegramAlertRouting>();
           
            _repository = repository;
      }

      public async Task Init()
      {
         try
         {
            var channels = await _repository.GetTelegramChannels();
            var alertsToChannels = await _repository.GetTelegramAlertsToChannels();
            var usersToChannels = await _repository.GetTelegramSubscriberToChannels();
            var alertBehaviours = await _repository.GetTelegramAlertBehaviours();
            _alertBehaviours = alertBehaviours.ToDictionary(x => x.TelegramAlert.AlertEnumId, x => x);
            _alertsToChannels = alertsToChannels.GroupBy(x => x.TelegramAlert.AlertEnumId)
               .ToDictionary(g => g.Key, g => g.ToList());
            _usersToChannels = usersToChannels.GroupBy(x => x.TelegramChannelId)
               .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var channel in channels)
            {
               var bot = new TelegramBotClient(channel.TokenId);
               _botClients.Add(channel.TokenId, bot);
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error in Init = {Error}", e.Message);
            throw;
         }
      }

      public async Task ProcessConnectionAlert(string venue, TelegramLiquidityAlert alert)
      {
         await SendMessage((int)alert.SpecificAlert, alert.Message);
      }

      public async Task ProcessLiquidityProgressAlert(string venue, TelegramLiquidityAlert alert)
      {
         await SendMessage((int)alert.SpecificAlert, alert.Message);
      }


      private async Task SendMessage(int alertType, string msg)
      {
         try
         {
            if (CheckIfAlertToBeSent(alertType, msg))
            {
               if (_alertsToChannels.ContainsKey(alertType))
               {
                  var channelsToSendOn = _alertsToChannels[alertType];
                  foreach (var channel in channelsToSendOn)
                  {
                     if (_botClients.ContainsKey(channel.TelegramChannel.TokenId))
                     {
                        var bot = _botClients[channel.TelegramChannel.TokenId];

                        // Get all the subscribers
                        if (_usersToChannels.ContainsKey(channel.TelegramChannelId))
                        {
                           var userList = _usersToChannels[channel.TelegramChannelId];
                           foreach (var user in userList)
                           {

                              await bot.SendTextMessageAsync(chatId: user.TelegramUser.UserToken,
                                 text: msg,
                                 cancellationToken: new CancellationTokenSource().Token);
                           }
                        }
                     }

                  }
               }
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error sending message to Telegram Bot {Error}", e.Message);
         }
      }

      private bool CheckIfAlertToBeSent(int alertType, string msg)
      {
         if (_msgPublishTable.ContainsKey(msg))
         {
            // Get the alert Id
            TelegramAlertBehaviourDTO behaviour = null;
            if (_alertBehaviours.ContainsKey(alertType))
            {
               behaviour = _alertBehaviours[alertType];
               switch (behaviour.TelegramAlertBehaviourType.EnumId)
               {
                  case (int)TelegramAlertBehaviorEnums.ALWAYS:
                     return true;
                  case (int)TelegramAlertBehaviorEnums.ONCE:
                     return false;
                  case (int)TelegramAlertBehaviorEnums.PERIODIC:
                     var timeSinceLast = _msgPublishTable[msg];
                     var timeNow = DateTime.UtcNow;
                     var diff = timeNow - timeSinceLast;
                     if (diff.TotalSeconds >= behaviour.TimeSpan)
                     {
                           _msgPublishTable[msg] = DateTime.UtcNow;
                           return true;
                     }
                     else
                        return false;
                  default:
                     _logger.LogWarning("Unrecognised Enum Id in CheckIfAlertToBeSent {EnumId}", behaviour.TelegramAlertBehaviourType.EnumId);
                     return false;
               }
            }
            else
            {
               _logger.LogError("The Alert with Id = {AlertId} has not been configured correctly");
               return true; // publishing it anyways
            }
         }
         else
         {
            _msgPublishTable.Add(msg, DateTime.UtcNow);
            return true;
         }
      }
   }
}
