using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
#if NET8_0_OR_GREATER
using Xunit;
#else
using Xunit.Abstractions;
#endif

namespace R8.XunitLogger
{
    internal class XunitLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly ITestOutputHelper? _output;
        private readonly Action<string>? _onLog;
        private readonly XunitLoggerOptions _options;

        // Cache one ILogger instance per category name so CreateLogger is idempotent and cheap.
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>(StringComparer.Ordinal);

        public XunitLoggerProvider(Action<string>? onLog, XunitLoggerOptions options)
        {
            _serviceProvider = options.ServiceProvider;
            _onLog = onLog;
            _options = options;
        }

        public XunitLoggerProvider(ITestOutputHelper output, XunitLoggerOptions options)
        {
            _serviceProvider = options.ServiceProvider;
            _output = output;
            _options = options;
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new XunitLoggerInstance(name, _serviceProvider, _onLog, _output, _options));

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}