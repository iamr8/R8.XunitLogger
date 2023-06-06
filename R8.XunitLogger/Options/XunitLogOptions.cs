using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace R8.XunitLogger.Options
{
    public abstract class XunitLogOptions
    {
        /// <summary>
        /// Get or set a value representing the minimum level of log messages to be logged. If <see cref="IConfiguration" /> is provided, this value will be overridden by <see cref="Microsoft.Extensions.Logging.LogLevel" /> defined in configuration.
        /// </summary>
        public LogLevel MinLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Get or set a value indicating whether to include timestamp in log messages.
        /// </summary>
        public bool IncludeTimestamp { get; set; } = true;
    }
}