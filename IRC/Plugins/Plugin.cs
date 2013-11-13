using Kiwana.Core.Config;

namespace Kiwana.Core.Plugins
{
    public class Plugin
    {
        public string Name { get; set; }

        public PluginConfig Config { get; set; }

        public Kiwana.Plugins.Api.Plugin Instance { get; set; }
    }
}
