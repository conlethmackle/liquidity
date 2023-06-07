using Common;
using Common.Messages;
using MessageBroker;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CentralCommand
{
   public interface ICommandReceiver
   {
      void Start();
   }

   internal class CommandReceiver : ICommandReceiver
   {
      private readonly ILogger<CommandReceiver> _logger;
      private readonly IMessageBroker _messageBroker;

      private string _orderId { get; set; }

      public CommandReceiver(ILoggerFactory loggerFactory,
                             IMessageBroker messageBroker)
      {
         _logger = loggerFactory.CreateLogger<CommandReceiver>();
         _messageBroker = messageBroker;
      }

      public void Start()
      {
         _messageBroker.SubscribeToSubject(Constants.CONNECTOR_COMMAND, ProcessCommands);         
      }

      private void ProcessCommands(string subject, byte[] data)
      {
         using (var stream = new MemoryStream(data))
         {
            var command = Serializer.Deserialize<MessageBusCommand>(stream);
            switch(command.CommandType)
            {
               case CommandTypesEnum.CONNECT_PRIVATE:
                  SendPrivateConnectMessage(command);
                  break;
               case CommandTypesEnum.CONNECT_PUBLIC:
                  SendPublicConnectMessage(command);
                  break;
               case CommandTypesEnum.DISCONNECT_PRIVATE:
                  SendPrivateDisconnectMessage(command);
                  break;
               case CommandTypesEnum.DISCONNECT_PUBLIC:
                  SendPublicDisconnectMessage(command);
                  break;
            }
         }
      }

      private void SendPublicDisconnectMessage(MessageBusCommand command)
      {
         _logger.LogInformation("Sending Public Disconnect to {Exchange}", command.Exchange);
         var bytesRef = MessageBusCommand.ProtoSerialize(command);
         _messageBroker.PublishToSubject(command.Exchange, bytesRef);
      }

      private void SendPrivateDisconnectMessage(MessageBusCommand command)
      {
         _logger.LogInformation("Sending Private Disconnect to {Exchange} and SP {Account}", command.Exchange, command.AccountName);
         var bytesRef = MessageBusCommand.ProtoSerialize(command);
         _messageBroker.PublishToSubject(command.Exchange, bytesRef);
      }

      private void SendPublicConnectMessage(MessageBusCommand command)
      {
         _logger.LogInformation("Sending Public Connect to {Exchange}", command.Exchange);
         var bytesRef = MessageBusCommand.ProtoSerialize(command);
         _messageBroker.PublishToSubject(command.Exchange, bytesRef);
      }

      private void SendPrivateConnectMessage(MessageBusCommand command)
      {
         _logger.LogInformation("Sending Private Connect to {Exchange} and SP {Account}", command.Exchange, command.AccountName);
         var bytesRef = MessageBusCommand.ProtoSerialize(command);
         _messageBroker.PublishToSubject(command.Exchange, bytesRef);
      }
   }
}
