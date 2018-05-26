using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Queue.Models
{
    public class Token
    {
        public string Value { get; }
        public DateTime ExpiresOn { get; }

        public Token(string value, DateTime expiresOn)
        {
            Value = value;
            ExpiresOn = expiresOn;
        }

        public bool IsExpired => DateTime.Now.AddMinutes(-1) > ExpiresOn;
    }
}
