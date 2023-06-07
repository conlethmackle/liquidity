﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class Order
   {
      public int Id { get; set; }
      public int SPId { get; set; }
      public SP SP { get; set; }
      public int VenueId { get; set; }
      public Venue Venue { get; set; }
      public int CoinPairId { get; set; }
      public CoinPair CoinPair { get; set; }
      public string Symbol { get; set; }
      public OrderTypeEnum OrderType { get; set; }
      public bool IsBuy { get; set; }
      public string OrderId { get; set; }
      public OwnOrderUpdateStatusEnum Type { get; set; }
      public DateTime OrderTime { get; set; }
      public decimal Quantity { get; set; }
      public decimal FilledQuantity { get; set; }
      public decimal Price { get; set; }
      public string ClientOid { get; set; }
      public decimal RemainingQuantity { get; set; }
      public string Status { get; set; }
      public DateTime Timestamp { get; set; }
     
      public string Account { get; set; }
      public string Instance { get; set; }
   }
}