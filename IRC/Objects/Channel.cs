using System.Collections.Generic;

namespace IRC.Objects
{
    public class Channel
    {
        public string Name { get; set; }

        public User Creator { get; set; }

        public List<User> Users { get; set; }
    }
}
