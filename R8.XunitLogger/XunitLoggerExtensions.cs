using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace R8.XunitLogger
{
    public static class XunitLoggerExtensions
    {
        /// <summary>
        /// Adds Xunit logging service to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="onWriteLine">An action to be invoked when a log message is logged. It's preferred to use <see cref="IXunitLogProvider.WriteLine"/>.</param>
        /// <param name="options">The options to configure the logger.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddXunitLogger(this IServiceCollection services, Action<string> onWriteLine, Action<XunitLoggerOptions>? options = null)
        {
            var opt = new XunitLoggerOptions();
            options?.Invoke(opt);

            services.Replace(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>(sp =>
            {
                var loggerFactory = new LoggerFactory();
                opt.ServiceProvider = sp;
                loggerFactory.AddProvider(new XunitLoggerProvider(onWriteLine, opt));
                return loggerFactory;
            }));

            return services;
        }

        /// <summary>
        /// Adds an xunit logger named 'Xunit' to the factory.
        /// </summary>
        /// <param name="factory">A reference to <see cref="Microsoft.Extensions.Logging.ILoggerFactory" />.</param>
        /// <param name="outputHelper">The <see cref="Xunit.Abstractions.ITestOutputHelper" />. To get this instance, you need to add a constructor argument of type <see cref="Xunit.Abstractions.ITestOutputHelper" /> to your test class.</param>
        /// <param name="options">An action to be invoked to configure the logger options.</param>
        /// <remarks>This approach is not recommended for integration tests. Use <see cref="AddXunitLogger"/> instead.</remarks>
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
    }
}