using Kiwana.Core.Api;
using System.Collections.Generic;

namespace AsciiArt
{
    public class AsciiArt : Plugin
    {
        public override void HandleLine(List<string> ex, string command, bool userAuthenticated, bool userAuthorized, bool console)
        {
            if (userAuthorized || console)
            {
                switch (command)
                {
                    case "doge":
                        foreach (string line in _doge)
                        {
                            SendData("PRIVMSG", ex[2] + " :" + line);
                        }
                        break;
                }
            }

        }

        private string[] _doge = new string[] {
            "░░░░░░░░░▄░░░░░░░░░░░░░░▄░░░░",
            "░░░░░░░░▌▒█░░░░░░░░░░░▄▀▒▌░░░",
            "░░░░░░░░▌▒▒█░░░░░░░░▄▀▒▒▒▐░░░",
            "░░░░░░░▐▄▀▒▒▀▀▀▀▄▄▄▀▒▒▒▒▒▐░░░",
            "░░░░░▄▄▀▒░▒▒▒▒▒▒▒▒▒█▒▒▄█▒▐░░░",
            "░░░▄▀▒▒▒░░░▒▒▒░░░▒▒▒▀██▀▒▌░░░",
            "░░▐▒▒▒▄▄▒▒▒▒░░░▒▒▒▒▒▒▒▀▄▒▒▌░░",
            "░░▌░░▌█▀▒▒▒▒▒▄▀█▄▒▒▒▒▒▒▒█▒▐░░",
            "░▐░░░▒▒▒▒▒▒▒▒▌██▀▒▒░░░▒▒▒▀▄▌░",
            "░▌░▒▄██▄▒▒▒▒▒▒▒▒▒░░░░░░▒▒▒▒▌░",
            "▀▒▀▐▄█▄█▌▄░▀▒▒░░░░░░░░░░▒▒▒▐░",
            "▐▒▒▐▀▐▀▒░▄▄▒▄▒▒▒▒▒▒░▒░▒░▒▒▒▒▌",
            "▐▒▒▒▀▀▄▄▒▒▒▄▒▒▒▒▒▒▒▒░▒░▒░▒▒▐░",
            "░▌▒▒▒▒▒▒▀▀▀▒▒▒▒▒▒░▒░▒░▒░▒▒▒▌░",
            "░▐▒▒▒▒▒▒▒▒▒▒▒▒▒▒░▒░▒░▒▒▄▒▒▐░░",
            "░░▀▄▒▒▒▒▒▒▒▒▒▒▒░▒░▒░▒▄▒▒▒▒▌░░",
            "░░░░▀▄▒▒▒▒▒▒▒▒▒▒▄▄▄▀▒▒▒▒▄▀░░░",
            "░░░░░░▀▄▄▄▄▄▄▀▀▀▒▒▒▒▒▄▄▀░░░░░",
            "░░░░░░░░░▒▒▒▒▒▒▒▒▒▒▀▀░░░░░░░░"
        };
    }
}
