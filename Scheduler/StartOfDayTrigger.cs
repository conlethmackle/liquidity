using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Messages;
using MessageBroker;
using Microsoft.Extensions.Logging;
using Quartz;
using Common;

namespace Scheduler
{
   public interface IStartOfDayTrigger
   {

   }
   internal class StartOfDayTrigger : IStartOfDayTrigger, IJob
   {
      private readonly ILogger<StartOfDayTrigger> _logger;
      private readonly IMessageBroker _messageBroker;
      public StartOfDayTrigger(ILoggerFactory factory, IMessageBroker messageBroker)
      {
         _logger = factory.CreateLogger<StartOfDayTrigger>();
         _messageBroker = messageBroker;
      }


      public Task Execute(IJobExecutionContext context)
      {

         var response = new MessageBusReponse();
         response.OriginatingSource = "Scheduler";
         response.ResponseType = ResponseTypeEnums.START_OF_DAY;
         response.IsPrivate = false;
         var bytes = MessageBusCommand.ProtoSerialize(response);
         _messageBroker.PublishToSubject(Constants.SCHEDULE_UPDATE_TOPIC, bytes);
         return Task.CompletedTask;
      }
   }
}
