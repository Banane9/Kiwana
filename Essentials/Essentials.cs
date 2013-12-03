using Kiwana.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essentials
{
    public class Essentials : Plugin
    {
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
