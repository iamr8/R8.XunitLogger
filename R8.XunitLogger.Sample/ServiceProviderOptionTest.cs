using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
#if !NET8_0_OR_GREATER
using Xunit.Abstractions;
#endif

namespace R8.XunitLogger.Sample
{
    public class ServiceProviderOptionTest
    {
        private readonly ITestOutputHelper _outputHelper;

        public ServiceProviderOptionTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        /// <summary>
        /// When <see cref="XunitLoggerOptions.ServiceProvider"/> is set manually in a unit test
        /// (via <c>AddXunit</c>), the <c>Logging:LogLevel</c> values from <see cref="IConfiguration"/>
        /// override <see cref="XunitLoggerOptions.MinimumLevel"/> and per-namespace overrides.
        /// The most specific matching prefix wins.
        /// </summary>
        [Fact]
        public void ServiceProvider_UsesConfigurationLogLevels_InUnitTest()
        {
            // Arrange: build a custom IServiceProvider with an in-memory IConfiguration.
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // Most specific: DummyObj namespace → Debug
                    [$"Logging:LogLevel:{typeof(DummyObj).FullName}"] = nameof(LogLevel.Debug),
                    // Fallback for everything else → Warning
                    ["Logging:LogLevel:Default"] = nameof(LogLevel.Warning),
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .BuildServiceProvider();

            // MinimumLevel = Error would normally silence everything below Error,
            // but the IConfiguration supplied via ServiceProvider should take precedence.
            var loggerFactory = new LoggerFactory().AddXunit(_outputHelper, options =>
            {
                options.MinimumLevel = LogLevel.Error; // intentionally high — should be overridden
                options.ServiceProvider = serviceProvider;
            });

            // Act
            var dummyLogger = loggerFactory.CreateLogger<DummyObj>();        // matches DummyObj prefix → Debug
            var otherLogger = loggerFactory.CreateLogger<IntegrationTest>(); // no specific match → Default = Warning

            // Assert — DummyObj: Debug and above should be enabled
            Assert.True(dummyLogger.IsEnabled(LogLevel.Debug),       "DummyObj: Debug should be enabled (config: Debug)");
            Assert.True(dummyLogger.IsEnabled(LogLevel.Information),  "DummyObj: Information should be enabled (config: Debug)");
            Assert.True(dummyLogger.IsEnabled(LogLevel.Warning),      "DummyObj: Warning should be enabled (config: Debug)");
            Assert.True(dummyLogger.IsEnabled(LogLevel.Error),        "DummyObj: Error should be enabled (config: Debug)");

            // Assert — IntegrationTest: only Warning and above (Default = Warning)
            Assert.False(otherLogger.IsEnabled(LogLevel.Debug),       "IntegrationTest: Debug should be disabled (Default: Warning)");
            Assert.False(otherLogger.IsEnabled(LogLevel.Information),  "IntegrationTest: Information should be disabled (Default: Warning)");
            Assert.True(otherLogger.IsEnabled(LogLevel.Warning),      "IntegrationTest: Warning should be enabled (Default: Warning)");
            Assert.True(otherLogger.IsEnabled(LogLevel.Error),        "IntegrationTest: Error should be enabled (Default: Warning)");
        }

        /// <summary>
        /// When <see cref="XunitLoggerOptions.ServiceProvider"/> is <see langword="null"/>,
        /// <see cref="XunitLoggerOptions.MinimumLevel"/> (and any <c>SetMinimumLevel</c> overrides)
        /// control log filtering as normal.
        /// </summary>
        [Fact]
        public void ServiceProvider_WhenNotSet_FallsBackToOptionsMinimumLevel()
        {
            // Arrange: no ServiceProvider — MinimumLevel is the sole filter.
            var loggerFactory = new LoggerFactory().AddXunit(_outputHelper, options =>
            {
                options.MinimumLevel = LogLevel.Warning;
                // options.ServiceProvider intentionally left null
            });

            // Act
            var logger = loggerFactory.CreateLogger<DummyObj>();

            // Assert
            Assert.False(logger.IsEnabled(LogLevel.Trace),       "Trace should be disabled (MinimumLevel: Warning)");
            Assert.False(logger.IsEnabled(LogLevel.Debug),       "Debug should be disabled (MinimumLevel: Warning)");
            Assert.False(logger.IsEnabled(LogLevel.Information),  "Information should be disabled (MinimumLevel: Warning)");
            Assert.True(logger.IsEnabled(LogLevel.Warning),      "Warning should be enabled (MinimumLevel: Warning)");
            Assert.True(logger.IsEnabled(LogLevel.Error),        "Error should be enabled (MinimumLevel: Warning)");
            Assert.True(logger.IsEnabled(LogLevel.Critical),     "Critical should be enabled (MinimumLevel: Warning)");
        }
    }
}

