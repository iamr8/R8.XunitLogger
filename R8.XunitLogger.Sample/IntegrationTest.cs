using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace R8.XunitLogger.Sample
{
    public class IntegrationTest : IXunitLogProvider, IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ServiceProvider _serviceProvider;

        private DummyObj Dummy => _serviceProvider.GetRequiredService<DummyObj>();

        public IntegrationTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            this._serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddXunitLogger(message => OnWriteLine?.Invoke(message), o =>
                {
                    o.ColorBehavior = LoggerColorBehavior.Enabled;
                })
                .AddScoped<DummyObj>()
                .BuildServiceProvider();
            this.OnWriteLine += _outputHelper.WriteLine;
        }

        public event Action<string> OnWriteLine;

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