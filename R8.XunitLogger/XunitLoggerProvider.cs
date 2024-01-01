using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace R8.XunitLogger
{
    internal class XunitLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly ITestOutputHelper? _output;
        private readonly Action<string>? _onLog;
        private readonly XunitLoggerOptions _options;

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

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLoggerInstance(categoryName, _serviceProvider, _onLog, _output, _options);
        }
        
        public void Dispose()
        {
        }
    }
}