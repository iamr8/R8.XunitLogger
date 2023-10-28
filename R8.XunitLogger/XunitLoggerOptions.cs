using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

namespace R8.XunitLogger
{
    /// <summary>
    /// A class to options for simple Xunit logging.
    /// </summary>
    public class XunitLoggerOptions : XunitLogOptions
    {
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