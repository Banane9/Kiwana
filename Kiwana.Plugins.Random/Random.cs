using Kiwana.Plugins.Api;
using System.Collections.Generic;

namespace Kiwana.Plugins.Random
{
    public class Random : Plugin
    {
        private System.Random _random = new System.Random();

        public void HandleLine(List<string> ex, bool userAuthenticated, bool console)
        {

        }
    }
}
