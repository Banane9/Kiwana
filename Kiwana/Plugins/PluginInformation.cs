using Kiwana.Api;
using Kiwana.Api.Config;
using Kiwana.Config;

namespace Kiwana.Plugins
{
    public class PluginInformation
    {
        public string Name { get; set; }

        public PluginConfig Config { get; set; }

        public Plugin Instance { get; set; }
    }
}
