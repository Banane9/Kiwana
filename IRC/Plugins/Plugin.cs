using Kiwana.Core.Config;

namespace Kiwana.Core.Plugins
{
    public class Plugin
    {
        public string Name { get; set; }

        public PluginConfig Config { get; set; }

        public dynamic Instance { get; set; }
    }
}
