using DataStore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;

namespace ClientConnections
{
   public interface IPrivateClientConnections
   {

   }

   public class PrivateClientConnections : IPrivateClientConnections
   {
      private readonly ILogger<PrivateClientConnections> _logger;
      private readonly IPortfolioRepository _portfolioRepository;
      private readonly string _account;
      private readonly string _configName;
      public PrivateClientConnections(IPortfolioRepository repository, StrategyStartConfig startupConfig, ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<PrivateClientConnections>(); 
         _portfolioRepository = repository;
         _account = startupConfig.Account;
         _configName = startupConfig.ConfigName;
      }

      public async Task ConnectToExchanges()
      {
         //_portfolioRepository.
      }
   }
}
