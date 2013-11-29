using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kiwana.Core.Objects
{
    public class ChannelUser
    {
        public string HostMask { get; set; }

        public bool Authenticated { get; set; }

        public ChannelUser(string hostMask = "", bool authenticated = false)
        {
            HostMask = hostMask;
            Authenticated = authenticated;
        }
    }
}
