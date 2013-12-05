using System;

namespace Kiwana.Plugins
{
    /// <summary>
    /// Exception for a failed plugin that failed to load.
    /// </summary>
    [Serializable]
    public class PluginLoadFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoadFailedException"/> class.
        /// </summary>
        public PluginLoadFailedException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoadFailedException"/> class.
        /// </summary>
        /// <param name="message">The message of the <see cref="Exception"/>.</param>
        public PluginLoadFailedException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoadFailedException"/> class.
        /// </summary>
        /// <param name="message">The messsage of the <see cref="Exception"/>.</param>
        /// <param name="inner">The inner <see cref="Exception"/>.</param>
        public PluginLoadFailedException(string message, Exception inner) : base(message, inner) { }
        
        protected PluginLoadFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
