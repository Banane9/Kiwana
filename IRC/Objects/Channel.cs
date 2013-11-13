using System.Collections.Generic;

namespace Kiwana.Core.Objects
{
    public class Channel
    {
        public string Name { get; set; }

        public User Creator { get; set; }

        public List<User> Users { get; set; }
    }
}
