using Common.Models;
using DashboardServer.Hubs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Timers;

namespace DashboardServer.Sender
{
   public interface IFakeOrderBookSender
   {

   }

   public class FakeOrderBookSender : IFakeOrderBookSender
   {
      public Timer _timer; 
      private readonly IChatHub _chatHub;

     

      public FakeOrderBookSender(ILoggerFactory factory, IChatHub chatHub)
      {
         _timer = new Timer();
         _timer.Elapsed += OnTimerExpired;
         _timer.Enabled = true;
         _timer.Interval = 1000;
         _chatHub = chatHub;
 
      }

      private void OnTimerExpired(object sender, ElapsedEventArgs e)
      {
         _timer.Enabled = false;
         OrderBookSnapshot snapshot = new OrderBookSnapshot
         {
            Symbol = "BTCUSDT",
            OrderBook = new OrderBook()
            {
               Ask = new List<Level>()
               {
                  new Level(29996, 0.15m),
                  new Level(29997, 0.10m),
                  new Level(29998, 0.08m),
                  new Level(29999, 0.06m),
                  new Level(30000, 0.05m),
               },

               Bid = new List<Level>()
               {
                  new Level(29995, 0.05m),
                  new Level(29994, 0.06m),
                  new Level(29993, 0.08m),
                  new Level(29992, 0.10m),
                  new Level(29991, 0.15m),
               },
            }
         };

         var message = JsonSerializer.Serialize(snapshot);
         _chatHub.SendOrderBookUpdate("mackle", message);
         _timer.Enabled = true;
      }
   }
}
