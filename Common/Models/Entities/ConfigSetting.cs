using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   [Index(nameof(Name))]
   public class ConfigSetting
   {
      public int ConfigSettingId { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public string Value { get; set; }
   }
}
