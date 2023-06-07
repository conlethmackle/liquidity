using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class TelegramChannelDTO
   {
      public int TelegramChannelId { get; set; }
      public string ChannelName { get; set; }
      public string TokenId { get; set; }
      public int TelegramAlertCategoryId { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
