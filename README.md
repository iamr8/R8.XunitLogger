# XunitLogger
A low-config Log Provider for Xunit tests to add more resolution on services

---
## Usage
In the following, you can find some usage examples of this package. However, the full definition of these examples are exist in the [Sample](R8.XunitLogger.Sample) folder.

### Unit Tests
```csharp
using R8.XunitLogger;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class UnitTest
{
    private readonly ILoggerFactory _loggerFactory;

    public UnitTest(ITestOutputHelper outputHelper)
    {
        _loggerFactory = new LoggerFactory().AddXunit(outputHelper, options => options.MinLevel = LogLevel.Debug);
    }

    [Fact]
    public void Test1()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<DummyObj>();
        var dummy = new DummyObj(logger);
        
        // Act
        dummy.Test();
        
        // Assert
        Assert.True(true);
    }
}
```

---

### Integration Tests

####
```csharp
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
```
#### with `IClassFixture`:
```csharp
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
```

#### with a `IConfiguration` service (`appsettings.json`)
```csharp
public class IntegrationTestWithConfigurations : IFixtureLogProvider, IDisposable
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
```

## A _possible issue_ in Integration Tests
*If you're encountering a mixed output of logger, you need to inherit `IDisposable` interface, and remove the event handler, on the same way you added it.*
```csharp
public void Dispose()
{
    this._fixture.OnWriteLine -= _outputHelper.WriteLine;
}
```

---

## Conclusion
This package is a simple implementation of `ILoggerProvider` and `ILogger` interfaces, which is useful for Xunit tests. It's not a replacement for `ILoggerProvider` and `ILogger` interfaces, and it's not a replacement for `ILoggerFactory` and `ILogger<T>` interfaces. It's just a simple implementation of these interfaces, which is useful for Xunit tests.

**#Women_Life_Freedom** :v: