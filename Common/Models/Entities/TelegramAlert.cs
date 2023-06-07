using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class TelegramAlert
   {
      public int TelegramAlertId { get; set; }
      public string AlertName { get; set; }
      public int AlertEnumId { get; set; }
      public int AlertCategoryId { get; set; }
      public TelegramAlertCategory AlertCategory { get; set; }
      public string Message { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
