using System.Collections.Generic;

namespace Kiwana.Plugins.Api
{
    /// <summary>
    /// The class all Plugins must inherit from.
    /// </summary>
    public abstract class Plugin
    {

        public delegate void SendData(string command, string argument = "");

        /// <summary>
        /// This method gets called every time there's an input that has to be handled by this plugin.
        /// </summary>
        /// <param name="ex">The List of strings resulting from splitting the input line at ' '. The command will already be matched to the name specified in the config (in lower case).</param>
        /// <param name="UserAuthenticated">Whether the user issuing the command is on the user list and authenticated with NickServ, or not.</param>
        /// <param name="console">Whether the command was issued from the console or not.</param>
        public abstract void HandleLine(List<string> ex, bool userAuthenticated, bool console, SendData SendData)
        {
            return;
        }
    }
}
