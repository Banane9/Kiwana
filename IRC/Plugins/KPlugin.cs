using Kiwana.Core.Api;
using Kiwana.Core.Config;

namespace Kiwana.Core.Plugins
{
    public class KPlugin
    {
        public string Name { get; set; }

        public PluginConfig Config { get; set; }

        public Plugin Instance { get; set; }
    }
}
