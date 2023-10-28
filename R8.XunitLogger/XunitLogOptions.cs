using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace R8.XunitLogger
{
    /// <summary>
    /// A base class to options for Xunit logging.
    /// </summary>
    public abstract class XunitLogOptions
    {
        /// <summary>
        /// Get or set a value representing the minimum level of log messages to be logged. If <see cref="IConfiguration" /> is provided, this value will be overridden by <see cref="Microsoft.Extensions.Logging.LogLevel" /> defined in configuration.
        /// </summary>
        /// <remarks>Default is <see cref="Microsoft.Extensions.Logging.LogLevel.Information" />.</remarks>
        public LogLevel MinLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Get or set a value indicating whether to include timestamp in log messages.
        /// </summary>
        /// <remarks>Default is <see langword="true" />.</remarks>
        public bool IncludeTimestamp { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether to use color formatter for log levels.
        /// </summary>
        /// <remarks>Default is <see langword="true" />.</remarks>
        public bool EnableColors { get; set; } = true;
    }
}