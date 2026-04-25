using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace R8.XunitLogger
{
    /// <summary>
    /// Options for Xunit logging.
    /// </summary>
    public class XunitLoggerOptions
    {
        /// <summary>
        /// Gets or sets the minimum level of log messages to be logged. If <see cref="IConfiguration" /> is provided,
        /// this value will be overridden by <see cref="Microsoft.Extensions.Logging.LogLevel" /> defined in configuration.
        /// </summary>
        /// <remarks>Default is <see cref="Microsoft.Extensions.Logging.LogLevel.Information" />.</remarks>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Gets or sets a value indicating whether to include timestamp in log messages.
        /// </summary>
        /// <remarks>Default is <see langword="true" />.</remarks>
        public bool IncludeTimestamp { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include scopes in log messages.
        /// </summary>
        /// <remarks>Default is <see langword="false" />.</remarks>
        public bool IncludeScopes { get; set; } = false;

        /// <summary>
        /// Gets or sets the <see cref="System.IServiceProvider" /> used to resolve <see cref="IConfiguration" />
        /// for log-level configuration. When set, the <see cref="MinimumLevel" /> may be overridden by
        /// <see cref="Microsoft.Extensions.Logging.LogLevel" /> defined in configuration.
        /// </summary>
        public IServiceProvider? ServiceProvider { get; set; } = null;

        /// <summary>
        /// Gets or sets a list of category name prefixes (fully-qualified) to allow logging.
        /// When non-empty and no <see cref="IConfiguration" /> is available, only categories whose
        /// name starts with one of these prefixes are logged at <see cref="MinimumLevel" />; all others
        /// are silenced.
        /// </summary>
        /// <remarks>
        /// Prefer <see cref="SetMinimumLevel" /> which lets you assign a specific <see cref="LogLevel"/>
        /// per namespace prefix and supports fluent chaining.
        /// </remarks>
        [Obsolete("Use SetMinimumLevel(sourceContext, level) to configure per-namespace levels with fluent chaining.")]
        public IList<string> Categories { get; set; } = new List<string>();

        // Internal storage for per-namespace overrides set via SetMinimumLevel.
        private readonly Dictionary<string, LogLevel> _overrides = new Dictionary<string, LogLevel>(StringComparer.Ordinal);

        /// <summary>
        /// Gets the per-namespace level overrides configured via <see cref="SetMinimumLevel" />.
        /// </summary>
        internal IReadOnlyDictionary<string, LogLevel> Overrides => _overrides;

        /// <summary>
        /// Overrides the minimum log level for log categories whose name starts with
        /// <paramref name="sourceContext" /> and returns a <see cref="XunitLoggerLevelChain" />
        /// that exposes only <c>SetMinimumLevel</c> for further chaining.
        /// When multiple overrides match a category, the longest (most specific) prefix wins.
        /// <see cref="MinimumLevel" /> remains in effect as the global default for
        /// namespaces that have no matching override.
        /// </summary>
        /// <param name="sourceContext">
        /// A category name prefix, e.g. <c>"MyApp.Services"</c> or <c>"Microsoft"</c>.
        /// </param>
        /// <param name="level">The minimum <see cref="LogLevel" /> for that prefix.</param>
        /// <returns>
        /// A <see cref="XunitLoggerLevelChain" /> for stacking additional
        /// <c>SetMinimumLevel</c> calls.
        /// </returns>
        /// <example>
        /// <code>
        /// options
        ///     .SetMinimumLevel("MyApp", LogLevel.Debug)
        ///     .SetMinimumLevel("MyApp.Data", LogLevel.Warning)
        ///     .SetMinimumLevel("Microsoft", LogLevel.None);
        /// </code>
        /// </example>
        public XunitLoggerLevelChain SetMinimumLevel(string sourceContext, LogLevel level)
        {
            if (string.IsNullOrEmpty(sourceContext))
                throw new ArgumentNullException(nameof(sourceContext));

            _overrides[sourceContext] = level;
            return new XunitLoggerLevelChain(_overrides);
        }
    }
}