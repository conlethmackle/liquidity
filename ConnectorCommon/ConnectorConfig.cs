using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorCommon
{
   public interface IConnectorConfig
   {

   }

   public class ConnectorConfig : IConnectorConfig
   {
      public string Exchange { get; set; }

      public ConnectorConfig(IConfiguration config)
      {
         Exchange = config.GetValue<string>("Exchange");         
      }
   }
}
