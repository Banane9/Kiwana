using Kiwana.Api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace McStatus
{
    /// <summary>
    /// Plugin to fetch the status of the Mojang/Minecraft services and print them.
    /// </summary>
    public class McStatus : Plugin
    {
        /// <summary>
        /// <see cref="JavaScriptSerializer"/> to deserialize the response from the server.
        /// </summary>
        private static JavaScriptSerializer _jsonSerializer = new JavaScriptSerializer();

        /// <summary>
        /// <see cref="Uri"/> from where to fetch the status.
        /// </summary>
        private static Uri _mcStatusUrl = new Uri("http://status.mojang.com/check");

        /// <summary>
        /// <see cref="HttpClient"/> for getting the status from the server.
        /// </summary>
        private static HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// <see cref="Dictionary"/> matching the status codes to the message that is displayed on the website for it.
        /// </summary>
        public static Dictionary<string, string> StatusMessages = new Dictionary<string, string>() {
            {"green", "This service is healthy. All is good!"},
            {"yellow", "This service might be a bit shaky. We are doing our best to stabilize it."},
            {"red", "This service is down! We are doing our very best to resolve the issue as soon as possible."}
        };

        /// <summary>
        /// <see cref="Dictionary"/> matching the server Urls to the Names used on the website.
        /// </summary>
        public static Dictionary<string, string> ServiceNames = new Dictionary<string, string>() {
            {"minecraft.net", "Minecraft.net"},
            {"login.minecraft.net", "Legacy Minecraft Logins"},
            {"session.minecraft.net", "Legacy Minecraft Sessions"},
            {"account.mojang.com", "Mojang Accounts Website"},
            {"auth.mojang.com", "Legacy Mojang Account Login"},
            {"skins.minecraft.net", "Minecraft Skins"},
            {"authserver.mojang.com", "Authentication Service"}
        };

        /// <summary>
        /// Gets the status from the server.
        /// </summary>
        /// <returns><see cref="Dictionary"/> with the status code matched to the url of the service.</returns>
        public static async Task<Dictionary<string, string>> GetMcStatus()
        {

            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            try
            {
                List<Dictionary<string, string>> mcStatus = _jsonSerializer.Deserialize<List<Dictionary<string, string>>>(await _httpClient.GetStringAsync(_mcStatusUrl));

                foreach (Dictionary<string, string> status in mcStatus)
                {
                    foreach (KeyValuePair<string, string> entry in status)
                    {
                        toReturn.Add(entry.Key, entry.Value);
                    }
                }
            }
            catch { }

            return toReturn;
        }

        /// <summary>
        /// Writes the status of the services, listing each service's name with the status message in a new message.
        /// </summary>
        /// <param name="recipient">The receiver of the messages.</param>
        private async void _writeMcStatusVerbose(string recipient)
        {
            Dictionary<string, string> mcStatus = await GetMcStatus();

            if (mcStatus.Count > 0)
            {
                foreach (KeyValuePair<string, string> entry in mcStatus)
                {
                    if (ServiceNames.ContainsKey(entry.Key) && StatusMessages.ContainsKey(entry.Value))
                        SendData(MessageTypes.PRIVMSG, recipient + " :" + ServiceNames[entry.Key] + ": " + StatusMessages[entry.Value]);
                }
            }
            else
            {
                SendData(MessageTypes.PRIVMSG, recipient + " :Couldn't retrieve status information.");
            }
        }

        /// <summary>
        /// Writes the status of the services, listing either that all are ok, or the ones that aren't with the service's name and status message in a new message each.
        /// </summary>
        /// <param name="recipient">The receiver of the messages.</param>
        private async void _writeMcStatus(string recipient)
        {
            Dictionary<string, string> mcStatus = await GetMcStatus();

            if (mcStatus.Count > 0)
            {
                List<string> notOk = new List<string>();
                foreach (KeyValuePair<string, string> entry in mcStatus)
                {
                    if (ServiceNames.ContainsKey(entry.Key) && StatusMessages.ContainsKey(entry.Value))
                    {
                        if (entry.Value != "green")
                        {
                            notOk.Add(entry.Key);
                        }
                    }
                }

                if (notOk.Count == 0)
                {
                    SendData(MessageTypes.PRIVMSG, recipient + " :All services healthy. All is good!");
                }
                else
                {
                    SendData(MessageTypes.PRIVMSG, recipient + " :These services aren't ok:");
                    foreach (string key in notOk)
                    {
                        SendData(MessageTypes.PRIVMSG, recipient + " :" + ServiceNames[key] + ": " + StatusMessages[mcStatus[key]]);
                    }
                }
            }
            else
            {
                SendData(MessageTypes.PRIVMSG, recipient + " :Couldn't retrieve status information.");
            }
        }

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
                switch (command)
                {
                    case "verbosemcstatus":
                        SendData(MessageTypes.PRIVMSG, recipient + " :Retrieving status of Minecraft services... Minor WOT incoming...");
                        _writeMcStatusVerbose(recipient);
                        break;
                    case "mcstatus":
                        SendData(MessageTypes.PRIVMSG, recipient + " :Retrieving status of Minecraft services...");
                        _writeMcStatus(recipient);
                        break;
                }
            }
        }
    }
}
