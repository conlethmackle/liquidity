﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public class TelegramConfigChange
   {
      public TelegramConfigChangeType ChangeType { get; set; }
      public string Data { get; set; }
   }
}
