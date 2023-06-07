using Common.Messages;
using Common.Models;
using Common.Models.DTOs;
using DataStore;
using MessageBroker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountBalanceManager
{
   public interface IInventoryManager
   {
      event Action<string, string, ExchangeBalance> OnOpeningBalance;
      event Func<string, string, ExchangeBalance, Task> OnOpeningBalanceForUI;
      event Action<string, string, ExchangeBalance> OnBalanceUpdate;
      event Func<string, string, ExchangeBalance, Task> OnBalanceUpdateForUI;
      event Action<string, string, int, ExchangeBalance> OnBalanceUpdateForTelegram;
      void Update(string venue, ExchangeBalance[] exchangeBalances);
      void UpdateForTelegram(string venue, ExchangeBalance[] exchangeBalances, int jobNo);
      void Update(string venue, ExchangeBalance exchangeBalance);
      void UpdateForTelegram(string venue, ExchangeBalance exchangeBalance, int jobNo);

      ExchangeBalance GetExchangeBalanceForCoin(string venue, string coin);
      List<ExchangeBalance> GetExchangeBalances(string venue);
      void GetOpeningBalancesDirect(string venue);
      void GetOpeningBalancesDirect(string venue, int jobNo);
      void GetOpeningBalancesDirect(string venue, string instanceName, string portfolioName);
      void InitCoins(string symbol);
      void InitConfig(string spName, string instanceName);
   }

   public class InventoryManager : IInventoryManager
   {
      public event Action<string, string, ExchangeBalance> OnOpeningBalance;
      public event Action<string, string, ExchangeBalance> OnBalanceUpdate;
      public event Func<string, string, ExchangeBalance, Task> OnOpeningBalanceForUI;
      public event Func<string, string, ExchangeBalance, Task> OnBalanceUpdateForUI;
      public event Action<string, string, int, ExchangeBalance> OnBalanceUpdateForTelegram;
      private Dictionary<string, Dictionary<string, ExchangeBalance>> _inventory = new Dictionary<string, Dictionary<string, ExchangeBalance>>();
      private Dictionary<string, bool> _getBalancesRequestSent = new Dictionary<string, bool>();
      private readonly ILogger<InventoryManager> _logger;
      private readonly IPortfolioRepository _repository;
      private readonly IMessageBroker _messageBroker;
      private string _portfolioName { get; set; }
      private string _configName { get; set; }
      private List<string> _monitoredCoins = new List<string>();
      private object _lock = new object();

      public InventoryManager(IPortfolioRepository repository,
                              StrategyStartConfig startupConfig,
                              IMessageBroker messageBroker,
                              ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<InventoryManager>();
         _repository = repository;
         _messageBroker = messageBroker;
         _portfolioName = startupConfig.Account;
         _configName = startupConfig.ConfigName;
      }

      public void InitCoins(string symbol)
      {
         
         var coins = symbol.Split("/");
         if (coins.Length > 1)
         {
            foreach(var coin in coins)
               _monitoredCoins.Add(coin);
         }
         else
         {
            _logger.LogInformation("");
         }
      }

      public void GetOpeningBalancesDirect(string venue)
      {
         SetBalanceCheckForRequest(venue, true);
         MessageBusCommand balanceCmd = new MessageBusCommand()
         {
            AccountName = _portfolioName,
            InstanceName = _configName,
            CommandType = CommandTypesEnum.GET_ACCOUNT_BALANCE,
            Data = ""
         };
         var bytesRef = MessageBusCommand.ProtoSerialize(balanceCmd);
         _messageBroker.PublishToSubject(venue, bytesRef);
      }

      public void GetOpeningBalancesDirect(string venue, int jobNo)
      {
         SetBalanceCheckForRequest(venue, true);
         MessageBusCommand balanceCmd = new MessageBusCommand()
         {
            AccountName = _portfolioName,
            InstanceName = _configName,
            JobNo = jobNo,
            CommandType = CommandTypesEnum.GET_ACCOUNT_BALANCE,
            Data = ""
         };
         var bytesRef = MessageBusCommand.ProtoSerialize(balanceCmd);
         _messageBroker.PublishToSubject(venue, bytesRef);
      }

      private void SetBalanceCheckForRequest(string venue, bool status)
      {
         if (_getBalancesRequestSent.ContainsKey(venue))
         {
            _getBalancesRequestSent[venue] = status;
         }
         else
         {
            _getBalancesRequestSent.Add(venue, status);
         }
      }

      private bool CheckForBalanceRequest(string venue)
      {
         return true;
         // if (_getBalancesRequestSent.ContainsKey(venue))
         //    return _getBalancesRequestSent[venue];
         //  else
         //     return false;
      }

      public void GetOpeningBalancesDirect(string venue, string instanceName, string portfolioName)
      {
         _portfolioName = portfolioName;
         SetBalanceCheckForRequest(venue, true);
         MessageBusCommand balanceCmd = new MessageBusCommand()
         {
            AccountName = _portfolioName,
            InstanceName = instanceName,
            CommandType = CommandTypesEnum.GET_ACCOUNT_BALANCE,
            Data = ""
         };
         var bytesRef = MessageBusCommand.ProtoSerialize(balanceCmd);
         _messageBroker.PublishToSubject(venue, bytesRef);
      }

      public void Update(string venue, ExchangeBalance[] exchangeBalances)
      {
         if (CheckForBalanceRequest(venue))
         {
            foreach (var exchangeBalance in exchangeBalances)
            {
               if (_monitoredCoins.Contains(exchangeBalance.Currency))
               {
                  Update(venue, exchangeBalance);
                  OnOpeningBalance?.Invoke(venue, exchangeBalance.Currency, exchangeBalance);
                  OnOpeningBalanceForUI?.Invoke(venue, exchangeBalance.Currency, exchangeBalance);
               }
            }
            SetBalanceCheckForRequest(venue, false);
         }
      }

      public void UpdateForTelegram(string venue, ExchangeBalance[] exchangeBalances, int jobNo)
      {
         if (CheckForBalanceRequest(venue))
         {
            foreach (var exchangeBalance in exchangeBalances)
            {
               if (_monitoredCoins.Contains(exchangeBalance.Currency))
               {
                  Update(venue, exchangeBalance);
                  OnBalanceUpdateForTelegram?.Invoke(venue, exchangeBalance.Currency, jobNo, exchangeBalance);
               }
            }
            SetBalanceCheckForRequest(venue, false);
         }
      }

      public void Update(string venue, ExchangeBalance exchangeBalance)
      {
         // TODO - Consider recording each transaction in a ledger as well
         lock (_lock)
         {
            if (_inventory.ContainsKey(venue))
            {
               var venueEntry = _inventory[venue];
               if (venueEntry.ContainsKey(exchangeBalance.Currency))
               {
                  var currencyEntry = venueEntry[exchangeBalance.Currency];
                  currencyEntry = exchangeBalance;
               }
               else
               {
                  venueEntry.Add(exchangeBalance.Currency, exchangeBalance);
               }
            }
            else
            {
               var venueEntry = new Dictionary<string, ExchangeBalance>();
               venueEntry.Add(exchangeBalance.Currency, exchangeBalance);
               _inventory[venue] = venueEntry;

            }
         }

         OnBalanceUpdate?.Invoke(venue, exchangeBalance.Currency, exchangeBalance);
         OnBalanceUpdateForUI?.Invoke(venue, exchangeBalance.Currency, exchangeBalance);
      }

      public void UpdateForTelegram(string venue, ExchangeBalance exchangeBalance, int jobNo)
      {
         // TODO - Consider recording each transaction in a ledger as well
         lock (_lock)
         {
            if (_inventory.ContainsKey(venue))
            {
               var venueEntry = _inventory[venue];
               if (venueEntry.ContainsKey(exchangeBalance.Currency))
               {
                  var currencyEntry = venueEntry[exchangeBalance.Currency];
                  currencyEntry = exchangeBalance;
               }
               else
               {
                  venueEntry.Add(exchangeBalance.Currency, exchangeBalance);
               }
            }
            else
            {
               var venueEntry = new Dictionary<string, ExchangeBalance>();
               venueEntry.Add(exchangeBalance.Currency, exchangeBalance);
               _inventory[venue] = venueEntry;
            }
         }

         OnBalanceUpdateForTelegram?.Invoke(venue, exchangeBalance.Currency, jobNo, exchangeBalance);
      }

      public ExchangeBalance GetExchangeBalanceForCoin(string venue, string coin)
      {
         lock (_lock)
         {
            if (_inventory.ContainsKey(venue))
            {
               var venueEntry = _inventory[venue];
               if (venueEntry.ContainsKey(coin))
               {
                  return venueEntry[coin];
               }

               _logger.LogError("No such coin {Coin} held at {Venue} from", coin, venue);
               return null;
            }
            else
            {
               _logger.LogError("No such coin {Coin} held at {Venue} from", coin, venue);
               return null;
            }
         }
      }

      public List<ExchangeBalance> GetExchangeBalances(string venue)
      {
         lock (_lock)
         {
            if (_inventory.ContainsKey(venue))
            {
               var venueEntry = _inventory[venue];
               if (venueEntry.Count() > 0)
                  return venueEntry.Values.ToList();
               return new List<ExchangeBalance>();
            }
            else
            {
               _logger.LogError("No such {Venue} from", venue);
               return new List<ExchangeBalance>();
            }
         }
      }

      public void InitConfig(string spName, string instanceName)
      {
         _portfolioName = spName;
         _configName = instanceName;
      }
   }
}
