using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public class RabbitConnectionSettings
   {
      public string HostName { get; set; }  // "localhost", //
      public string UserName { get; set; } //= _config.Username, // "guest", //
      public string Password { get; set; } //= _config.Password, // "guest", //
      public int Port { get; set; }       //= _config.PortNo, // 5672, //
      public string VirtualHost { get; set; } // = _config.VirtualHost // "/" //
   }
}
