using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kiwana.Core.Objects
{
    public class ChannelUser
    {
        public bool Authorized { get; set; }

        public bool Authenticated { get; set; }

        public bool AuthenticationRequested { get; set; }

        public ChannelUser(bool authorized = false, bool authenticated = false, bool authenticationRequested = false)
        {
            Authorized = authorized;
            Authenticated = authenticated;
            AuthenticationRequested = authenticationRequested;
        }
    }
}
