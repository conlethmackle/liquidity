using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class TelegramAlertToChannelDTO
   {
      public int TelegramAlertToChannelId { get; set; }
      public int TelegramAlertId { get; set; }
      public TelegramAlertDTO TelegramAlert { get; set; }
      public int TelegramChannelId { get; set; }
      public TelegramChannelDTO TelegramChannel { get; set; }
      public bool IsAuthorised { get; set; }
      public DateTime DateCreated { get; set; }
      
   }
}
