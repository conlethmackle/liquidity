using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class RabbitMQSettingsDTO
   {
      public int Id { get; set; }
      public static string Name = "RabbitMQSettings";
      public string Description { get; set; }    
      public string UserName { get; set; }
      public string Password { get; set; }
      public string Url { get; set; }
      public int Port { get; set; }
      public string VirtualHost { get; set; }
   }
}
