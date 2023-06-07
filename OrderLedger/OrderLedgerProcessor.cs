using Common.Messages;
using Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderLedger
{

   public class OrderLedgerProcessor : IOrderLedgerProcessor
   {
      private Dictionary<string, List<OwnOrderChange>> _orderLedger { get; set; } = new Dictionary<string, List<OwnOrderChange>>();
      private readonly ILogger<OrderLedgerProcessor> _logger;

      public OrderLedgerProcessor(ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<OrderLedgerProcessor>();
      }

   //   public void AddOrder(OrderPlacementHolder orderHolder)
    //  {
    //     List<OwnOrderChange> orderList = null;
     //    if (!_orderLedger.ContainsKey(orderHolder.Venue))
    //     {
   //         orderList = new List<OwnOrderChange>();
   //         _orderLedger.Add(orderHolder.Venue, orderList);
   //      }
    //     else
    //        orderList = _orderLedger[orderHolder.Venue];
     //    orderList.Add(orderHolder.Order);
     // }
   }
}
