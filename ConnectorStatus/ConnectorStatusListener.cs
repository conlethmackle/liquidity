using System.Text.Json;
using Common.Messages;
using Common.Models;
using DataStore;
using Microsoft.Extensions.Logging;
using StrategyMessageListener;
using System.Timers;
using Common;
using MessageBroker;
using Timer = System.Timers.Timer;


namespace ConnectorStatus
{
   enum PingState
   {
      NOT_MONITORING_MISSED_PINGS = 1,
      MONITORING_MISSED_PINGS =2,
      PINGS_MISSED = 3
   }

   internal class PingTimerHandler : IDisposable
   {
      Timer _timer;
      Task _timerTask;
      CancellationTokenSource _cts;
      private readonly string _venue;
      private int _missedPingsCounter;
      private bool _inMissedState = false;
      private bool _isDisposed = false;

      private readonly ConnectorStatusListener _connectorStatusListener;

      PingState _pingState = PingState.NOT_MONITORING_MISSED_PINGS;
      
      public PingTimerHandler(ConnectorStatusListener connectorStatusListener, string venue)
      {        
         _venue = venue;
         _connectorStatusListener = connectorStatusListener;

         _timer = new Timer();
         _timer.Enabled = false;
         _timer.Elapsed +=  PingTimerExpired;
         _timer.Interval = 3000; // TODO - make configurable
      }

      public void Cancel()
      {
         if (!_isDisposed)
            _timer.Stop();
      }

      public void Restart()
      {
         _pingState = PingState.NOT_MONITORING_MISSED_PINGS;
         if (!_isDisposed)
            _timer.Start();         
      }

      public void CheckForMissedPings()
      {
         if (_pingState == PingState.MONITORING_MISSED_PINGS)
         {
            _pingState = PingState.NOT_MONITORING_MISSED_PINGS;
         }
      }

      private void PingTimerExpired(object sender, ElapsedEventArgs e)
      {
         if (!_isDisposed)
         {
            _timer.Enabled = false;
            if (_pingState == PingState.NOT_MONITORING_MISSED_PINGS)
            {
               _pingState = PingState.MONITORING_MISSED_PINGS;
               _timer.Enabled = true;
            }
            else if (_pingState == PingState.MONITORING_MISSED_PINGS)
            {

               _missedPingsCounter++;
               if (_missedPingsCounter >= 3) // TODO configure
               {
                  _connectorStatusListener.NotifyVenueFailure(_venue);
                  _missedPingsCounter = 0;
                  //_pingState= PingState.PINGS_MISSED;

               }
               else
                  _timer.Enabled = true;
            }
         }
      }

        public void Dispose()
        {

           _timer.Dispose();
           _isDisposed = true;
        }
    }

   public interface IConnectorStatusListener 
   {
      event Action<string> OnConnectorIsUp;
      event Action<string> OnConnectorIsDown;
      event Action<string> OnPrivateConnectivityIssue;
      event Action<string> OnPrivateConnectivityIssueCleared;
      public event Action<string> OnPublicConnectorUp;
      public event Action<string> OnPublicConnectorDown;

      Task Init(string configName);
      Task Init();
      void NotifyVenueFailure(string venue);
      bool GetConnectorStatus(string venue);
      List<ConnectorStatusMsg> GetAllConnectorStatuses();
      bool GetPublicConnectorStatus(string venue);
      void SendPublicStatusEnquiry(string venueName);
   }
   
   public class ConnectorStatusListener : IConnectorStatusListener
   {
      public event Action<string> OnConnectorIsUp;
      public event Action<string> OnConnectorIsDown;
      public event Action<string> OnPublicConnectorUp;
      public event Action<string> OnPublicConnectorDown;
      public event Action<string> OnPrivateConnectivityIssue;
      public event Action<string> OnPrivateConnectivityIssueCleared;
      private readonly ILogger<ConnectorStatusListener> _logger;
      private readonly IMessageReceiver _messageReceiver;
      private readonly IMessageBroker _messageBroker;
      private readonly IPortfolioRepository _portfolioRepository;
      private Timer _timer = new Timer();

      protected readonly string _portfolioName;
      protected string _configName;
      private Dictionary<string, PingTimerHandler> _aliveStatuses = new Dictionary<string, PingTimerHandler>();
      private Dictionary<string, ConnectorStatusMsg> _connectorStatuses = new Dictionary<string, ConnectorStatusMsg>();
      private bool IsInitialised = false;
      private object _locker = new object();
      private List<string> _venueList = new();
      public ConnectorStatusListener(StrategyStartConfig startupConfig,
                                     IPortfolioRepository repository,
                                     IMessageReceiver messageReceiver,
                                     IMessageBroker messageBroker,
                                     ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<ConnectorStatusListener>();
         _portfolioName = startupConfig.Account;
         _configName = startupConfig.ConfigName;
         _portfolioRepository = repository;
         _messageReceiver = messageReceiver;
         _messageReceiver.OnConnectorPingStatus += PingReceived;
         _messageReceiver.OnConnectorPublicStatus += ConnectorPublicStatusUpdate;
         _messageReceiver.OnConnectorPrivateStatus += ConnectorPrivateStatusUpdate;
         _messageBroker = messageBroker;
      }

      public async Task Init(string configName)
      {
         _configName = configName;
         await Init();
      }

      public async Task Init()
      {
         try
         {
            _timer.Interval = 10000;
            _timer.Elapsed += OnSendPublicStatus;
            _timer.Start();

            ClearPreviousStatuses();
            var configEntry = await _portfolioRepository.GetStrategyExchangeConfigEntry(_configName);
            foreach (var exchange in configEntry.ExchangeDetails)
            {
               var pingHandler = new PingTimerHandler(this, exchange.Venue.VenueName);
               if (!_aliveStatuses.ContainsKey(exchange.Venue.VenueName))
               {
                  _aliveStatuses.Add(exchange.Venue.VenueName, pingHandler);
               }
               else
               {
                  _aliveStatuses[exchange.Venue.VenueName] = pingHandler;
               }

               /*var connectorStatus = new ConnectorStatusMsg()
               {
                  Venue = exchange.Venue.VenueName,
                  IsConnected = false,
                  ErrorMsg = ""
               };
               if (!_connectorStatuses.ContainsKey(exchange.Venue.VenueName))
                  _connectorStatuses.Add(exchange.Venue.VenueName, connectorStatus);
                  */
               _venueList.Add(exchange.Venue.VenueName);
               //SendPublicStatusEnquiry(exchange.Venue.VenueName);
            }

            IsInitialised = true;
         }
         catch (Exception e)
         {
            _logger.LogError("Caught Error {Error}", e.Message);
         }
      }

      private void OnSendPublicStatus(object sender, ElapsedEventArgs e)
      {
         _timer.Stop();
         _venueList.ForEach(v =>
         {
            SendPublicStatusEnquiry(v);
         });
      }

      private void ConnectorPublicStatusUpdate(string venue, bool status, string msg)
      {
         lock (_locker)
         {
            // Might need to break this up 
            if (_connectorStatuses.ContainsKey(venue))
            {
               var connectorStatus = _connectorStatuses[venue];
               if (!connectorStatus.Public.IsConnected)
               {
                  _connectorStatuses[venue].Public.IsConnected = true;
                  OnConnectorIsUp?.Invoke(venue);
               }
               else
               {
                  _connectorStatuses[venue].Public.IsConnected = false;
                  OnConnectorIsDown?.Invoke(venue);
               }
            }
            else
            {
               var connectorStatus = new ConnectorStatusMsg();
               connectorStatus.Public.Venue = venue;
               connectorStatus.Public.IsConnected = status;
               connectorStatus.Public.ErrorMsg = msg;
               _connectorStatuses.Add(venue, connectorStatus);
               if (status)
                  OnConnectorIsUp?.Invoke(venue);
               else
                  OnConnectorIsDown?.Invoke(venue);
            }
         }
      }

      private void ConnectorPrivateStatusUpdate(string venue, bool status, string msg)
      {
         lock (_locker)
         {
            // Might need to break this up 
            if (_connectorStatuses.ContainsKey(venue))
            {
               var connectorStatus = _connectorStatuses[venue];
               if (!connectorStatus.Private.IsConnected)
               {
                  _connectorStatuses[venue].Private.IsConnected = true;
                  if (connectorStatus.Public.IsConnected)
                     OnPrivateConnectivityIssueCleared?.Invoke(venue);
               }
               else
               {
                  if (!status)
                  {
                     _connectorStatuses[venue].Private.IsConnected = false;
                     OnPrivateConnectivityIssue?.Invoke(venue);
                  }
               }
            }
            else
            {
               var connectorStatus = new ConnectorStatusMsg();
               connectorStatus.Private.Venue = venue;
               connectorStatus.Private.IsConnected = status;
               connectorStatus.Private.ErrorMsg = msg;
               _connectorStatuses.Add(venue, connectorStatus);
               if (status)
                  OnPrivateConnectivityIssueCleared?.Invoke(venue);
               else
                  OnPrivateConnectivityIssue?.Invoke(venue);
            }
         }
      }

      private void PingReceived(string venue, bool status= true)
      {
         if (!IsInitialised) return;
         // Cancel the timer for this venue
         lock (_locker)
         {
            if (_aliveStatuses.ContainsKey(venue))
            {
               var handler = _aliveStatuses[venue];
               handler.Cancel();
               handler.Restart();
               handler.CheckForMissedPings();

               if (_connectorStatuses.ContainsKey(venue))
               {
                  var connectorStatus = _connectorStatuses[venue];

                //  _logger.LogInformation(" ******************** Ping received for {Venue} - {ConnectorStatus}", venue,
                 //    connectorStatus.IsConnected);
                  if (!connectorStatus.Public.IsConnected)
                  {
                     _connectorStatuses[venue].Public.IsConnected = true;
                     //handler.Restart();
                     // A connector is back up - so should
                     // definitely send some sort of notification
                   
                     OnConnectorIsUp?.Invoke(venue);
                  }
               }
            }
            else
            {
               var pingHandler = new PingTimerHandler(this, venue);
               _aliveStatuses.Add(venue, pingHandler);
               // _logger.LogError("Can't find {Venue} in alive Status table", venue);
               //throw new Exception($"Can't find {venue} in alive Status table");
            }
         }
      }

      

      public void NotifyVenueFailure(string venue)
      {
         lock (_locker)
         {
            if (_connectorStatuses.ContainsKey(venue))
            {
               var connectorStatus = _connectorStatuses[venue];
               connectorStatus.Public.IsConnected = false;
               _logger.LogError("The {Venue} Connector not sending heartbeats - marked as offline", venue);
               OnConnectorIsDown?.Invoke(venue);
            }
         }

      }

      public bool GetConnectorStatus(string venue)
      {
         lock (_locker)
         {
            if (_connectorStatuses.ContainsKey(venue))
            {
               var publicStatus = _connectorStatuses[venue].Public.IsConnected;
               var privateStatus = _connectorStatuses[venue].Private.IsConnected;
               if (publicStatus && privateStatus) return true;
               return false;
            }

            _logger.LogError("{Venue} not found for GetConnectorStatus", venue); // Maybe should throw here
            return false;
         }
      }

      public bool GetPublicConnectorStatus(string venue)
      {
         lock (_locker)
         {
            if (_connectorStatuses.ContainsKey(venue))
            {
               var publicStatus = _connectorStatuses[venue].Public.IsConnected;
               
               if (publicStatus) return true;
               return false;
            }

            _logger.LogError("{Venue} not found for GetConnectorStatus", venue); // Maybe should throw here
            return false;
         }
      }

      public List<ConnectorStatusMsg> GetAllConnectorStatuses()
      {
         lock (_locker)
         {
            var all = _connectorStatuses.Values.ToList();
            return all;
         }
      }

      

      public void SendPublicStatusEnquiry(string venueName)
      {
         try
         {
            var msg = new MessageBusCommand()
            {
               CommandType = CommandTypesEnum.GET_PUBLIC_STATUS,
               Exchange = venueName,
               Data = venueName
            };
            _logger.LogInformation("Sending out a GET_PUBLIC_STATUS to {Venue}", venueName);
            var bytesRef = MessageBusCommand.ProtoSerialize(msg);
            _messageBroker.PublishToSubject(venueName, bytesRef);
         }
         catch (Exception e)
         {
           _logger.LogError(e, "Error in SendPublicStatusEnquiry - {Error}", e.Message);
         }
      }

      private void ClearPreviousStatuses()
      {
         if (_aliveStatuses.Count > 0)
         {
            foreach (var x in _aliveStatuses)
            {
               var pingHandler = x.Value;
               pingHandler.Dispose();
            }
            _aliveStatuses.Clear();
         }
      }
   }
}
