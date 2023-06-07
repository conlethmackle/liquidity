using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DashboardServer.Hubs
{
   public interface IChatHub
   {
      Task SendMessage(string user, string message);
      Task SendOrderBookUpdate(string user, string message);
   }

   public class ChatHub : Hub, IChatHub
   {
      public static IHubContext<ChatHub> CurrentHub { get; set; }
      public ChatHub(ILoggerFactory factory, IHubContext<ChatHub> thisHub)
      {
         if (CurrentHub == null)
         {
            CurrentHub = thisHub;
           
         }
      }

      public async Task SendMessage(string user, string message)
      {
         await CurrentHub?.Clients.All.SendAsync("ReceiveMessage", user, message);
      }

      public async Task SendOrderBookUpdate(string user, string message)
      {
         await CurrentHub?.Clients.All.SendAsync("ReceiveOrderBook", user, message);
       
      }
   }
}
