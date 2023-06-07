using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class TelegramAlertToChannel
   {
      public int TelegramAlertToChannelId { get; set; }
      public int TelegramAlertId { get; set; }
      public TelegramAlert TelegramAlert { get; set; }
      public int TelegramChannelId { get; set; }
      public TelegramChannel TelegramChannel { get; set; }
      public bool IsAuthorised { get; set; }
      public DateTime DateCreated { get; set; }
      
   }
}
