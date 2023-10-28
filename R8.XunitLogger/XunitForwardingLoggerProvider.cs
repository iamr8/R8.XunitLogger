using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace R8.XunitLogger
{
    public static class XunitForwardingLoggerExtensions
    {
        /// <summary>
        /// Adds Xunit logging service to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="onLog">An action to be invoked when a log message is logged. It's preferred to use <see cref="IXunitForwardingLogProvider.WriteLine"/>.</param>
        /// <param name="options">The options to configure the logger.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddXunitForwardingLoggerProvider(this IServiceCollection services, LogDelegate? onLog, Action<XunitForwardingLoggerOptions>? options = null)
        {
            var opt = new XunitForwardingLoggerOptions();
            options?.Invoke(opt);

            services.Replace(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>(sp =>
            {
                var loggerFactory = new LoggerFactory();
                loggerFactory.AddProvider(new XunitForwardingLoggerProvider(sp, onLog, opt));
                return loggerFactory;
            }));

            return services;
        }

        private class XunitForwardingLoggerProvider : ILoggerProvider
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly LogDelegate? _onLog;
            private readonly XunitForwardingLoggerOptions _options;

            public XunitForwardingLoggerProvider(IServiceProvider serviceProvider, LogDelegate? onLog, XunitForwardingLoggerOptions options)
            {
                _serviceProvider = serviceProvider;
                _onLog = onLog;
                _options = options;
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new XunitForwardingLogger(_serviceProvider, categoryName, _onLog, _options);
            }

            public void Dispose()
            {
            }
        }

        private class XunitForwardingLogger : ILogger, IDisposable
        {
            private readonly string _categoryName;
            private readonly LogDelegate? _onLog;
            private readonly LogLevel _minLevel;
            private readonly bool _includeTimestamp;
            private readonly bool _colorize;

            public XunitForwardingLogger(IServiceProvider serviceProvider, string categoryName, LogDelegate? onLog, XunitForwardingLoggerOptions options)
            {
                _categoryName = categoryName;
                _onLog = onLog;
                _includeTimestamp = options.IncludeTimestamp;
                _minLevel = LogProviderHelper.GetMinimumLevel(serviceProvider, _categoryName, options.MinLevel, new List<string>());
                _colorize = options.EnableColors;
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
            
                _onLog?.Invoke(log);
            }
            
            public void Dispose()
            {
            }
        }
    }
}