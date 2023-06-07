using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Configuration
{
   public class DatabaseSettings
   {
      public const string SectionName = "Database";

      public string ConnectionString { get; set; } = default!;
   }
}
