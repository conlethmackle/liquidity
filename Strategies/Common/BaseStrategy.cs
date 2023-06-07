using AccountBalanceManager;
using ClientConnections;
using Common.Messages;
using Common.Models;
using DataStore;
using Microsoft.Extensions.Logging;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Timers;
using System.Text.Json;
using MessageBroker;
using Common;
using ConnectorStatus;
using StrategyMessageListener;
using LastTradedPriceProcessing;
using FairValueProcessing;
using Common.Models.DTOs;
using Timer = System.Timers.Timer;
using DynamicConfigHandling;

namespace Strategies.Common
{
   public abstract class BaseStrategy : IStrategy
   {
      protected readonly ILogger<BaseStrategy> _logger;
      protected readonly IInventoryManager _inventoryManager;
      protected readonly IOrderAndTradeProcessing _orderAndTradeProcessing;
      protected readonly IOrderBookProcessor _orderBookProcessor;
      protected readonly IFairValuePricing _fairValuePricing;
      protected readonly IPortfolioRepository _portfolioRepository;
      protected readonly IPrivateClientConnections _privateClientConnections;
      protected readonly IPublicClientConnections _publicClientConnections;
      protected readonly IMessageBroker _messageBroker;
      protected readonly IMessageReceiver _messageReceiver;
      protected readonly IConnectorStatusListener _connectorStatusListener;
      protected readonly IDynamicConfigUpdater _dynamicConfigUpdater;
      protected readonly string _portfolioName;
      protected readonly string _configName;
      protected int _instanceId { get; set; }
      private Timer _strategyInitTimer = new Timer();
      protected StrategyExchangeConfigDTO _exchangeConfig { get; set; }
      protected ConcurrentDictionary<string, ExchangeDetailsDTO > _exchangeDetailsTable = new ConcurrentDictionary<string, ExchangeDetailsDTO>();
      protected ConcurrentDictionary<string, bool> _loginStatusTable = new ConcurrentDictionary<string, bool>();
      protected Timer _snapshotTimer = new Timer();
      protected Dictionary<string, Tuple<string, bool>> _snapshotDetailsTable = new ();

      public abstract Task StrategyInit();

      public BaseStrategy(ILoggerFactory loggerFactory, 
                          IInventoryManager inventoryManager, 
                          IOrderAndTradeProcessing orderTradeProcessing, 
                          IOrderBookProcessor orderBookProcessor,
                          IFairValuePricing fairValuePricing,
                          IPortfolioRepository repository,
                          IPrivateClientConnections privateClientConnections,
                          IPublicClientConnections publicClientConnections,
                          IMessageBroker messageBroker,
                          IMessageReceiver messageReceiver,
                          IConnectorStatusListener connectorStatusListener,
                          StrategyStartConfig startupConfig,
                          IDynamicConfigUpdater dynamicConfigUpdater)
      {
         _logger = loggerFactory.CreateLogger<BaseStrategy>();
         _inventoryManager = inventoryManager;
         _orderAndTradeProcessing = orderTradeProcessing;
         _orderBookProcessor = orderBookProcessor;
         _fairValuePricing = fairValuePricing;
         _portfolioRepository = repository;
         _privateClientConnections = privateClientConnections;
         _publicClientConnections = publicClientConnections;
         _messageBroker = messageBroker;
         _messageReceiver = messageReceiver;
         _connectorStatusListener = connectorStatusListener;
         _connectorStatusListener.OnConnectorIsUp += OnConnectorIsUp;
         _connectorStatusListener.OnConnectorIsDown += OnConnectorIsDown;
         _connectorStatusListener.OnPrivateConnectivityIssue += OnPrivateConnectivityIssue;
         _connectorStatusListener.OnPrivateConnectivityIssueCleared += OnPrivateConnectivityIssueCleared;
         _portfolioName = startupConfig.Account;
         _configName = startupConfig.ConfigName;
         _dynamicConfigUpdater = dynamicConfigUpdater;
         _orderBookProcessor.OnSnapshotReceived += OnSnapshotReceived;
         _orderAndTradeProcessing.Init();

         _snapshotTimer.Elapsed += OnSnapShotTimerExpired;
         _snapshotTimer.Interval = 5000;
         _snapshotTimer.Start();
      }

      private void OnSnapShotTimerExpired(object sender, ElapsedEventArgs e)
      {
         _snapshotTimer.Stop();
         if (_snapshotDetailsTable.Count > 0)
         {
            foreach (var entry in _snapshotDetailsTable)
            {
               var exchange = _exchangeDetailsTable[entry.Key];
               GetOrderBooks(exchange);
            }
            _snapshotTimer.Start();
         }
      }

      private void OnSnapshotReceived(string venue, string symbol)
      {
         if (_snapshotDetailsTable.ContainsKey(venue))
         {
            _snapshotDetailsTable.Remove(venue);
         }
      }

      private void OnPrivateConnectivityIssueCleared(string venue)
      {
         _logger.LogInformation("***************** Connector {Venue} is up - private side **********************************", venue);
         ConnectorUp(venue);
      }

      private void OnPrivateConnectivityIssue(string venue)
      {
         _logger.LogError("***************** Connector {Venue} is down - private side **********************************", venue);
         ConnectorDown(venue);
      }

      private void OnConnectorIsDown(string venue)
      {
         _logger.LogError("***************** Connector {Venue} is down - public side **********************************", venue);
        ConnectorDown(venue);
      }

      protected void OnConnectorIsUp(string venue)
      {
         _logger.LogInformation("***************** Connector {Venue} - public is up **********************************", venue);
         ConnectorUp(venue);
      }

      private void ConnectorUp(string venue)
      {
  
        if (_exchangeDetailsTable.ContainsKey(venue))
        {
           var exchange = _exchangeDetailsTable[venue];
           if (_loginStatusTable.ContainsKey(venue))
           {
              if (!_loginStatusTable[venue])
              {
                 LoginToVenue(exchange);
                 GetOrderBooks(exchange);
                 GetLatestTrade(exchange);
              }
           }
           else
           {
              LoginToVenue(exchange);
              GetOrderBooks(exchange);
              GetLatestTrade(exchange);
           }
        }
        _orderAndTradeProcessing.ConnectorStatusChange(venue, true);
      }

      private void ConnectorDown(string venue)
      {
         _orderAndTradeProcessing.ConnectorStatusChange(venue, false);
         if (_loginStatusTable.ContainsKey(venue))
         {
            _loginStatusTable[venue] = false;
         }
      }

      public async  Task Initialise()
      {

         try
         {


            _exchangeConfig = await _portfolioRepository.GetStrategyExchangeConfigEntry(_configName);
            if (_exchangeConfig == null)
            {
               _logger.LogCritical("Fatal Error: No Strategy Instance called {StrategyInstance}", _configName);
               throw new Exception($"Fatal Error: No Strategy Instance called {_configName}");
            }

            _instanceId = _exchangeConfig.StrategySPSubscriptionConfigId;
            foreach (var exchange in _exchangeConfig.ExchangeDetails)
            {
               try
               {
                  _exchangeDetailsTable.TryAdd(exchange.Venue.VenueName, exchange);
                  // LoginToVenue(exchange);
                  // GetOrderBooks(exchange);                           
               }
               catch (Exception e)
               {
                  _logger.LogError(e, "Error in logon to connectors {Error}", e.Message);
               }

            }

            await _connectorStatusListener.Init();
            await StrategyInit();
         }
         catch (Exception e)
         {

         }
      }

      private void GetOrderBooks(ExchangeDetailsDTO exchange)
      {
         var coinPairs = exchange.CoinPairs.Split(",");
         foreach (var coinPair in coinPairs)
         {
            if (!_snapshotDetailsTable.ContainsKey(exchange.Venue.VenueName))
            {
               var status = new Tuple<string, bool>(coinPair, false);
               _snapshotDetailsTable.Add(exchange.Venue.VenueName, status);
            }
         }
         
         MessageBusCommand msgPublic = new MessageBusCommand()
         {
            AccountName = _portfolioName,
            InstanceName = _configName,
            CommandType = CommandTypesEnum.GET_ORDERBOOK,
            Data = exchange.CoinPairs
         };
         var bytesRef = MessageBusCommand.ProtoSerialize(msgPublic);
         _messageBroker.PublishToSubject(exchange.Venue.VenueName, bytesRef);
      }

      private void GetLatestTrade(ExchangeDetailsDTO exchange)
      {
         
         MessageBusCommand msgPublic = new MessageBusCommand()
         {
            AccountName = _portfolioName,
            InstanceName = _configName,
            CommandType = CommandTypesEnum.GET_LATEST_TRADES,
            Data = exchange.CoinPairs
         };
         var bytesRef = MessageBusCommand.ProtoSerialize(msgPublic);
         _messageBroker.PublishToSubject(exchange.Venue.VenueName, bytesRef);
      }

      private void LoginToVenue(ExchangeDetailsDTO exchange)
      {
         if (_connectorStatusListener.GetPublicConnectorStatus((exchange.Venue.VenueName)))
         {
            if (_loginStatusTable.ContainsKey(exchange.Venue.VenueName))
            {
               if (_loginStatusTable[exchange.Venue.VenueName])
                  return;
            }

            _logger.LogInformation("Logging on to {Venue}", exchange.Venue.VenueName);
            _loginStatusTable[exchange.Venue.VenueName] = false;
            MessageBusCommand msgPrivate = new MessageBusCommand()
            {
               AccountName = _portfolioName,
               InstanceName = _configName,
               CommandType = CommandTypesEnum.CONNECT_PRIVATE,
            };

            var apiKeys = exchange.ApiKey;
            PrivateConnectionLogon logon = new PrivateConnectionLogon()
            {
               SPName = _portfolioName,
               ConfigInstance = _configName,
               ApiKey = apiKeys.Key,
               PassPhrase = apiKeys.PassPhrase,
               Secret = apiKeys.Secret,
            };

            var data = JsonSerializer.Serialize(logon);
            msgPrivate.Data = data;
            var bytesRef = MessageBusCommand.ProtoSerialize(msgPrivate);
            _logger.LogInformation("Sending login message to {Venue}", exchange.Venue.VenueName);
            _messageBroker.PublishToSubject(exchange.Venue.VenueName, bytesRef);
         }
         else
         {
            _logger.LogWarning("Not logging on to {Venue} as it is not connected", exchange.Venue.VenueName);
         }
      }

      public virtual void OnCancelStrategyOrders()
      {
         throw new NotImplementedException();
      }
    
      public virtual void OnCheckStaleOrdersPending()
      {
         throw new NotImplementedException();
      }

      public virtual void OnFairValueUpdate()
      {
         throw new NotImplementedException();
      }

      public virtual void OnOrderStatusUpdate()
      {
         throw new NotImplementedException();
      }
   }
}
