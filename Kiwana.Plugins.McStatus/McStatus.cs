using Kiwana.Core.Api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace McStatus
{
    public class McStatus : Plugin
    {
        private static JavaScriptSerializer _jsonSerializer = new JavaScriptSerializer();
        private static Uri _mcStatusUrl = new Uri("http://status.mojang.com/check");
        private static HttpClient _httpClient = new HttpClient();

        public static Dictionary<string, string> StatusMessages = new Dictionary<string, string>() {
            {"green", "This service is healthy. All is good!"},
            {"yellow", "This service might be a bit shaky. We are doing our best to stabilize it."},
            {"red", "This service is down! We are doing our very best to resolve the issue as soon as possible."}
        };

        public static Dictionary<string, string> ServiceNames = new Dictionary<string, string>() {
            {"minecraft.net", "Minecraft.net"},
            {"login.minecraft.net", "Legacy Minecraft Logins"},
            {"session.minecraft.net", "Legacy Minecraft Sessions"},
            {"account.mojang.com", "Mojang Accounts Website"},
            {"auth.mojang.com", "Legacy Mojang Account Login"},
            {"skins.minecraft.net", "Minecraft Skins"},
            {"authserver.mojang.com", "Authentication Service"}
        };

        public static async Task<Dictionary<string, string>> GetMcStatus()
        {
            List<Dictionary<string, string>> mcStatus = _jsonSerializer.Deserialize<List<Dictionary<string, string>>>(await _httpClient.GetStringAsync(_mcStatusUrl));
            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            foreach (Dictionary<string, string> status in mcStatus)
            {
                foreach (string key in status.Keys)
                {
                    toReturn.Add(key, status[key]);
                }
            }

            return toReturn;
        }

        private async void _writeMcStatus(string who)
        {
            Dictionary<string, string> mcStatus = await GetMcStatus();

            foreach (string key in mcStatus.Keys)
            {
                if (ServiceNames.ContainsKey(key) && StatusMessages.ContainsKey(mcStatus[key]))
                    SendData("PRIVMSG", who + " :" + ServiceNames[key] + ": " + StatusMessages[mcStatus[key]]);
            }
        }

        public override void HandleLine(List<string> ex, string command, bool userAuthenticated, bool userAuthorized, bool console)
        {
            if (command == "mcstatus")
            {
                if (userAuthenticated)
                {
                    SendData("PRIVMSG", ex[2] + " :Retrieving status of Minecraft services...");
                    _writeMcStatus(ex[2]);
                }
            }
        }
    }
}
