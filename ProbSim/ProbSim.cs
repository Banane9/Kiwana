using Kiwana.Api;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProbSim
{
    /// <summary>
    /// Plugin that adds Probability Simulation commands.
    /// </summary>
    public class ProbSim : Plugin
    {
        /// <summary>
        /// <see cref="Random"/> number generator.
        /// </summary>
        private Random _random = new Random();

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
                        case "random":
                            try
                            {
                                SendData(MessageTypes.PRIVMSG, recipient + " :" + Util.NickRegex.Match(ex[0]) + ": " + _random.Next(int.Parse(Regex.Match(ex[4], @"\d+").Value), int.Parse(Regex.Match(ex[5], @"\d+").Value)));
                            }
                            catch
                            {
                                SendData(MessageTypes.PRIVMSG, recipient + " :" + Util.NickRegex.Match(ex[0]) + ": Couldn't parse numbers.");
                            }
                            break;
                        case "dice":
                            try
                            {
                                int value = 0;
                                string nextMode = "+";
                                string formula = Util.JoinStringList(ex, start: 4);

                                if (Regex.IsMatch(formula, @"\d+((-|\+)\d+)?d\d+(-|\+)?"))
                                {
                                    MatchCollection diceFormulas = Regex.Matches(formula, @"\d+((-|\+)\d+)?d\d+(-|\+)?");
                                    foreach (Match diceFormula in diceFormulas)
                                    {
                                        int diceValue = 0;

                                        int dice = int.Parse(Regex.Match(diceFormula.Value, @"^\d+").Value);

                                        int add = 0;
                                        if (Regex.IsMatch(diceFormula.Value, @"(-|\+)\d+(?=d)"))
                                        {
                                            add = int.Parse(Regex.Match(diceFormula.Value, @"(-|\+)\d+(?=d)").Value);
                                        }

                                        int sides = int.Parse(Regex.Match(diceFormula.Value, @"(?<=d)\d+").Value);

                                        for (int i = 0; i < dice; i++)
                                        {
                                            diceValue += _random.Next(1, sides);
                                        }

                                        diceValue += add;

                                        switch (nextMode)
                                        {
                                            case "+":
                                                value += diceValue;
                                                break;
                                            case "-":
                                                value -= diceValue;
                                                break;
                                        }

                                        nextMode = Regex.Match(diceFormula.Value, @"(-|\+)$").Value;
                                    }

                                    SendData(MessageTypes.PRIVMSG, recipient + " :" + Util.NickRegex.Match(ex[0]) + ": " + value);
                                }
                                else
                                {
                                    SendData(MessageTypes.PRIVMSG, recipient + " :" + Util.NickRegex.Match(ex[0]) + ": Invalid dice formula.");
                                }
                            }
                            catch
                            {
                                SendData(MessageTypes.PRIVMSG, recipient + " :" + Util.NickRegex.Match(ex[0]) + ": Couldn't parse dice formula.");
                            }
                            break;
                    }
                }
                else if (ex.Count > 3)
                {
                    switch (command)
                    {
                        case "tosscoin":
                            SendData(MessageTypes.PRIVMSG, recipient + " :" + Util.NickRegex.Match(ex[0]) + ": " + (_random.Next(1, 3) == 1 ? "Tails" : "Heads"));
                            break;
                    }
                }
            }
        }
    }
}
