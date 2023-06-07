using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public class ConnectorStatusMsg
   {
      public ConnectorStatusMsg()
      {
         Public = new Status();
         Private = new Status();
      }

      public string Account { get; set; }
      public string Instance { get; set; }
      public Status Public { get; set; }
      public Status Private { get; set; }
   }

   public class Status
   {
      public string Venue { get; set; }
      public bool IsConnected { get; set; }
      public string? ErrorMsg { get; set; }
   }

   
}
