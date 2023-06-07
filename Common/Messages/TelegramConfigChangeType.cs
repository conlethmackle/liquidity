using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public enum TelegramConfigChangeType
   {
      USER_TO_CHANNEL = 1,
      USER_TO_COMMAND = 2,
      USER_TO_ALERT = 3,
      USER_CHANGE = 4,
      COMMAND_CHANGE = 5,
      CHANNEL_CHANGE = 6,
      ALERT_CHANGE = 7,
      ALERT_TO_CHANNEL = 8
   }
}
