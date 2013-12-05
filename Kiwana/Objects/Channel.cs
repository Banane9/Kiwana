using System;
using System.Collections.Generic;

namespace Kiwana.Objects
{
    /// <summary>
    /// Stores information about a channel the bot is in.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// The name of the channel.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The motd of the channel.
        /// </summary>
        public string Motd { get; set; }

        /// <summary>
        /// The nick of the user who set the motd.
        /// </summary>
        public string MotdSetter { get; set; }

        /// <summary>
        /// The <see cref="DateTime"/> when the motd was set.
        /// </summary>
        public DateTime MotdSetDate { get; set; }

        /// <summary>
        /// List of User nicks in the channel.
        /// </summary>
        public List<string> Users { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="name">The name of the channel.</param>
        /// <param name="motd">The motd of the channel.</param>
        /// <param name="motdSetter">The nick of the user who set the motd.</param>
        /// <param name="motdSetDate">Timestamp of the date the motd was set.</param>
        /// <param name="users">List of user nicks in the channel.</param>
        public Channel(string name = "", string motd = "", string motdSetter = "", long motdSetDate = 0, List<string> users = null)
        {
            Name = name;
            Motd = motd;
            MotdSetter = motdSetter;
            MotdSetDate = motdSetDate > 0 ? new DateTime(motdSetDate) : new DateTime();
            Users = users ?? new List<string>();
        }
    }
}
