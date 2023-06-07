using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class TelegramCommandToUserDTO
   {

      public TelegramCommandToUserDTO()
      {
         TelegramCommand = new();
         TelegramUser = new();
      }
      public int TelegramCommandToUserId { get; set; }
      public int TelegramCommandId { get; set; }
      public TelegramCommandDTO TelegramCommand { get; set; }
      public int TelegramUserId { get; set; }
      public TelegramUserDTO TelegramUser { get; set; }
      public bool IsAuthorised { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
