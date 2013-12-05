using Kiwana.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essentials
{
    /// <summary>
    /// Plugin for various essential commands.
    /// </summary>
    public class Essentials : Plugin
    {
        /// <summary>
        /// Accepts the NewLine event of the <see cref="Kiwana"/> class.
        /// </summary>
        /// <param name="ex">The line from the server, split at spaces.</param>
        /// <param name="recipient">The channel or user the message came from. Empty when from console.</param>
        /// <param name="command">The normalized command. Empty if there's no command.</param>
        /// <param name="userAuthorized">Whether the user sending the command is authorized. False if there's no command.</param>
        /// <param name="console">Whether the line came from the console.</param>
        public override void HandleLine(List<string> ex, string recipient, string command, bool userAuthorized, bool console)
        {
            if (userAuthorized)
            {
                if (ex.Count > 4)
                {
                    switch (command)
                    {
                        case "say":
                            SendData(MessageTypes.PRIVMSG, recipient + " :" + Util.JoinStringList(ex, " ", 4)); //channel + *space*: + message
                            break;
                        case "me":
                            SendData(MessageTypes.PRIVMSG, recipient + " :\x01" + "ACTION " + Util.JoinStringList(ex, " ", 4) + "\x01");
                            break;
                        case "tell":
                            SendData(MessageTypes.PRIVMSG, ex[4] + " :" + Util.JoinStringList(ex, " ", 5));
                            break;
                    }
                }
                else
                {
                    switch (command)
                    {
                        case "ping":
                            SendData(MessageTypes.PRIVMSG, recipient + " :pong!");
                            break;
                    }
                }
            }
        }
    }
}
