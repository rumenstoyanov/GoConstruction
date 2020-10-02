using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoLibrary.Data.Constants;

namespace GoApi.Data.Constants
{
    public class Settings
    {
        public PgSqlSettings PgSqlSettings { get; set; }
        public JwtSettings JwtSettings { get; set; }
        public RedisSettings RedisSettings { get; set; }
        public MailSettings MailSettings { get; set; }
        public ServiceBusSettings ServiceBusSettings { get; set; }
    }
}
