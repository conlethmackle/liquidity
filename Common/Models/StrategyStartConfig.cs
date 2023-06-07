
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public interface IStrategyStartConfig
   {

   }
   public class StrategyStartConfig : IStrategyStartConfig
   {
      public string Account { get; set; }
      public string Strategy { get; set; }
      public string ConfigName { get; set; }

      public StrategyStartConfig(IConfiguration config)
      {
         Account = config.GetValue<string>("Account");
         Strategy = config.GetValue<string>("Strategy");
         ConfigName = config.GetValue<string>("ConfigName");
      }
   }
}
