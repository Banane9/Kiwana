using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kiwana.Core.Api
{
    /// <summary>
    /// The class all Plugins must inherit from.
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// The delegate for the SendData function. This will be given to you from the core assembly every call.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="argument"></param>
        public delegate void SendDataEventHandler(string command, string argument = "");

        /// <summary>
        /// Used to send the data back to the core.
        /// </summary>
        public event SendDataEventHandler SendDataEvent;

        /// <summary>
        /// This function is used by the derived class to send the event. Can be overridden.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="argument"></param>
        protected virtual void SendData(string command, string argument = "")
        {
            SendDataEvent(command, argument);
        }

        /// <summary>
        /// This method gets called every time the bot receives a line.
        /// </summary>
        /// <param name="ex">The List of strings resulting from splitting the input line at ' '. The command will already be matched to the name specified in the config (in lower case).</param>
        /// <param name="UserAuthenticated">Whether the user issuing the command is on the user list and authenticated with NickServ, or not.</param>
        /// <param name="console">Whether the command was issued from the console or not.</param>
        public virtual void HandleLine(List<string> ex, string command, bool userAuthenticated, bool userAuthorized, bool console)
        {
            return;
        }

        /// <summary>
        /// Called when the class is initialized. Load data, etc. here.
        /// </summary>
        public virtual void Init()
        {
            return;
        }

        /// <summary>
        /// Called when the bot shuts down. Save data, et.c here.
        /// </summary>
        public virtual void Disable()
        {
            return;
        }
    }
}
