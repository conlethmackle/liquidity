using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class TelegramAlertBehaviourDTO
   {
      public int Id { get; set; }
      public int TelegramAlertId { get; set; }
      public TelegramAlertDTO TelegramAlert { get; set; }
      public int TelegramAlertBehaviourTypeId { get; set; }
      public TelegramAlertBehaviourTypeDTO TelegramAlertBehaviourType { get; set; }
      public int TimeSpan { get; set; }
   }
}
