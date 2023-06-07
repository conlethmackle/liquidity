using Microsoft.Extensions.Logging;
using Common.Messages;
using System.Collections.Concurrent;

namespace LastTradedPriceProcessing
{
   public interface ILastTradedPriceHandler
   {
      event Action<string, LatestTrade> OnLatestTradeUpdate;
      void UpdateLastTraded(string venue, LatestTrade price);
   }

   public class LastTradedPriceHandler : ILastTradedPriceHandler
   {
      public event Action<string, LatestTrade> OnLatestTradeUpdate;
      private ConcurrentDictionary<string, ConcurrentDictionary<string,LatestTrade>> _latestTradeMap = new ConcurrentDictionary<string, ConcurrentDictionary<string, LatestTrade>>();
      private readonly ILogger<LastTradedPriceHandler> _logger;

      public LastTradedPriceHandler(ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<LastTradedPriceHandler>();
      }

      public void UpdateLastTraded(string venue, LatestTrade priceData)
      {
         if (_latestTradeMap.ContainsKey(venue))
         {
            var symbolMap = _latestTradeMap[venue];
            if (symbolMap.ContainsKey(priceData.Symbol))
            {
               symbolMap[priceData.Symbol] = priceData;
            }
            else
            {
               symbolMap.TryAdd(priceData.Symbol, priceData);
            }
         }
         else
         {
            var symbolMap = new ConcurrentDictionary<string,LatestTrade>();
          //  if (symbolMap.ContainsKey(priceData.Symbol))
            symbolMap.TryAdd(priceData.Symbol, priceData);
            _latestTradeMap.TryAdd(venue, symbolMap);
         }
         OnLatestTradeUpdate?.Invoke(venue, priceData);
      }
   }
}
