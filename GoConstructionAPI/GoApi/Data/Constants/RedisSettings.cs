using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Constants
{
    public class RedisSettings
    {
        public bool IsEnabled { get; set; }
        public string ConnectionString { get; set; }
    }
}
