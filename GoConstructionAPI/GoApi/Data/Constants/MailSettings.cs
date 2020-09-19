using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Constants
{
    /// <summary>
    /// Binded from configuration at startup.
    /// </summary>
    public class MailSettings
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string SenderName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
