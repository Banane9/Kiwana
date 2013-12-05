namespace Kiwana.Objects
{
    /// <summary>
    /// Stores information about a user that the bot performed a check on.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The rank of the user.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Has the authentication of the user been requested by the bot?
        /// </summary>
        public bool AuthenticationRequested { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="rank">The rank of the user. Default is the <see cref="int"/>.MinValue</param>
        /// <param name="authenticationRequested">Whether the authentication of the user has been requested.</param>
        public User(int rank = int.MinValue, bool authenticationRequested = false)
        {
            Rank = rank;
            AuthenticationRequested = authenticationRequested;
        }
    }
}
