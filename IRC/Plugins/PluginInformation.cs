using Kiwana.Core.Api;
using Kiwana.Core.Api.Config;
using Kiwana.Core.Config;

namespace Kiwana.Core.Plugins
{
    public class PluginInformation
    {
        public string Name { get; set; }

        public PluginConfig Config { get; set; }

        public Plugin Instance { get; set; }
    }
}
