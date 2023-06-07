using Common.Models;
using Microsoft.AspNetCore.SignalR;

namespace PortfolioManagementAPI.Hubs
{
   public interface IPortfolioHub
   {
      Task BalanceUpdate(ExchangeBalance balanceUpdate);
   }
   public class PortfolioHub : Hub<IPortfolioHub>
   {
      public async Task JoinSurveyGroup(Guid surveyId)
      {
         await Groups.AddToGroupAsync(Context.ConnectionId, surveyId.ToString());
      }
      public async Task LeaveSurveyGroup(Guid surveyId)
      {
         await Groups.RemoveFromGroupAsync(Context.ConnectionId, surveyId.ToString());
      }
   }
}
