using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class TelegramAlertBehaviour
   {
      public int Id { get; set; }
      public int TelegramAlertId { get; set; }
      public TelegramAlert TelegramAlert { get; set; }
      public int TelegramAlertBehaviourTypeId { get; set; }
      public TelegramAlertBehaviourType TelegramAlertBehaviourType { get; set; }
      public int TimeSpan { get; set; }
   }
}
