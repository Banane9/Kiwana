using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IRC.Objects
{
    public class User
    {
        public string Name { get; set; }

        public string Nick { get; set; }

        public bool Authenticated { get; set; }
    }
}
