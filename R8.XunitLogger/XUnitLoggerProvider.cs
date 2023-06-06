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
        /// <returns>The <see cref="Microsoft.Extensions.Logging.ILoggingBuilder" />.</returns>
        public static ILoggerFactory AddXunit(this ILoggerFactory factory, ITestOutputHelper outputHelper, Action<XunitLoggerOptions>? options = null)
        {
            if (outputHelper == null) 
                throw new ArgumentNullException(nameof(outputHelper));
        
            var opt = new XunitLoggerOptions();
            options?.Invoke(opt);
            factory.AddProvider(new XunitLoggerProvider(outputHelper, opt));
            return factory;
        }

        private class XunitLoggerProvider : ILoggerProvider
        {
            private readonly IServiceProvider? _serviceProvider;
            private readonly ITestOutputHelper _output;
            private readonly XunitLoggerOptions _options;

            public XunitLoggerProvider(ITestOutputHelper output, XunitLoggerOptions options)
            {
                _serviceProvider = options.ServiceProvider;
                _output = output;
                _options = options;
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new XunitLogger(categoryName, _serviceProvider, _output, _options);
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
                private readonly bool _colorize;

                public XunitLogger(string categoryName, IServiceProvider? serviceProvider, ITestOutputHelper output, XunitLoggerOptions options)
                {
                    _categoryName = categoryName;
                    _output = output;
                    _includeTimestamp = options.IncludeTimestamp;
                    _colorize = options.EnableColors;
                    _minLevel = serviceProvider != null 
                        ? LogProviderHelper.GetMinimumLevel(serviceProvider, _categoryName, options.MinLevel, options.Categories ?? Enumerable.Empty<string>()) 
                        : options.MinLevel;
                }

                public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

                public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                {
                    if (!IsEnabled(logLevel))
                        return;

                    var log = LogProviderHelper.FormatLog(_categoryName, _includeTimestamp, logLevel, state, exception, formatter, _colorize);
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