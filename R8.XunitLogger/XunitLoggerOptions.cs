using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace R8.XunitLogger
{
    /// <summary>
    /// A class to options for simple Xunit logging.
    /// </summary>
    public class XunitLoggerOptions
    {
        /// <summary>
        /// Get or set a value representing the minimum level of log messages to be logged. If <see cref="IConfiguration" /> is provided, this value will be overridden by <see cref="Microsoft.Extensions.Logging.LogLevel" /> defined in configuration.
        /// </summary>
        /// <remarks>Default is <see cref="Microsoft.Extensions.Logging.LogLevel.Information" />.</remarks>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Get or set a value indicating whether to include timestamp in log messages.
        /// </summary>
        /// <remarks>Default is <see langword="true" />.</remarks>
        public bool IncludeTimestamp { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether to include scopes in log messages.
        /// </summary>
        /// <remarks>Default is <see langword="false" />.</remarks>
        public bool IncludeScopes { get; set; } = false;
        
        /// <summary>
        /// Gets or sets a value indicating whether to use color formatter for log levels.
        /// </summary>
        /// <remarks>Default is <see cref="LoggerColorBehavior.Default" />.</remarks>
        public LoggerColorBehavior ColorBehavior { get; set; } = LoggerColorBehavior.Default;
        
        /// <summary>
        /// Get or set the <see cref="System.IServiceProvider" />. If <see cref="IConfiguration" /> is provided, this value will be overridden by <see cref="Microsoft.Extensions.Logging.LogLevel" /> defined in configuration.
        /// </summary>
        public IServiceProvider? ServiceProvider { get; set; } = null;

        /// <summary>
        /// Get or set a list of categories (full qualified names) to be logged. If <see cref="IConfiguration" /> is provided, this value will be overridden by <see cref="Microsoft.Extensions.Logging.LogLevel" /> defined in configuration.
        /// </summary>
        public IList<string> Categories { get; set; } = new List<string>();
    }
}