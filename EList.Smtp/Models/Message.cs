using System;
using System.Collections.Generic;
using System.Text;

namespace EList.Smtp.Models
{
    public class Message
    {
        public string RecipientEmail { get; set; }

        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public bool IsBodyHtml { get; set; }
    }
}
