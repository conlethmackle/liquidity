using Common.Messages;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorCommon
{
   public interface IPrivateClient
   {      
      Task Init(PrivateConnectionLogon accountsConfig);
      Task PlaceOrder(PlaceOrderCmd cmd);
      Task PlaceMarketOrder(PlaceOrderCmd cmd);
      Task GetOpenOrders(string[] symbols);
      Task CancelOrder(OrderIdHolder orderId);
      Task CancelAllOrdersCommand(string symbol);
      Task GetBalances(int jobNo);
      bool LoggedInState();
      Task DisconnectPrivate();
   }
}
