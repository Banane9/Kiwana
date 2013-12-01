using System;
using System.Collections.Generic;

namespace Kiwana.Objects
{
    public class Channel
    {
        public string Name { get; set; }

        public string Motd { get; set; }

        public string MotdSetter { get; set; }

        public DateTime MotdSetDate { get; set; }

        public List<string> Users { get; set; }

        public Channel(string name = "", string motd = "", string motdSetter = "", int motdSetDate = 0, List<string> users = null)
        {
            Name = name;
            Motd = motd;
            MotdSetter = motdSetter;
            MotdSetDate = motdSetDate > 0 ? new DateTime(motdSetDate) : new DateTime();
            Users = users != null ? users : new List<string>();
        }
    }
}
