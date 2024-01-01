using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace R8.XunitLogger.Sample
{
    public class IntegrationTestWithConfigurations : IXunitLogProvider, IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ServiceProvider _serviceProvider;

        private DummyObj Dummy => _serviceProvider.GetRequiredService<DummyObj>();

        public IntegrationTestWithConfigurations(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            this._serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(sp => new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .Build())
                .AddLogging()
                .AddXunitLogger(message => OnWriteLine?.Invoke(message), options =>
                {
                    options.MinimumLevel = LogLevel.Debug;
                    options.ColorBehavior = LoggerColorBehavior.Enabled;
                })
                .AddScoped<DummyObj>()
                .BuildServiceProvider();
            this.OnWriteLine += _outputHelper.WriteLine;
        }

        public event Action<string>? OnWriteLine;

        public void Dispose()
        {
            this.OnWriteLine -= _outputHelper.WriteLine;
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