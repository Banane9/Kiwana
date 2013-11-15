using Kiwana.Core.Api;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProbSim
{
    public class ProbSim : Plugin
    {
        private Random _random = new Random();

        public override void HandleLine(List<string> ex, string command, bool userAuthenticated, bool console)
        {
            if (ex.Count > 4)
            {
                switch (command)
                {
                    case "random":
                        try
                        {
                            SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": " + _random.Next(int.Parse(Regex.Match(ex[4], @"\d+").Value), int.Parse(Regex.Match(ex[5], @"\d+").Value)));
                        }
                        catch
                        {
                            SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": Couldn't parse numbers.");
                        }
                        break;
                    case "dice":
                        try
                        {
                            int value = 0;
                            string nextMode = "+";

                            if (Regex.IsMatch(Util.JoinStringList(ex, start: 4), @"\d+((-|\+)\d+)?d\d+(-|\+)?"))
                            {
                                MatchCollection diceFormulas = Regex.Matches(Util.JoinStringList(ex, start: 4), @"\d+((-|\+)\d+)?d\d+(-|\+)?");
                                foreach (Match diceFormula in diceFormulas)
                                {
                                    Console.WriteLine(diceFormula.Value);

                                    int diceValue = 0;

                                    int dice = int.Parse(Regex.Match(diceFormula.Value, @"^\d+").Value);

                                    int add = 0;
                                    if (Regex.IsMatch(ex[4], @"(-|\+)\d+(?=d)"))
                                    {
                                        add = int.Parse(Regex.Match(diceFormula.Value, @"(-|\+)\d+(?=d)").Value);
                                    }

                                    int max = int.Parse(Regex.Match(diceFormula.Value, @"(?<=d)\d+").Value);

                                    for (int i = 0; i < dice; i++)
                                    {
                                        diceValue += _random.Next(1, max);
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

                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": " + value);
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": No valid dice formula found.");
                            }
                        }
                        catch
                        {
                            SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": Couldn't parse dice formula.");
                        }
                        break;
                }
            }
            else if (ex.Count > 3)
            {
                switch (command)
                {
                    case "tosscoin":
                        SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": " + (_random.Next(1, 3) == 1 ? "Tails" : "Heads"));
                        break;
                }
            }
        }
    }
}
