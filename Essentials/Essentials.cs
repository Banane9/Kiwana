﻿using Kiwana.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essentials
{
    public class Essentials : Plugin
    {
        public override void HandleLine(List<string> ex, string command, bool userAuthenticated, bool console)
        {
            if (ex.Count > 4)
            {
                switch (command)
                {
                    case "say":
                        SendData("PRIVMSG", ex[2] + " :" + Util.JoinStringList(ex, " ", 4)); //channel + *space*: + message
                        break;
                    case "me":
                        SendData("PRIVMSG", ex[2] + " :\x01" + "ACTION " + Util.JoinStringList(ex, " ", 4) + "\x01");
                        break;
                    case "tell":
                        if (userAuthenticated || console)
                        {
                            SendData("PRIVMSG", ex[4] + " :" + Util.JoinStringList(ex, " ", 5));
                        }
                        else
                        {
                            SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                        }
                        break;
                    case "raw":
                        if (userAuthenticated || console)
                        {
                            if (ex[4].ToLower() == "quit")
                            {
                                SendData("PRIVMSG", ex[2] + " :Use the quit command for this.");
                            }
                            else
                            {
                                SendData(Util.JoinStringList(ex, " ", 4));
                            }
                        }
                        else
                        {
                            SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                        }
                        break;
                }
            }
            else
            {
                switch (command)
                {
                    case "ping":
                        SendData("PRIVMSG", ex[2] + " :pong!");
                        break;
                }
            }
        }
    }
}
