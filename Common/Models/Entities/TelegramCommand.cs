using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class TelegramCommand
   {
      public int TelegramCommandId { get; set; }
      public string TelegramCommandText { get; set; }
      public int TelegramCommandTypeId { get; set; }
      public TelegramCommandType TelegramCommandType { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
