using SyncfusionLiquidity.Server.Receiver;
using SyncfusionLiquidity.Shared;
using Common;
using Common.Messages;
using DataStore;
using MessageBroker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MultipleStrategyManager;
using System.Text.Json;

namespace BlazorLiquidity.Server.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class StrategyProcessController : ControllerBase
   {
      private readonly ILogger<StrategyProcessController> _logger;     
      private readonly IMessageBroker _messageBroker;
    
      public StrategyProcessController(ILoggerFactory loggerFactory,                                        
                                       IMessageBroker messageBroker)
                               
      {
         _logger = loggerFactory.CreateLogger<StrategyProcessController>();       
        _messageBroker = messageBroker;        
      }

      [HttpPost]
      [Route("strategyprocess/command")]
      public async Task<ActionResult> SendStrategyProcessCommand(StrategyProcessDetails strategyDetails)
      {
         try
         {
            if (strategyDetails == null)
               return BadRequest();

            SendMessageToMultipleStrategyManager(strategyDetails);

            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating MQ Settings");
         }
      }

      private void SendMessageToMultipleStrategyManager(StrategyProcessDetails strategyDetails)
      {
         
         var msgPrivate = new MessageBusCommand()
         {
            
            AccountName = strategyDetails.AccountName,
            InstanceName = strategyDetails.ConfigName,
            CommandType = CommandTypesEnum.CONNECT_PRIVATE,
            IsPrivate = true,
            Exchange = Constants.MULTI_STRATEGY_MANAGER
         };

         if (strategyDetails.Enable)
         {
            msgPrivate.CommandType = CommandTypesEnum.START_STRATEGY;
            var data = new StartStrategyData()
            {
               StrategyConfigId = strategyDetails.StrategyConfigId
            };
            msgPrivate.Data = JsonSerializer.Serialize(data);
            PublishHelper.Publish(Constants.STRATEGY_CONTROL, msgPrivate, _messageBroker);
         }
         else
         {
            msgPrivate.CommandType = CommandTypesEnum.STOP_STRATEGY;
            var data = new StopStrategyData()
            {
               Method = StopType.BY_INSTANCE,
               InstanceName = strategyDetails.ConfigName
            };
            msgPrivate.Data = JsonSerializer.Serialize(data);
            PublishHelper.Publish(Constants.STRATEGY_CONTROL, msgPrivate, _messageBroker);
         }
      }
   }
}
