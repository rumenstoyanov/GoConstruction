using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Constants
{
    /// <summary>
    /// Binded from configuration at startup.
    /// </summary>
    public class JwtSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SigningKey { get; set; }
        public double TokenLifetimeHours { get; set; }
    }
}
