﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class TelegramSubscriberToChannel
   {
      public int TelegramSubscriberToChannelId { get; set; }
      public int TelegramChannelId { get; set; }
      public TelegramChannel TelegramChannel { get; set; }
      public int TelegramUserId { get; set; }
      public TelegramUser TelegramUser { get; set; }
      public bool IsAuthorised { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
