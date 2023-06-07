using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncfusionLiquidity.Shared
{
   public class MessageQueueData
   {
      public QueueMsgTypes MessageType { get; set; }
      public string Venue { get; set; }
      public string Data { get; set; }
   }
}