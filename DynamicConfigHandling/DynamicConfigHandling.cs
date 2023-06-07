using Common.Messages;
using DataStore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DynamicConfigHandling
{
   
   public interface IDynamicConfigUpdater
   {
      event Func<StrategyConfigChangeData?, Task> OnStrategyConfigChange;
      Task UpdateConfig(ConfigChangeUpdate configChange);
   }

   public class DynamicConfigUpdater : IDynamicConfigUpdater
   {
      public event Func<StrategyConfigChangeData?, Task> OnStrategyConfigChange;
      private readonly ILogger<DynamicConfigUpdater> _logger;


        public DynamicConfigUpdater(ILoggerFactory loggerFactory)
        {
         _logger = loggerFactory.CreateLogger<DynamicConfigUpdater>();
       
      }
      public async Task UpdateConfig(ConfigChangeUpdate configChange)
      {
         switch(configChange.ConfigChangeType)
         {
            case ConfigChangeType.STRATEGY:
               var data = JsonSerializer.Deserialize<StrategyConfigChangeData>(configChange.Data);
               await OnStrategyConfigChange?.Invoke(data);
               break;
         }
      }
   }
}