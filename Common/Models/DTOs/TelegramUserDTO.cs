﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class TelegramUserDTO
   {
      public int Id { get; set; }
      public string UserName { get; set; }
      public string UserToken { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
