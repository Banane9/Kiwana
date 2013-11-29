using System;
using System.Collections.Generic;

namespace Kiwana.Core.Objects
{
    public class Channel
    {
        public string Name { get; set; }

        public string Motd { get; set; }

        public ChannelUser MotdSetter { get; set; }

        public DateTime MotdSetDate { get; set; }

        public Dictionary<string, ChannelUser> Users { get; set; }

        public Channel(string name = "", string motd = "", ChannelUser motdSetter = null, int motdSetDate = 0, Dictionary<string, ChannelUser> users = null)
        {
            Name = name;
            Motd = motd;
            MotdSetter = motdSetter != null ? motdSetter : new ChannelUser();
            MotdSetDate = motdSetDate > 0 ? new DateTime(motdSetDate) : new DateTime();
            Users = users != null ? users : new Dictionary<string, ChannelUser>();
        }
    }
}
