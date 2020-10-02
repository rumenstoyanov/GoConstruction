using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoLibrary.Data.Constants
{
    /// <summary>
    /// Binded from configuration at startup.
    /// </summary>
    public class RedisSettings
    {
        public bool IsEnabled { get; set; }
        public string ConnectionString { get; set; }
        public int TimeToLiveSeconds { get; set; }
    }
}
