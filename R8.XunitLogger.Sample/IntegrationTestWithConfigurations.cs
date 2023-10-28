using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace R8.XunitLogger.Sample
{
    public class IntegrationTestWithConfigurations : IXunitForwardingLogProvider, IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        private DummyObj Dummy => _serviceProvider.GetRequiredService<DummyObj>();

        public IntegrationTestWithConfigurations(ITestOutputHelper outputHelper)
        {
            this._serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(sp => new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .Build())
                .AddLogging()
                .AddXunitForwardingLoggerProvider(WriteLine, options => options.MinLevel = LogLevel.Debug)
                .AddScoped<DummyObj>()
                .BuildServiceProvider();
            this.OnWriteLine += outputHelper.WriteLine;
        }

        public event LogDelegate? OnWriteLine;
        public void WriteLine(string message) => OnWriteLine?.Invoke(message);

        public void Dispose()
        {
            this.OnWriteLine -= OnWriteLine;
        }

        [Fact]
        public void Test1()
        {
            // Act
            Dummy.Test();

            // Assert
            Assert.True(true);
        }
    }
}