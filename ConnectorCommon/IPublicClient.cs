using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorCommon
{
   public interface IPublicClient
   {      
      Task Init();
      Task GetReferenceData();
      void GetOrderBook(string instanceName, string symbol);
      Task DisconnectPublic();
      Task GetLatestTrades(string instanceName, string symbol);
   }
}
