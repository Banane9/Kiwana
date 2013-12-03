using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kiwana.Api
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
        public delegate void SendDataEventHandler(MessageTypes messageType, string argument = "");

        /// <summary>
        /// Used to send the data back to the core.
        /// </summary>
        public event SendDataEventHandler SendDataEvent;

        /// <summary>
        /// This function is used by the derived class to send the event. Can be overridden.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="argument"></param>
        protected virtual void SendData(MessageTypes messageType, string argument = "")
        {
            SendDataEvent(messageType, argument);
        }

        /// <summary>
        /// This method gets called every time the bot receives a line.
        /// </summary>
        /// <param name="ex">The List of strings resulting from splitting the input line at ' '.</param>
        /// <param name="recipient">What to send the response to.</param>
        /// <param name="command">The command will be matched to the name specified in the config (in lower case).</param>
        /// <param name="userAuthorized">Whether the user is authorized to execute the command or not. If there's no command it's false too.</param>
        /// <param name="console">Whether the command was issued from the console or not.</param>
        public virtual void HandleLine(List<string> ex, string recipient, string command, bool userAuthorized, bool console)
        {
            return;
        }

        /// <summary>
        /// Called when the class is initialized. Load data, etc. here.
        /// </summary>
        public virtual void Load()
        {
            return;
        }

        /// <summary>
        /// Called when the bot shuts down. Save data, etc. here.
        /// </summary>
        public virtual void Unload()
        {
            return;
        }
    }
}
