using BlazorLiquidity.Shared;
using Common.Models;
using Microsoft.AspNetCore.SignalR;

namespace SyncfusionLiquidity.Server.Hubs
{
   public interface IPortfolioHub
   {
      Task OpeningBalance(string venue, string currency, ExchangeBalance balanceUpdate);
      Task BalanceUpdate(string venue, string currency, ExchangeBalance balanceUpdate);
      Task FairValueUpdate(FairValueData data );
      Task RealTimeUpdate(MessageQueueData messageQueueData);
   }
   public class PortfolioHub : Hub<IPortfolioHub>
   {
      public async Task JoinStrategyInstanceGroup(string instanceName)
      {
         await Groups.AddToGroupAsync(Context.ConnectionId, instanceName);
      }
      public async Task LeaveStrategyInstanceGroup(string instanceName)
      {
         await Groups.RemoveFromGroupAsync(Context.ConnectionId, instanceName);
      }
   }
}
