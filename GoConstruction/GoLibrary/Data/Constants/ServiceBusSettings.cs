using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoLibrary.Data.Constants
{
    public class ServiceBusSettings
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }
}
