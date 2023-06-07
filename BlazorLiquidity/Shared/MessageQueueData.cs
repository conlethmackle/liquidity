using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorLiquidity.Shared
{
   public class MessageQueueData
   {
      public QueueMsgTypes MessageType { get; set; }
      public string Venue { get; set; }
      public bool IsPublic { get; set; } = true;
      public string Data { get; set; }
   }
}