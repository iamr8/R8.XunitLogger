using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace R8.XunitLogger.Sample;

public class IntegrationTestClassFixture : IClassFixture<IntegrationTestFixture>, IDisposable
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _outputHelper;

    public IntegrationTestClassFixture(IntegrationTestFixture fixture, ITestOutputHelper outputHelper)
    {
        this._fixture = fixture;
        _outputHelper = outputHelper;
        this._fixture.OnWriteLine += _outputHelper.WriteLine;
    }

    public void Dispose()
    {
        this._fixture.OnWriteLine -= _outputHelper.WriteLine;
    }

    [Fact]
    public void Test1()
    {
        // Act
        this._fixture.Dummy.Test();

        // Assert
        Assert.True(true);
    }
}

public class IntegrationTestFixture : IFixtureLogProvider
{
    private readonly ServiceProvider _serviceProvider;

    protected internal DummyObj Dummy => _serviceProvider.GetRequiredService<DummyObj>();

    public IntegrationTestFixture()
    {
        this._serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddXunitForwardingLoggerProvider(WriteLine, options => options.MinLevel = LogLevel.Warning)
            .AddScoped<DummyObj>()
            .BuildServiceProvider();
    }

    public event LogDelegate? OnWriteLine;
    public void WriteLine(string message) => OnWriteLine?.Invoke(message);
}