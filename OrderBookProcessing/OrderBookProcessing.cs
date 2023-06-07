using Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookProcessing
{
   public interface IOrderBookProcessor
   {
      event Action<string, string, decimal, decimal> OnBestAskChange;
      event Action<string, string, decimal, decimal> OnBestBidChange;
      event Action<string, string> OnSnapshotReceived;

      void UpdateOrderBook(string venue, string symbol, OrderBookChanged orderBookUpdate);
      void SnapshotOrderBook(string venue, string symbol, OrderBookSnapshot snapshot);
   }

   public class OrderBookProcessor : IOrderBookProcessor
   {
      public event Action<string, string, decimal, decimal> OnBestAskChange;
      public event Action<string, string, decimal, decimal> OnBestBidChange;
      public event Action<string, string> OnSnapshotReceived;
      Dictionary<string, Dictionary<string, OrderBook>> _orderbook = new Dictionary<string, Dictionary<string, OrderBook>>();
      Dictionary<string, Dictionary<string, bool>> _orderbookSnapshotReceived = new Dictionary<string, Dictionary<string, bool>>();
      private object _orderBookLock = new object();
      private readonly ILogger<OrderBookProcessor> _logger;

      public OrderBookProcessor(ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<OrderBookProcessor>();
      }

      public void SnapshotOrderBook(string venue, string symbol, OrderBookSnapshot snapshot)
      {
         OrderBook orderbook = null;
         lock (_orderBookLock)
         {
            Console.WriteLine($"In Snapshot Orderbook *************************");
            Dictionary<string, OrderBook> venueOrderBook = null;
            if (!_orderbook.ContainsKey(venue))
            {
               venueOrderBook = new Dictionary<string, OrderBook>();
               _orderbook.Add(venue, venueOrderBook);
            }
            else
               venueOrderBook = _orderbook[venue];

            
            if (!venueOrderBook.ContainsKey(symbol))
            {
               orderbook = new OrderBook();
               venueOrderBook.Add(symbol, orderbook);
            }
            else
               orderbook = venueOrderBook[symbol];
            orderbook = snapshot.OrderBook;
            venueOrderBook[symbol] = orderbook;
            UpdateSnapshotReceived(venue, symbol);
            OnSnapshotReceived?.Invoke(venue, symbol);
         }
         if (orderbook.Ask.Any())
            OnBestAskChange?.Invoke(venue, symbol, orderbook.Ask[0].Price, orderbook.Ask[0].Quantity);
         if (orderbook.Bid.Any())
         {
            OnBestBidChange?.Invoke(venue, symbol, orderbook.Bid[0].Price, orderbook.Bid[0].Quantity);
         }
      }

      private void UpdateSnapshotReceived(string venue, string symbol)
      {
         if (_orderbookSnapshotReceived.ContainsKey(venue))
         {
            var venueMap = _orderbookSnapshotReceived[venue];
            if (venueMap.ContainsKey(symbol))
            {
               // Shouldn't really happen !!!!
               venueMap[symbol] = true;
               _logger.LogWarning("Looks like Orderbook snapshot for {Symbol} from {Venue} has happened more than once",
                                   symbol, venue);
            }
            else
            {
               venueMap = new Dictionary<string, bool>();
               venueMap.Add(symbol, true);
               _orderbookSnapshotReceived[venue] = venueMap;
            }
         }
         else
         {
            var venueMap = new Dictionary<string, bool>();
            venueMap.Add(symbol, true);
            _orderbookSnapshotReceived.Add(venue, venueMap);
         }
      }

      public void UpdateOrderBook(string venue, string symbol, OrderBookChanged orderBookUpdate)
      {
       //  _logger.LogInformation("************************ In UpdateOrderBook *********************************");
         if (!HasSnapShotBeenReceived(venue, symbol))
            return;
         
         Dictionary<string, OrderBook> venueOrderBook = null;
         if (!_orderbook.ContainsKey(venue))
         {
            venueOrderBook = new Dictionary<string, OrderBook>();
            _orderbook.Add(venue, venueOrderBook);
         }
         else
            venueOrderBook = _orderbook[venue];

         OrderBook orderbook = null;
         if (!venueOrderBook.ContainsKey(symbol))
         {
            orderbook = new OrderBook();
            venueOrderBook.Add(symbol, orderbook);
         }
         else
            orderbook = venueOrderBook[symbol];

         UpdateOrderBook(venue, symbol, orderBookUpdate.Data[0], orderbook.Bid, true);
         UpdateOrderBook(venue, symbol, orderBookUpdate.Data[1], orderbook.Ask, false);
      }

      private bool HasSnapShotBeenReceived(string venue, string symbol)
      {
         if (!_orderbookSnapshotReceived.ContainsKey(venue))
            return false;

         var venueMap = _orderbookSnapshotReceived[venue];
         if (!venueMap.ContainsKey(symbol))
            return false;
         return true;
      }

      private void UpdateOrderBook(string venue, string symbol, OrderBookSide bidOrAsks, List<Level> sideOfBook, bool IsBuy)
      {
         if (!HasSnapShotBeenReceived(venue, symbol))
            return;
         decimal currentBestBidPrice = 0;
         decimal currentBestAskPrice = 0;

         if (IsBuy)
         {
            if (sideOfBook.Any())
               currentBestBidPrice = sideOfBook[0].Price;
         }

         else
         {
            if (sideOfBook.Any())
               currentBestAskPrice = sideOfBook[0].Price;
         }

         // _logger.LogIn
         lock (_orderBookLock)
         {
            foreach (var removeLevel in bidOrAsks.Remove)
            {
               var l = new Level(removeLevel, 0.0m);
               var rem = sideOfBook.Where(x => x.Price == removeLevel).FirstOrDefault();
               sideOfBook.Remove(rem);
            }

            foreach (var addLevel in bidOrAsks.Add)
            {
               // Shouldn't happen
               var anyLevel = sideOfBook.Where(p => p.Price == addLevel.Price).ToList();
               if (anyLevel.Count > 0)
               {
                  var level = anyLevel.FirstOrDefault();
                  level.Quantity = addLevel.Quantity;
               }
               else
               {
                  var l = new Level(addLevel.Price, addLevel.Quantity);
                  sideOfBook.Add(l);
               }
            }

            foreach (var updateLevel in bidOrAsks.Update)
            {
               var anyLevel = sideOfBook.Where(p => p.Price == updateLevel.Price).ToList();
               if (anyLevel.Count > 0)
               {
                  var level = anyLevel.FirstOrDefault();
                  level.Quantity = updateLevel.Quantity;
               }
               else // Shouldn't happen
               {
                  var l = new Level(updateLevel.Price, updateLevel.Quantity);
                  sideOfBook.Add(l);
               }
            }

            // Do a sort 
            if (bidOrAsks.Side.Equals("buy"))
               sideOfBook = sideOfBook.OrderByDescending(point => point.Price)
                  .ToList();
            else
               sideOfBook = sideOfBook.OrderBy(point => point.Price)
                  .ToList();

            if (IsBuy)
            {
               if (sideOfBook.Any())
               {
                  if (sideOfBook[0].Price != currentBestBidPrice)
                  {
                //     _logger.LogInformation("Best Bid change {current} {latest}", sideOfBook[0].Price,
                 //       currentBestBidPrice);
                     OnBestBidChange?.Invoke(venue, symbol, sideOfBook[0].Price, sideOfBook[0].Quantity);
                  }
               }
            }
            else
            {
               if (sideOfBook.Any())
               {
                  if (sideOfBook[0].Price != currentBestAskPrice)
                  {
                 //    _logger.LogInformation("Best Ask change {current} {latest}", sideOfBook[0].Price,
                    //    currentBestBidPrice);
                     OnBestAskChange?.Invoke(venue, symbol, sideOfBook[0].Price, sideOfBook[0].Quantity);
                  }
               }
            }
         }
      }
   }   
}
