using Microsoft.Extensions.Logging;
using R8.XunitLogger.Options;
using Xunit.Abstractions;

namespace R8.XunitLogger
{
    public static class XunitLoggerExtensions
    {
        /// <summary>
        /// Adds an xunit logger named 'Xunit' to the factory.
        /// </summary>
        /// <param name="factory">A reference to <see cref="Microsoft.Extensions.Logging.ILoggerFactory" />.</param>
        /// <param name="outputHelper">The <see cref="Xunit.Abstractions.ITestOutputHelper" />. To get this instance, you need to add a constructor argument of type <see cref="Xunit.Abstractions.ITestOutputHelper" /> to your test class.</param>
        /// <param name="options">An action to be invoked to configure the logger options.</param>
        /// <remarks>This approach is not recommended for integration tests. Use <see cref="XunitForwardingLoggerExtensions.AddXunitForwardingLoggerProvider"/> instead.</remarks>
        /// <exception cref="ArgumentNullException">When <paramref name="outputHelper" /> is <see langword="null" />.</exception>
        /// <returns>The <see cref="T:Microsoft.Extensions.Logging.ILoggingBuilder" />.</returns>
        public static ILoggerFactory AddXunit(this ILoggerFactory factory, ITestOutputHelper outputHelper, Action<XunitLoggerOptions>? options = null)
        {
            if (outputHelper == null) 
                throw new ArgumentNullException(nameof(outputHelper));
        
            var opt = new XunitLoggerOptions();
            options?.Invoke(opt);
        
            var categories = new List<string>();
            opt.Categories?.Invoke(categories);
            factory.AddProvider(new XunitLoggerProvider(opt.ServiceProvider, outputHelper, opt.MinLevel, opt.IncludeTimestamp, categories));
            return factory;
        }

        private class XunitLoggerProvider : ILoggerProvider
        {
            private readonly IServiceProvider? _serviceProvider;
            private readonly ITestOutputHelper _output;
            private readonly LogLevel _minLevel;
            private readonly bool _includeTimestamp;
            private readonly List<string> _categories;

            public XunitLoggerProvider(IServiceProvider? serviceProvider, ITestOutputHelper output, LogLevel minLevel, bool includeTimestamp, List<string> categories)
            {
                _serviceProvider = serviceProvider;
                _output = output;
                _minLevel = minLevel;
                _includeTimestamp = includeTimestamp;
                _categories = categories;
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new XunitLogger(categoryName, _serviceProvider, _output, _minLevel, _includeTimestamp, _categories);
            }

            public void Dispose()
            {
            }

            private class XunitLogger : ILogger, IDisposable
            {
                private readonly string _categoryName;
                private readonly ITestOutputHelper _output;
                private readonly LogLevel _minLevel;
                private readonly bool _includeTimestamp;

                public XunitLogger(string categoryName, IServiceProvider? serviceProvider, ITestOutputHelper output, LogLevel minLevel, bool includeTimestamp, List<string> categories)
                {
                    _categoryName = categoryName;
                    _output = output;
                    _includeTimestamp = includeTimestamp;
                    _minLevel = serviceProvider != null ? LogProviderHelper.GetMinimumLevel(serviceProvider, _categoryName, minLevel, categories) : minLevel;
                }

                public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

                public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
                {
                    if (!IsEnabled(logLevel))
                        return;

                    var log = LogProviderHelper.FormatLog(_categoryName, _includeTimestamp, logLevel, state, exception, formatter);
                    if (string.IsNullOrWhiteSpace(log))
                        return;

                    _output?.WriteLine(log);
                }
            
                public void Dispose()
                {
                }
            }
        }
    }
}