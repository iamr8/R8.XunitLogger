using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace R8.XunitLogger.Sample;

public class IntegrationTest : IFixtureLogProvider, IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    private DummyObj Dummy => _serviceProvider.GetRequiredService<DummyObj>();

    public IntegrationTest(ITestOutputHelper outputHelper)
    {
        this._serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddXunitForwardingLoggerProvider(WriteLine)
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