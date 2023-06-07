using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Configuration
{
   public class MessageBrokerConfig
   {
      public const string MsgBrokerConfig = "MessageBrokerConfig";
      public string Endpoint { get; set; }
      public int PortNo { get; set; }
      public string Username { get; set; }
      public string Password { get; set; }
      public string VirtualHost { get; set; }  = "/";
      public string ReceiveQueue { get; set; }
      public string[] PublishToQueues { get; set; }
      public string Exchange { get; set; }
   }
}
