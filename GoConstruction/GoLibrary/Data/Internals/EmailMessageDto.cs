using System;
using System.Collections.Generic;
using System.Text;

namespace GoLibrary.Data.Internals
{
    public class EmailMessageDto
    {
        public string ToName { get; set; }
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
    }
}
