using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace R8.XunitLogger
{
    /// <summary>
    /// A fluent chain returned by <see cref="XunitLoggerOptions.SetMinimumLevel" /> that allows
    /// stacking multiple per-namespace level overrides without exposing the rest of
    /// <see cref="XunitLoggerOptions" />.
    /// </summary>
    /// <example>
    /// <code>
    /// options
    ///     .SetMinimumLevel("MyApp", LogLevel.Debug)
    ///     .SetMinimumLevel("MyApp.Data", LogLevel.Warning)
    ///     .SetMinimumLevel("Microsoft", LogLevel.None);
    /// </code>
    /// </example>
    public sealed class XunitLoggerLevelChain
    {
        private readonly Dictionary<string, LogLevel> _overrides;

        internal XunitLoggerLevelChain(Dictionary<string, LogLevel> overrides)
        {
            _overrides = overrides;
        }

        /// <summary>
        /// Overrides the minimum log level for log categories whose name starts with
        /// <paramref name="sourceContext" />.
        /// When multiple overrides match a category, the longest (most specific) prefix wins.
        /// </summary>
        /// <param name="sourceContext">
        /// A category name prefix, e.g. <c>"MyApp.Services"</c> or <c>"Microsoft"</c>.
        /// </param>
        /// <param name="level">The minimum <see cref="LogLevel" /> for that prefix.</param>
        /// <returns>The same <see cref="XunitLoggerLevelChain" /> instance for further chaining.</returns>
        public XunitLoggerLevelChain SetMinimumLevel(string sourceContext, LogLevel level)
        {
            if (string.IsNullOrEmpty(sourceContext))
                throw new ArgumentNullException(nameof(sourceContext));

            _overrides[sourceContext] = level;
            return this;
        }
    }
}

