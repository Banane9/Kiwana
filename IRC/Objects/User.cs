using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kiwana.Core.Objects
{
    public class User
    {
        public int Rank { get; set; }

        public bool Authenticated { get; set; }

        public bool AuthenticationRequested { get; set; }

        public User(int rank = 0, bool authenticated = false, bool authenticationRequested = false)
        {
            Rank = rank;
            Authenticated = authenticated;
            AuthenticationRequested = authenticationRequested;
        }
    }
}
