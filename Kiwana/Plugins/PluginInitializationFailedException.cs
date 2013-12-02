using System;

namespace Kiwana.Plugins
{
    [Serializable]
    public class PluginInitializationFailedException : Exception
    {
        public PluginInitializationFailedException() { }

        public PluginInitializationFailedException(string message) : base(message) { }

        public PluginInitializationFailedException(string message, Exception inner) : base(message, inner) { }
        
        protected PluginInitializationFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
