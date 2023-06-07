using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.SecretsManager.Model;

namespace Okx.Models
{
   public  enum OkxOrderType
   {
      limit   = 0,
      market = 1,
      post_only = 2,
      fok = 3,
      ioc = 4,
      optional_limit_ioc = 5,
   }
}
