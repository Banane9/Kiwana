using System;

namespace Kiwana.Plugins
{
    [Serializable]
    public class PluginLoadFailedException : Exception
    {
        public PluginLoadFailedException() { }

        public PluginLoadFailedException(string message) : base(message) { }

        public PluginLoadFailedException(string message, Exception inner) : base(message, inner) { }
        
        protected PluginLoadFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
