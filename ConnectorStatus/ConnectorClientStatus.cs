using Common.Messages;
using MessageBroker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using ConnectorCommon;

namespace ConnectorStatus
{
   public interface IConnectorClientStatus
   {
      void UpdatePublicRestApiStatus(bool status, string venue, string? errorMsg = null);
      void UpdatePublicWebSocketStatus(bool status, string venue, string? errorMsg=null);
      void UpdatePrivateRestApiStatus(string subject, string account, string instanceName, bool status, string? errorMsg=null);
      void UpdatePrivateWebSocketStatus(string subject, string account, string instanceName, bool status, string? errorMsg);

      SingleClientConnectionStatus GetPrivateStatus(string instance);
      void GetPublicStatus(string venue);
      void SetExchange(string exchange);
   }

   public class ConnectorClientStatus : IConnectorClientStatus
   {
      public SingleClientConnectionStatus PublicClient { get; set; }
      public Dictionary<string, SingleClientConnectionStatus> PrivateClients { get; set; } = new Dictionary<string, SingleClientConnectionStatus>();
      private readonly ILogger<ConnectorClientStatus> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly IMessageBusProcessor _messageBusProcessor;
      private string _exchange { get; set; }

      public ConnectorClientStatus(ILoggerFactory loggerFactory, IMessageBroker messageBroker, IMessageBusProcessor messageBusProcessor)
      {
         _logger = loggerFactory.CreateLogger<ConnectorClientStatus>();
         _messageBroker = messageBroker;
         _messageBusProcessor = messageBusProcessor;
         _messageBusProcessor.OnGetPublicStatus += OnGetPublicStatus;
         PublicClient = new SingleClientConnectionStatus();
      }

      private void OnGetPublicStatus(string venue)
      {
         GetPublicStatus(venue);
      }

      public void SetExchange(string exchange)
      {
         _exchange = exchange;
      }

      public void UpdatePublicRestApiStatus(bool status, string venue, string? errorMsg)
      {
         if (PublicClient.RestApi != status)
         {
            PublicClient.RestApi = status;
            PublicClient.ErrorMsg = errorMsg;
            // Send an update
            SendPublicUpdate(status, venue, errorMsg);
         }
      }

      public void UpdatePublicWebSocketStatus(bool status, string venue, string? errorMsg)
      {
         if (PublicClient.WebSocket != status)
         {
            PublicClient.WebSocket = status;
            PublicClient.ErrorMsg = errorMsg;
            // Send an update
            SendPublicUpdate(status, venue, errorMsg);
         }
      }
     
      public void UpdatePrivateRestApiStatus(string subject, string account, string instanceName, bool status, string? errorMsg)
      {
         if (!PrivateClients.ContainsKey(instanceName))
         {
            var s = new SingleClientConnectionStatus()
            {
               RestApi = status,
               WebSocket = false,
               ErrorMsg = errorMsg
            };
            PrivateClients.Add(instanceName, s);
            if (!status)
               SendPrivateUpdate(subject, account, instanceName, false, errorMsg);
         }
         else
         {
            if (PrivateClients[instanceName].RestApi != status)
            {
               PrivateClients[instanceName].RestApi = status;
               PrivateClients[instanceName].ErrorMsg = errorMsg;
               if (status && PrivateClients[instanceName].WebSocket)
                  SendPrivateUpdate(subject, account, instanceName, true, errorMsg);
               else
                  SendPrivateUpdate(subject, account, instanceName, status, errorMsg);
            }
         }
      }

      public void UpdatePrivateWebSocketStatus(string subject, string account, string instanceName, bool status, string? errorMsg)
      {
         if (!PrivateClients.ContainsKey(instanceName))
         {
            var s = new SingleClientConnectionStatus()
            {
               RestApi = false,
               WebSocket = status,
               ErrorMsg = errorMsg
            };
            PrivateClients.Add(instanceName, s);
            if (!status)
               SendPrivateUpdate(subject, account, instanceName, false, errorMsg);
         }
         else
         {
            if (PrivateClients[instanceName].WebSocket != status)
            {
               PrivateClients[instanceName].RestApi = status;
               PrivateClients[instanceName].ErrorMsg = errorMsg;
               if (status && PrivateClients[instanceName].RestApi)
                  SendPrivateUpdate(subject, account, instanceName, true, errorMsg);
               else
                  SendPrivateUpdate(subject, account, instanceName, status, errorMsg);
            }

         }
      }

      private void SendPrivateUpdate(string subject, string account, string instanceName, bool status, string? errorMsg)
      {
         var statusMsg = new ConnectorStatusMsg();
         statusMsg.Private.IsConnected = status;
         statusMsg.Private.ErrorMsg = errorMsg;
         statusMsg.Private.Venue = _exchange;

         var msg = new MessageBusReponse()
         {
            ResponseType = ResponseTypeEnums.CONNECTOR_STATUS_UPDATE,
            AccountName = account,
            OriginatingSource = instanceName,
            FromVenue = _exchange,
            IsPrivate = true,
            Data = JsonSerializer.Serialize(statusMsg)
         };
         PublishHelper.PublishToTopic(subject, msg, _messageBroker);
      }

      private void SendPublicUpdate(bool status, string venue, string? errorMsg)
      {
         _logger.LogInformation("Sending out a CONNECTOR_STATUS_UPDATE");
         var statusMsg = new ConnectorStatusMsg();
         statusMsg.Public.IsConnected = status;
         statusMsg.Public.ErrorMsg = errorMsg;
         statusMsg.Public.Venue = venue;

         var msg = new MessageBusReponse()
         {
            ResponseType = ResponseTypeEnums.CONNECTOR_STATUS_UPDATE,
            AccountName = "",
            FromVenue = venue,
            IsPrivate = false,
            Data = JsonSerializer.Serialize(statusMsg)
         };
         var bytesRef = MessageBusCommand.ProtoSerialize(msg);
         _messageBroker.PublishToSubject(Constants.CONNECTOR_PUBLIC_CONNECTION_TOPIC, bytesRef);
      }

      public SingleClientConnectionStatus GetPrivateStatus(string instance)
      {
         if (PrivateClients.ContainsKey(instance))
         {
            return PrivateClients[instance];
         }
         _logger.LogWarning("ConnectorClientStatus - GetStatus - no entry for {Instance}", instance);
         return null;
      }

      public void GetPublicStatus(string venue)
      {
         
         if (PublicClient.RestApi && PublicClient.WebSocket)
         {
            SendPublicUpdate(true, venue, $"All Good With Public {venue}");
         }
         else
         {
            if (!PublicClient.WebSocket && !PublicClient.RestApi)
            {
               SendPublicUpdate(false, venue,$"Error with {venue} Websocket and RestApi");
            }
            else if (PublicClient.WebSocket && !PublicClient.RestApi)
            {
               SendPublicUpdate(false, venue,$"Error with {venue}  RestApi");
            }
            else if (!PublicClient.WebSocket && PublicClient.RestApi)
            {
               SendPublicUpdate(false, venue,$"Error with {venue} Websocket");
            }
            else
            {
               SendPublicUpdate(false, venue,$"Unspecified Error with {venue} Websocket and RestApi");
            }
         }
      }
   }
}
