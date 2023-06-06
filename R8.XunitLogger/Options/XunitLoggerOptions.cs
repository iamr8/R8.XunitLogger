using Microsoft.Extensions.Configuration;

namespace R8.XunitLogger.Options
{
    public class XunitLoggerOptions : XunitLogOptions
    {
        /// <summary>
        /// Get or set the <see cref="System.IServiceProvider" />. If <see cref="IConfiguration" /> is provided, this value will be overridden by <see cref="Microsoft.Extensions.Logging.LogLevel" /> defined in configuration.
        /// </summary>
        public IServiceProvider? ServiceProvider { get; set; } = null;
    
        /// <summary>
        /// Get or set a list of categories (full qualified names) to be logged. If <see cref="IConfiguration" /> is provided, this value will be overridden by <see cref="Microsoft.Extensions.Logging.LogLevel" /> defined in configuration.
        /// </summary>
        public IEnumerable<string>? Categories { get; set; }
    }
}